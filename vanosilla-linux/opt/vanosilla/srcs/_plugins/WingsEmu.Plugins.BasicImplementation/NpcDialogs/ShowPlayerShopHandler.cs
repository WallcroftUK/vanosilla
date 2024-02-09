﻿using System.Collections.Generic;
using System.Threading.Tasks;
using WingsAPI.Game.Extensions.ItemExtension;
using WingsEmu.Game._NpcDialog;
using WingsEmu.Game._NpcDialog.Event;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Shops;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Plugins.BasicImplementations.NpcDialogs;

public class ShowPlayerShopHandler : INpcDialogAsyncHandler
{
    public NpcRunType[] NpcRunTypes => new[] { NpcRunType.SHOW_PLAYER_SHOP };

    public async Task Execute(IClientSession session, NpcDialogEvent e)
    {
        IPlayerEntity owner = session.CurrentMapInstance.GetCharacterById(e.NpcId);
        IEnumerable<ShopPlayerItem> items = owner?.ShopComponent.Items;
        if (items == null)
        {
            return;
        }

        session.SendShopContent(e.NpcId, items);
    }
}