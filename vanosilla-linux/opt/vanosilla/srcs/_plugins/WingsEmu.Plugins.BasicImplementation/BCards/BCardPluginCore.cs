﻿using Microsoft.Extensions.DependencyInjection;
using WingsAPI.Plugins;

namespace WingsEmu.Plugins.BasicImplementations.BCards;

public class BCardPluginCore : IGameServerPlugin
{
    public string Name => nameof(BCardPluginCore);

    public void AddDependencies(IServiceCollection services, GameServerLoader gameServer)
    {
        services.AddBcardHandlers();
        services.AddBCardContextFactory();
    }
}