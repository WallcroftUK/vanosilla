// WingsEmu
// 
// Developed by NosWings Team[403]

using WingsEmu.DTOs.BCards;
using WingsEmu.Game._enum;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Monster;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Plugins.BasicImplementations.BCards.Handlers;

public class BCardLightAndShadowHandler : IBCardEffectAsyncHandler
{
    public BCardType HandledType => BCardType.LightAndShadow;

    public void Execute(IBCardEffectContext ctx)
    {
        IBattleEntity sender = ctx.Target;
        IBattleEntity target = ctx.Target;
        BCardDTO bCard = ctx.BCard;

        int firstData = bCard.FirstDataValue(target.Level);

        switch ((AdditionalTypes.LightAndShadow)bCard.SubType)
        {
            case AdditionalTypes.LightAndShadow.RemoveBadEffects:
                ctx.Target.RemoveNegativeBuffs(firstData);
                break;
            case AdditionalTypes.LightAndShadow.RemoveGoodEffects:
                ctx.Target.RemovePositiveBuffs(firstData);
                break;
            case AdditionalTypes.LightAndShadow.InflictDamageOnUndead:

                if (target is not IMonsterEntity monsterEntity)
                {
                    return;
                }

                if (!monsterEntity.IsAlive())
                {
                    return;
                }

                if (sender.Level < monsterEntity.Level)
                {
                    return;
                }

                if (monsterEntity.MonsterRaceType != MonsterRaceType.Undead)
                {
                    return;
                }

                if (!Equals(monsterEntity.GetMonsterRaceSubType(), MonsterSubRace.Undead.LowLevelUndead))
                {
                    return;
                }

                monsterEntity.Hp /= 2;
                if (monsterEntity.Hp <= 0)
                {
                    monsterEntity.Hp = 1;
                }

                if (sender is not IPlayerEntity playerEntity)
                {
                    return;
                }

                playerEntity.Session.SendStPacket(monsterEntity);

                break;
        }
    }
}