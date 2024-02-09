using System.Threading.Tasks;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace WingsEmu.Plugins.BasicImplementations.ItemUsage.Etc.Teacher;

public class PickUpPetFoodHandler : IItemHandler
{
    private readonly IGameLanguageService _gameLanguage;

    public PickUpPetFoodHandler(IGameLanguageService gameLanguage) => _gameLanguage = gameLanguage;

    public ItemType ItemType => ItemType.PetPartnerItem;
    public long[] Effects => new long[] { 14 };

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        if (e.Packet.Length < 4)
        {
            return;
        }

        if (!int.TryParse(e.Packet[3], out int x1))
        {
            return;
        }

        IMateEntity mateEntity = session.PlayerEntity.MateComponent.GetMate(s => s.Id == x1 && s.MateType == MateType.Pet);

        if (mateEntity == null)
        {
            return;
        }

        if (mateEntity.CanPickUp)
        {
            session.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.PET_CHATMESSAGE_ALREADY_CAN_PICK_UP, session.UserLanguage), ChatMessageColorType.Red);
            return;
        }

        session.SendPacket(mateEntity.GenerateEffectPacket(EffectType.ShinyStars));
        session.SendPacket(mateEntity.GenerateEffectPacket(EffectType.PetLove));

        mateEntity.CanPickUp = true;

        session.SendScpPackets();
        session.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.PET_CHATMESSAGE_CAN_PICK_UP, e.Sender.UserLanguage), ChatMessageColorType.Yellow);

        await session.RemoveItemFromInventory(item: e.Item);
    }
}