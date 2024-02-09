﻿using Microsoft.Extensions.DependencyInjection;
using WingsAPI.Plugins;

namespace WingsEmu.Plugins.BasicImplementations;

public class GuriPluginCore : IGameServerPlugin
{
    public string Name => nameof(GuriPluginCore);

    public void AddDependencies(IServiceCollection services, GameServerLoader gameServer)
    {
        services.AddGuriHandlers();
    }
}