﻿using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Raids.Events;

namespace WingsEmu.Plugins.BasicImplementations.CharacterLifetimeStats;

public class TotalRaidsLostEventHandler : IAsyncEventProcessor<RaidLostEvent>
{
    public async Task HandleAsync(RaidLostEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        session.PlayerEntity.LifetimeStats.TotalRaidsLost++;
    }
}