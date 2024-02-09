﻿using System.Threading.Tasks;
using WingsAPI.Game.Extensions.RelationsExtensions;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;
using WingsEmu.Packets.Enums.Relations;

namespace WingsEmu.Plugins.PacketHandling.Game.Basic;

public class BlDelPacketHandler : GenericGamePacketHandlerBase<BlDelPacket>
{
    private readonly IGameLanguageService _language;

    public BlDelPacketHandler(IGameLanguageService language) => _language = language;

    protected override async Task HandlePacketAsync(IClientSession session, BlDelPacket packet)
    {
        if (!session.PlayerEntity.IsBlocking(packet.CharacterId))
        {
            return;
        }

        await session.RemoveRelationAsync(packet.CharacterId, CharacterRelationType.Blocked);
        session.SendInfo(_language.GetLanguage(GameDialogKey.BLACKLIST_INFO_DELETED, session.UserLanguage));
    }
}