// WingsEmu
// 
// Developed by NosWings Team

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WingsAPI.Data.Families;
using WingsAPI.Packets.Enums.Shells;
using WingsEmu.Core.Extensions;
using WingsEmu.DTOs.BCards;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Extensions.Mates;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Inventory.Event;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Monster;
using WingsEmu.Game.RainbowBattle.Event;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace WingsEmu.Game.Battle;

public static class BattleEntityDumpExtensions
{
    private static readonly double[] _skillRankMultiplier = { 0, 0.3, 0.5, 0.8, 1, 1.2, 1.5, 2.5 };
    private static IRandomGenerator _randomGenerator => StaticRandomGenerator.Instance;

    public static bool HasBuff(this IBattleEntityDump entity, int cardId)
    {
        if (!entity.BuffsById.Any())
        {
            return false;
        }

        return entity.BuffsById.Contains(cardId);
    }

    public static bool IsPlayer(this IEntity entity) => entity.Type == VisualType.Player;

    public static bool IsAlive(this IBattleEntity battleEntity) => battleEntity.Hp > 0;

    public static bool IsMonster(this IEntity entity) => entity.Type == VisualType.Monster;

    public static bool IsMate(this IEntity entity) => entity is IMateEntity;

    public static bool IsNpc(this IEntity entity) => entity is INpcEntity;

    public static bool IsPlayer(this IBattleEntityDump entity) => entity.Type == VisualType.Player;

    public static bool IsMonster(this IBattleEntityDump entity) => entity.Type == VisualType.Monster;

    public static bool IsMate(this IBattleEntityDump entity) => entity is MateBattleEntityDump;

    public static (int firstData, int secondData, int count) GetBCardInformation(this IBattleEntityDump attacker, BCardType type, byte subtype)
    {
        int count = 0;

        (int firstData, int secondData) = GetBuff(attacker, type, subtype, ref count);
        return (firstData, secondData, count);
    }

    public static (int firstData, int secondData, int count) TryFindPartnerSkillInformation(this IBattleEntityDump attacker, BCardType type, byte subtype, SkillInfo skillInfo)
    {
        if (!attacker.IsMate())
        {
            return GetBCardInformation(attacker, type, subtype);
        }

        if (!skillInfo.PartnerSkillRank.HasValue)
        {
            return GetBCardInformation(attacker, type, subtype);
        }

        int count = 0;

        int firstData = 0;
        int secondData = 0;

        if (attacker.BCards.TryGetValue((type, subtype), out List<BCardDTO> allBCardsExcept))
        {
            HashSet<Guid> bCardsToIgnore = new();
            foreach (BCardDTO bCard in allBCardsExcept)
            {
                foreach (BCardDTO skillBCard in skillInfo.BCards.Where(skillBCard => bCard.Id == skillBCard.Id))
                {
                    if (!bCardsToIgnore.Contains(skillBCard.Id))
                    {
                        bCardsToIgnore.Add(skillBCard.Id);
                    }

                    int firstDataSkillValue = (int)(bCard.FirstData * _skillRankMultiplier[skillInfo.PartnerSkillRank.Value]);
                    int secondDataSkillValue = (int)(bCard.SecondData * _skillRankMultiplier[skillInfo.PartnerSkillRank.Value]);

                    firstData += firstDataSkillValue;
                    secondData += secondDataSkillValue;
                    count++;
                }

                if (bCardsToIgnore.Contains(bCard.Id))
                {
                    continue;
                }

                int firstDataValue = bCard.FirstDataValue(attacker.Level);
                int secondDataValue = bCard.SecondDataValue(attacker.Level);

                firstData += firstDataValue;
                secondData += secondDataValue;
                count++;
            }
        }

        if (!attacker.BuffBCards.TryGetValue((type, subtype), out List<(int casterLevel, BCardDTO bCard)> getBuffs))
        {
            return (firstData, secondData, count);
        }

        foreach ((int casterLevel, BCardDTO bCard) in getBuffs)
        {
            int firstDataValue = bCard.FirstDataValue(casterLevel);
            int secondDataValue = bCard.SecondDataValue(casterLevel);

            firstData += firstDataValue;
            secondData += secondDataValue;
            count++;
        }

        return (firstData, secondData, count);
    }

