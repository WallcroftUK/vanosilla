﻿// WingsEmu
// 
// Developed by NosWings Team

using System;

namespace WingsAPI.Plugins.Exceptions
{
    public class PluginException : Exception
    {
        public PluginException(string message) : base(message)
        {
        }
    }
}