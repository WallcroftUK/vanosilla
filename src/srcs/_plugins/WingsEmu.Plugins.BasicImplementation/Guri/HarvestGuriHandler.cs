﻿using System;
using System.Linq;
using System.Threading.Tasks;
using PhoenixLib.Events;
using PhoenixLib.Logging;
using WingsAPI.Data.Drops;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Game.Extensions.ItemExtension.Item;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game._Guri;
using WingsEmu.Game._Guri.Event;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Entities.Extensions;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Npcs.Event;
using WingsEmu.Game.Quests.Event;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace WingsEmu.Plugins.BasicImplementations.Guri;

public class HarverstGuriHandler : IGuriHandler
{
    private readonly IAsyncEventPipeline _asyncEventPipeline;
    private readonly IDelayManager _delayManager;
    private readonly IGameItemInstanceFactory _gameItemInstanceFactory;
    private readonly IGameLanguageService _gameLanguageService;
    private readonly IItemsManager _itemsManager;
    private readonly IRandomGenerator _randomGenerator;

    private readonly IServerManager _serverManager;

    public HarverstGuriHandler(IServerManager serverManager,
        IGameLanguageService gameLanguageService, IRandomGenerator randomGenerator,
        IItemsManager itemsManager, IGameItemInstanceFactory gameItemInstanceFactory, IDelayManager delayManager, IAsyncEventPipeline asyncEventPipeline)
    {
        _randomGenerator = randomGenerator;
        _serverManager = serverManager;
        _gameLanguageService = gameLanguageService;
        _itemsManager = itemsManager;
        _gameItemInstanceFactory = gameItemInstanceFactory;
        _delayManager = delayManager;
        _asyncEventPipeline = asyncEventPipeline;
    }

    public long GuriEffectId => 400;