    public static double GetMultiplier(this IBattleEntityDump entity, int data) => data * 0.01;

    private static (int firstData, int secondData) GetBuff(IBattleEntityDump entity, BCardType type, byte subtype, ref int count)
    {
        int firstData = 0;
        int secondData = 0;

        if (entity.BCards.TryGetValue((type, subtype), out List<BCardDTO> allBCardsExcept))
        {
            foreach (BCardDTO bCard in allBCardsExcept)
            {
                int firstDataValue = bCard.FirstDataValue(entity.Level);
                int secondDataValue = bCard.SecondDataValue(entity.Level);

                firstData += firstDataValue;
                secondData += secondDataValue;
                count++;
            }
        }

        if (!entity.BuffBCards.TryGetValue((type, subtype), out List<(int casterLevel, BCardDTO bCard)> getBuffs))
        {
            return (firstData, secondData);
        }

        foreach ((int casterLevel, BCardDTO bCard) in getBuffs)
        {
            int firstDataValue = bCard.FirstDataValue(casterLevel);
            int secondDataValue = bCard.SecondDataValue(casterLevel);

            firstData += firstDataValue;
            secondData += secondDataValue;
            count++;
        }

        return (firstData, secondData);
    }

    public static bool HasBCard(this IBattleEntityDump entity, BCardType type, byte subType)
    {
        if (!entity.BCards.Any() && !entity.BuffBCards.Any())
        {
            return false;
        }

        return entity.BCards.ContainsKey((type, subType)) || entity.BuffBCards.ContainsKey((type, subType));
    }

    public static void BroadcastEffect(this IBattleEntityDump dump, EffectType effectType)
    {
        dump.MapInstance?.Broadcast($"eff {(byte)dump.Type} {dump.Id} {(short)effectType}");
    }

    public static bool IsInvisible(this IBattleEntityDump entity) => entity.HasBCard(BCardType.SpecialActions, (byte)AdditionalTypes.SpecialActions.Hide);

    public static bool IsSucceededChance(this IBattleEntityDump entity, int chance) => _randomGenerator.RandomNumber() <= chance;

    public static Enum GetRaceSubType(this IBattleEntityDump entity, MonsterRaceType type, byte value)
    {
        return type switch
        {
            MonsterRaceType.LowLevel => (MonsterSubRace.LowLevel)value,
            MonsterRaceType.HighLevel => (MonsterSubRace.HighLevel)value,
            MonsterRaceType.Furry => (MonsterSubRace.Furry)value,
            MonsterRaceType.People => (MonsterSubRace.People)value,
            MonsterRaceType.Angel => (MonsterSubRace.Angels)value,
            MonsterRaceType.Undead => (MonsterSubRace.Undead)value,
            MonsterRaceType.Spirit => (MonsterSubRace.Spirits)value,
            MonsterRaceType.Other => (MonsterSubRace.Other)value,
            MonsterRaceType.Fixed => (MonsterSubRace.Fixed)value,
            _ => null
        };
    }

    public static int GetFamilyUpgradeValue(this IBattleEntityDump entity, FamilyUpgradeType familyUpgradeType) => entity.FamilyUpgrades.GetValueOrDefault(familyUpgradeType);

    public static int GetShellWeaponEffectValue(this IBattleEntityDump entity, ShellEffectType effectType)
    {
        if (!entity.IsPlayer())
        {
            return 0;
        }

        var player = (PlayerBattleEntityDump)entity;
        if (!player.ShellOptionsWeapon.Any())
        {
            return 0;
        }

        return player.ShellOptionsWeapon.GetOrDefault(effectType);
    }

    public static int GetShellArmorEffectValue(this IBattleEntityDump entity, ShellEffectType effectType)
    {
        if (!entity.IsPlayer())
        {
            return 0;
        }

        var character = (PlayerBattleEntityDump)entity;
        if (!character.ShellOptionArmor.Any())
        {
            return 0;
        }

        return character.ShellOptionArmor.GetOrDefault(effectType);
    }

    public static int GetJewelsEffectValue(this IBattleEntityDump entity, CellonType effectType)
    {
        if (!entity.IsPlayer())
        {
            return 0;
        }

        var character = (PlayerBattleEntityDump)entity;

        int ring = character.RingCellonValues.GetValueOrDefault(effectType);
        int necklace = character.NecklaceCellonValues.GetValueOrDefault(effectType);
        int bracelet = character.BraceletCellonValues.GetValueOrDefault(effectType);

        return ring + necklace + bracelet;
    }

