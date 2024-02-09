// WingsEmu
// 
// Developed by NosWings Team

using System;
using System.Threading.Tasks;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.DTOs.Bonus;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace WingsEmu.Plugins.BasicImplementations.ItemUsage.Main.Special;

public class PartnerBackpackHandler : IItemHandler
{
    private readonly IGameLanguageService _gameLanguage;

    public PartnerBackpackHandler(IGameLanguageService gameLanguage) => _gameLanguage = gameLanguage;

    public ItemType ItemType => ItemType.Special;
    public long[] Effects => new long[] { 602, 1008 };

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        if (session.PlayerEntity.HaveStaticBonus(StaticBonusType.PartnerBackpack))
        {
            return;
        }

        DateTime? effectDateEnd = e.Item.ItemInstance.GameItem.EffectValue == 0 ? null : DateTime.UtcNow.AddDays(e.Item.ItemInstance.GameItem.EffectValue);

        await session.EmitEventAsync(new AddStaticBonusEvent(new CharacterStaticBonusDto
        {
            DateEnd = effectDateEnd,
            ItemVnum = e.Item.ItemInstance.GameItem.Id,
            StaticBonusType = StaticBonusType.PartnerBackpack
        }));

        await session.RemoveItemFromInventory(item: e.Item);
        session.ShowInventoryExtensions();

        string name = _gameLanguage.GetLanguage(GameDataType.Item, e.Item.ItemInstance.GameItem.Name, session.UserLanguage);
        session.SendChatMessage(
            _gameLanguage.GetLanguageFormat(GameDialogKey.ITEM_CHATMESSAGE_EFFECT_ACTIVATED, session.UserLanguage, name),
            ChatMessageColorType.Green);
    }
}