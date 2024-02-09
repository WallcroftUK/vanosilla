using System.Threading.Tasks;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace WingsEmu.Plugins.BasicImplementations.ItemUsage.Etc.Teacher;

public class PetSummoningScrollHandler : IItemHandler
{
    private readonly IGameLanguageService _gameLanguage;

    public PetSummoningScrollHandler(IGameLanguageService gameLanguage) => _gameLanguage = gameLanguage;

    public ItemType ItemType => ItemType.PetPartnerItem;

    public long[] Effects => new long[] { 17 };

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        if (!int.TryParse(e.Packet[3], out int x1))
        {
            return;
        }

        IMateEntity mateEntity = session.PlayerEntity.MateComponent.GetMate(s => s.Id == x1);
        if (mateEntity == null)
        {
            return;
        }

        if (mateEntity.IsSummonable)
        {
            session.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.PET_MESSAGE_IS_ALREADY_SUMMONABLE, e.Sender.UserLanguage), ChatMessageColorType.Yellow);
            session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.PET_MESSAGE_IS_ALREADY_SUMMONABLE, e.Sender.UserLanguage), MsgMessageType.Middle);
            return;
        }

        await session.RemoveItemFromInventory(item: e.Item);
        mateEntity.IsSummonable = true;
        string mateName = string.IsNullOrEmpty(mateEntity.MateName) || mateEntity.MateName == mateEntity.Name
            ? _gameLanguage.GetLanguage(GameDataType.NpcMonster, mateEntity.Name, session.UserLanguage)
            : mateEntity.MateName;
        session.SendScpPackets();
        session.SendScnPackets();
        session.SendChatMessage(_gameLanguage.GetLanguageFormat(GameDialogKey.PET_MESSAGE_SUMMONABLE, e.Sender.UserLanguage, mateName), ChatMessageColorType.Yellow);
        session.SendMsg(_gameLanguage.GetLanguageFormat(GameDialogKey.PET_MESSAGE_SUMMONABLE, e.Sender.UserLanguage, mateName), MsgMessageType.Middle);
    }
}