    public static int GetMonsterDamageBonus(this IBattleEntityDump attacker, int level)
    {
        if (level < 45)
        {
            return -15;
        }

        if (level < 55)
        {
            return 30 + (level - 45) * 1;
        }

        if (level < 60)
        {
            return 95 + (level - 55) * 2;
        }

        if (level < 65)
        {
            return 165 + (level - 60) * 3;
        }

        if (level < 70)
        {
            return 245 + (level - 65) * 4;
        }

        return 335 + (level - 70) * 5;
    }

    public static bool IsOnMapType(this IBattleEntityDump entity, MapFlags type) => entity.MapInstance.HasMapFlag(type);

    public static async Task<bool> ShouldSaveDefender(this IBattleEntity attacker, IBattleEntity defender, int damage,
        GameRevivalConfiguration gameRevivalConfiguration, IBuffFactory buffFactory)
    {
        switch (defender)
        {
            case IPlayerEntity playerEntity:
                if (playerEntity.CheatComponent.HasGodMode)
                {
                    return true;
                }

                if (playerEntity.Hp - damage > 0)
                {
                    return false;
                }

                if (!playerEntity.RainbowBattleComponent.IsInRainbowBattle)
                {
                    return false;
                }

                await playerEntity.Session.EmitEventAsync(new RainbowBattleFreezeEvent
                {
                    Killer = attacker
                });

                return true;

            case IMateEntity mateEntity:
            {
                if (attacker.IsPlayer())
                {
                    return false;
                }

                if (mateEntity.MapInstance.HasMapFlag(MapFlags.IS_MINILAND_MAP))
                {
                    return false;
                }

                IPlayerEntity owner = mateEntity.Owner;

                if (mateEntity.Hp - damage > 0)
                {
                    return false;
                }

                if (mateEntity.MateType == MateType.Pet ? !owner.IsPetAutoRelive : !owner.IsPartnerAutoRelive)
                {
                    return false;
                }

                bool shouldSave = false;

                List<int> itemNeeded = mateEntity.MateType == MateType.Pet
                    ? gameRevivalConfiguration.MateRevivalConfiguration.MateInstantRevivalPenalizationSaver
                    : gameRevivalConfiguration.MateRevivalConfiguration.PartnerInstantRevivalPenalizationSaver;

                foreach (int item in itemNeeded)
                {
                    InventoryItem getItem = owner.GetFirstItemByVnum(item);
                    if (getItem == null)
                    {
                        continue;
                    }

                    await owner.Session.EmitEventAsync(new InventoryRemoveItemEvent(item, inventoryItem: getItem));
                    shouldSave = true;
                    break;
                }

                if (!shouldSave)
                {
                    return false;
                }

                (int mateHpToDecrease, int _) =
                    mateEntity.BCardComponent.GetAllBCardsInformation(BCardType.DamageConvertingSkill, (byte)AdditionalTypes.DamageConvertingSkill.HPRecoveryDecreased, mateEntity.Level);

                (int mateHpToIncrease, int _) =
                    mateEntity.BCardComponent.GetAllBCardsInformation(BCardType.DamageConvertingSkill, (byte)AdditionalTypes.DamageConvertingSkill.HPRecoveryIncreased, mateEntity.Level);

                int mateHpToChange = mateHpToIncrease - mateHpToDecrease;
                int mateHpHeal = (int)(mateEntity.MaxHp * (1 + mateHpToChange / 100.0));

                mateEntity.Hp = mateHpHeal;
                mateEntity.Mp = mateEntity.MaxMp;
                owner.Session.SendMateLife(mateEntity);

                GameDialogKey gameDialogKey = mateEntity.MateType == MateType.Pet ? GameDialogKey.PET_SHOUTMESSGE_SAVED_BY_SAVER : GameDialogKey.PARTNER_SHOUTMESSAGE_SAVED_BY_SAVER;
                owner.Session.SendMsg(owner.Session.GetLanguage(gameDialogKey), MsgMessageType.Middle);
                return true;
            }
            default:
                return false;
        }
    }
}