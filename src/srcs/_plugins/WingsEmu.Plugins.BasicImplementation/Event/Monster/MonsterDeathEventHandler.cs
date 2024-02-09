﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Scripting.Object.Timespace;
using WingsEmu.DTOs.BCards;
using WingsEmu.Game._enum;
using WingsEmu.Game.Act4;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Battle.Managers;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Monster.Event;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Quests.Event;
using WingsEmu.Game.TimeSpaces;
using WingsEmu.Game.TimeSpaces.Events;
using WingsEmu.Game.Triggers;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Plugins.BasicImplementations.Event.Monster;

public class MonsterDeathEventHandler : IAsyncEventProcessor<MonsterDeathEvent>
{
    private readonly IAct4FlagManager _act4FlagManager;
    private readonly IAsyncEventPipeline _asyncEventPipeline;
    private readonly IBCardEffectHandlerContainer _bCardEffectHandlerContainer;
    private readonly IPhantomPositionManager _phantomPositionManager;
    private readonly ITimeSpaceManager _timeSpaceManager;

    public MonsterDeathEventHandler(IBCardEffectHandlerContainer bCardEffectHandlerContainer, IAsyncEventPipeline asyncEventPipeline,
        ITimeSpaceManager timeSpaceManager, IAct4FlagManager act4FlagManager, IPhantomPositionManager phantomPositionManager)
    {
        _bCardEffectHandlerContainer = bCardEffectHandlerContainer;
        _asyncEventPipeline = asyncEventPipeline;
        _timeSpaceManager = timeSpaceManager;
        _act4FlagManager = act4FlagManager;
        _phantomPositionManager = phantomPositionManager;
    }