    public async Task ExecuteAsync(IClientSession session, GuriEvent guriPacket)
    {
        if (guriPacket == null)
        {
            return;
        }

        if (guriPacket.Data == 0)
        {
            return;
        }

        if (!session.HasCurrentMapInstance)
        {
            return;
        }

        INpcEntity npcEntity = session.CurrentMapInstance.GetNpcById(guriPacket.Data);
        if (npcEntity == null)
        {
            Log.Debug($"Npc not found. GuriEffectId : {GuriEffectId}");
            return;
        }

        if (!npcEntity.IsAlive())
        {
            await session.NotifyStrangeBehavior(StrangeBehaviorSeverity.ABUSING, "[ITEM_HARVEST] Tried to harvest a dead NPC.");
            return;
        }

        if (!npcEntity.IsInRange(session.PlayerEntity.PositionX, session.PlayerEntity.PositionY, 5))
        {
            await session.NotifyStrangeBehavior(StrangeBehaviorSeverity.ABUSING, "[ITEM_HARVEST] Tried to harvest an NPC that is too far away.");
            return;
        }

        if (!await _delayManager.CanPerformAction(session.PlayerEntity, DelayedActionType.Mining))
        {
            return;
        }

        await _delayManager.CompleteAction(session.PlayerEntity, DelayedActionType.Mining);

        if (npcEntity.CurrentCollection <= 0)
        {
            int delayTime = (int)(npcEntity.LastCollection - DateTime.UtcNow).TotalSeconds;
            session.SendMsg(_gameLanguageService.GetLanguageFormat(GameDialogKey.HARVEST_SHOUTMESSAGE_FAIL_TRY_AGAIN, session.UserLanguage, delayTime), MsgMessageType.Middle);
            return;
        }

        if (npcEntity.VNumRequired != 0 && npcEntity.AmountRequired != 0)
        {
            if (!session.PlayerEntity.HasItem(npcEntity.VNumRequired, npcEntity.AmountRequired))
            {
                string itemName = _itemsManager.GetItem(npcEntity.VNumRequired).GetItemName(_gameLanguageService, session.UserLanguage);
                session.SendMsg(_gameLanguageService.GetLanguageFormat(GameDialogKey.INVENTORY_SHOUTMESSAGE_NOT_ENOUGH_ITEMS, session.UserLanguage, npcEntity.AmountRequired, itemName),
                    MsgMessageType.Middle);
                return;
            }
        }

        double chance = _randomGenerator.RandomNumber(0, 100_000);
        DropDTO drop;

        if (npcEntity.Drops.Count <= 1)
        {
            drop = npcEntity.Drops.FirstOrDefault(s => s.MonsterVNum == npcEntity.NpcVNum);

            if (drop != null && chance > drop.DropChance)
            {
                session.SendMsg(_gameLanguageService.GetLanguage(GameDialogKey.HARVEST_SHOUTMESSAGE_TRY_FAIL, session.UserLanguage), MsgMessageType.Middle);
                return;
            }
        }
        else
        {
            var randomBag = new RandomBag<DropDTO>(_randomGenerator);
            foreach (DropDTO dropDto in npcEntity.Drops)
            {
                randomBag.AddEntry(dropDto, dropDto.DropChance);
            }

            drop = randomBag.GetRandom();
        }

        if (drop == null)
        {
            session.SendMsg(_gameLanguageService.GetLanguage(GameDialogKey.HARVEST_SHOUTMESSAGE_TRY_FAIL, session.UserLanguage), MsgMessageType.Middle);
            return;
        }

        int vnum = drop.ItemVNum;

        if (npcEntity.VNumRequired != 0 && npcEntity.AmountRequired != 0)
        {
            await session.RemoveItemFromInventory(npcEntity.VNumRequired, npcEntity.AmountRequired);
        }

        npcEntity.CurrentCollection--;

        GameItemInstance newItem = _gameItemInstanceFactory.CreateItem(vnum, drop.Amount);
        session.SendMsg(
            _gameLanguageService.GetLanguageFormat(GameDialogKey.INVENTORY_CHATMESSAGE_X_ITEM_ACQUIRED, session.UserLanguage, newItem.Amount.ToString(),
                _gameLanguageService.GetItemName(newItem.GameItem, session)), MsgMessageType.Middle);

        switch ((MonsterVnum)npcEntity.NpcVNum)
        {
            case MonsterVnum.POISON_PLANT_OF_DAMNATION:
            case MonsterVnum.ICE_FLOWER:
                npcEntity.MapInstance.Broadcast(npcEntity.GenerateOut());
                await _asyncEventPipeline.ProcessEventAsync(new MapNpcGenerateDeathEvent(npcEntity, null));
                Position newPosition = npcEntity.MapInstance.GetRandomPosition();
                npcEntity.FirstX = newPosition.X;
                npcEntity.FirstY = newPosition.Y;
                break;
            case MonsterVnum.ROBBER_GANG_CHEST:
                npcEntity.MapInstance.Broadcast(npcEntity.GenerateOut());
                await _asyncEventPipeline.ProcessEventAsync(new MapNpcGenerateDeathEvent(npcEntity, null));
                break;
        }

        // Quest logic
        bool isHarvestObjective = session.PlayerEntity.GetCurrentQuestsByType(QuestType.COLLECT)
            .Any(s => s.Quest.Objectives.Any(o =>
                newItem.ItemVNum == o.Data1 && npcEntity.NpcVNum == o.Data0 && s.ObjectiveAmount[o.ObjectiveIndex].CurrentAmount <= s.ObjectiveAmount[o.ObjectiveIndex].RequiredAmount));

        if (!isHarvestObjective)
        {
            await session.AddNewItemToInventory(newItem, true, sendGiftIsFull: true);
            return;
        }

        if (session.PlayerEntity.IsInGroup())
        {
            foreach (IPlayerEntity member in session.PlayerEntity.GetGroup().Members)
            {
                await member.Session.EmitEventAsync(new QuestHarvestEvent
                {
                    ItemVnum = newItem.ItemVNum,
                    NpcVnum = npcEntity.NpcVNum
                });
            }
        }
        else
        {
            await session.EmitEventAsync(new QuestHarvestEvent
            {
                ItemVnum = newItem.ItemVNum,
                NpcVnum = npcEntity.NpcVNum
            });
        }
    }
}