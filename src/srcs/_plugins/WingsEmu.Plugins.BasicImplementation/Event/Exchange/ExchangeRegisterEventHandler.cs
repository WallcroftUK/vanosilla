﻿using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.Game.Exchange;
using WingsEmu.Game.Exchange.Event;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;

namespace WingsEmu.Plugins.BasicImplementations.Event.Exchange;

public class ExchangeRegisterEventHandler : IAsyncEventProcessor<ExchangeRegisterEvent>
{
    private readonly ISessionManager _sessionManager;

    public ExchangeRegisterEventHandler(ISessionManager sessionManager) => _sessionManager = sessionManager;

    public async Task HandleAsync(ExchangeRegisterEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;

        if (!session.PlayerEntity.IsInExchange())
        {
            return;
        }

        PlayerExchange exchange = session.PlayerEntity.GetExchange();
        if (exchange == null)
        {
            return;
        }

        if (exchange.RegisteredItems)
        {
            return;
        }

        IClientSession target = _sessionManager.GetSessionByCharacterId(exchange.TargetId);
        if (target == null)
        {
            return;
        }

        exchange.Items = e.InventoryItems;
        exchange.Gold = e.Gold;
        exchange.BankGold = e.BankGold;
        exchange.RegisteredItems = true;
        target.SendExchangeWindow(session.PlayerEntity.Id, e.Gold, e.BankGold, e.Packets);
    }
}