    public async Task HandleAsync(MonsterDeathEvent e, CancellationToken cancellation)
    {
        IMonsterEntity monsterEntity = e.MonsterEntity;

        DateTime dateTimeNow = DateTime.UtcNow;
        monsterEntity.IsStillAlive = false;
        monsterEntity.Hp = 0;
        monsterEntity.Death = dateTimeNow;
        monsterEntity.Killer = e.Killer;

        if (monsterEntity.SummonerType is not VisualType.Player)
        {
            monsterEntity.MapInstance.IncreaseMonsterDeathsOnMap();
        }

        monsterEntity.MapInstance.DeactivateMode(monsterEntity);

        if (monsterEntity.DeathEffect != 0)
        {
            monsterEntity.BroadcastEffectInRange(monsterEntity.DeathEffect);
        }

        if (monsterEntity.IsPhantom())
        {
            _phantomPositionManager.AddPosition(monsterEntity.UniqueId, monsterEntity.Position);
        }

        await monsterEntity.TriggerEvents(BattleTriggers.OnDeath);
        if (!e.IsByCommand)
        {
            IEnumerable<BCardDTO> triggerBCard = monsterEntity.BCards.Where(b => b.TriggerType == BCardNpcMonsterTriggerType.ON_DEATH);
            foreach (BCardDTO bCard in triggerBCard)
            {
                _bCardEffectHandlerContainer.Execute(monsterEntity, monsterEntity, bCard, triggerType: BCardNpcMonsterTriggerType.ON_DEATH);
            }
        }

        if (monsterEntity.MapInstance.MapInstanceType == MapInstanceType.TimeSpaceInstance)
        {
            await CheckTargetMonstersInRoom(monsterEntity);
            await _asyncEventPipeline.ProcessEventAsync(new TimeSpaceCheckMonsterEvent(monsterEntity), cancellation);
        }

        monsterEntity.Mp = 0;

        switch (e.Killer)
        {
            case IPlayerEntity player:
                await player.Session.EmitEventAsync(new KillBonusEvent
                {
                    MonsterEntity = monsterEntity
                });
                if (player.IsInGroup())
                {
                    foreach (IPlayerEntity member in player.GetGroup().Members.ToArray())
                    {
                        await member.Session.EmitEventAsync(new QuestMonsterDeathEvent { MonsterEntity = monsterEntity });
                    }
                }
                else
                {
                    await player.Session.EmitEventAsync(new QuestMonsterDeathEvent { MonsterEntity = monsterEntity });
                }

                player.HitsByMonsters.TryRemove(monsterEntity.Id, out _);
                break;
            case IMateEntity mate:
                await mate.Owner.Session.EmitEventAsync(new KillBonusEvent
                {
                    MonsterEntity = monsterEntity
                });

                if (mate.Owner.IsInGroup())
                {
                    foreach (IPlayerEntity member in mate.Owner.GetGroup().Members.ToArray())
                    {
                        await member.Session.EmitEventAsync(new QuestMonsterDeathEvent { MonsterEntity = monsterEntity });
                    }
                }
                else
                {
                    await mate.Owner.Session.EmitEventAsync(new QuestMonsterDeathEvent { MonsterEntity = monsterEntity });
                }

                break;
            case IMonsterEntity mapMonster:
                if (!mapMonster.SummonerId.HasValue)
                {
                    break;
                }

                if (mapMonster.IsMateTrainer)
                {
                    break;
                }

                if (mapMonster.SummonerType != VisualType.Player)
                {
                    break;
                }

                IClientSession summoner = mapMonster.MapInstance.GetCharacterById(mapMonster.SummonerId.Value)?.Session;
                if (summoner == null)
                {
                    break;
                }

                if (monsterEntity.MonsterVNum == (short)MonsterVnum.BOMB)
                {
                    summoner.PlayerEntity.SkillComponent.BombEntityId = null;
                }

                await summoner.EmitEventAsync(new KillBonusEvent
                {
                    MonsterEntity = monsterEntity
                });

                if (summoner.PlayerEntity.IsInGroup())
                {
                    foreach (IPlayerEntity member in summoner.PlayerEntity.GetGroup().Members.ToArray())
                    {
                        await member.Session.EmitEventAsync(new QuestMonsterDeathEvent { MonsterEntity = monsterEntity });
                    }
                }
                else
                {
                    await summoner.EmitEventAsync(new QuestMonsterDeathEvent { MonsterEntity = monsterEntity });
                }

                break;
        }

        switch ((MonsterVnum)monsterEntity.MonsterVNum)
        {
            case MonsterVnum.DEMON_CAMP:
                _act4FlagManager.RemoveDemonFlag();
                break;
            case MonsterVnum.ANGEL_CAMP:
                _act4FlagManager.RemoveAngelFlag();
                break;
        }

        await monsterEntity.RemoveAllBuffsAsync(true);
        monsterEntity.MapInstance.ForgetAll(monsterEntity, dateTimeNow);
    }

    private async Task CheckTargetMonstersInRoom(IMonsterEntity monsterEntity)
    {
        if (!monsterEntity.IsTarget)
        {
            return;
        }

        Guid guid = monsterEntity.MapInstance.Id;
        TimeSpaceParty timeSpace = _timeSpaceManager.GetTimeSpaceByMapInstanceId(guid);
        if (timeSpace == null)
        {
            return;
        }

        if (timeSpace.Instance.TimeSpaceObjective.KillMonsterVnum.HasValue && monsterEntity.MonsterVNum == timeSpace.Instance.TimeSpaceObjective.KillMonsterVnum.Value)
        {
            timeSpace.Instance.TimeSpaceObjective.KilledMonsterAmount++;
            await _asyncEventPipeline.ProcessEventAsync(new TimeSpaceRefreshObjectiveProgressEvent
            {
                MapInstanceId = guid
            });
        }

        if (monsterEntity.MapInstance.GetAliveMonsters(x => x.IsTarget).Any())
        {
            return;
        }

        TimeSpaceSubInstance timeSpaceSubInstance = _timeSpaceManager.GetSubInstance(guid);
        if (timeSpaceSubInstance == null)
        {
            return;
        }

        await timeSpaceSubInstance.TriggerEvents(TimespaceConstEventKeys.OnAllTargetMobsDead);
    }
}