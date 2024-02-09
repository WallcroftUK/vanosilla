﻿using System.Threading.Tasks;
using WingsAPI.Game.Extensions.Groups;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Configuration;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Algorithm;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Extensions.Mates;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Plugins.BasicImplementations.ItemUsage.Etc.Special;

public class StrangePetFoodHandler : IItemHandler
{
    private readonly IBattleEntityAlgorithmService _algorithm;
    private readonly ICharacterAlgorithm _characterAlgorithm;
    private readonly IGameLanguageService _gameLanguage;
    private readonly ISpPartnerConfiguration _spPartnerConfiguration;

    public StrangePetFoodHandler(IGameLanguageService gameLanguage, IBattleEntityAlgorithmService algorithm, ICharacterAlgorithm characterAlgorithm, ISpPartnerConfiguration spPartnerConfiguration)
    {
        _gameLanguage = gameLanguage;
        _algorithm = algorithm;
        _characterAlgorithm = characterAlgorithm;
        _spPartnerConfiguration = spPartnerConfiguration;
    }

    public ItemType ItemType => ItemType.PetPartnerItem;
    public long[] Effects => new long[] { 16 };

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        if (e.Packet == null || e.Packet.Length == 0)
        {
            return;
        }

        if (!int.TryParse(e.Packet[3], out int mateId))
        {
            return;
        }

        IMateEntity mateEntity = session.PlayerEntity.MateComponent.GetMate(x => x.Id == mateId && x.MateType == MateType.Pet);
        if (mateEntity == null)
        {
            return;
        }

        if (!mateEntity.IsAlive())
        {
            return;
        }

        if (mateEntity.Level - 1 <= 0)
        {
            return;
        }

        if (session.PlayerEntity.IsOnVehicle)
        {
            return;
        }

        long mateXp = _characterAlgorithm.GetLevelXp((short)(mateEntity.Level - 1), true, mateEntity.MateType);
        mateEntity.Level -= 1;
        mateEntity.Experience = mateXp;
        mateEntity.RefreshMaxHpMp(_algorithm);
        session.RefreshParty(_spPartnerConfiguration);
        mateEntity.Hp = mateEntity.MaxHp;
        mateEntity.Mp = mateEntity.MaxMp;
        session.SendMateEffect(mateEntity, EffectType.PetLoveBroke);
        session.SendPetInfo(mateEntity, _gameLanguage);
        await session.RemoveItemFromInventory(item: e.Item);
    }
}