// WingsEmu
// 
// Developed by NosWings Team

using System;

namespace PhoenixLib.ServiceBus
{
    internal interface ISubscribedMessage
    {
        Type Type { get; }
    }
}