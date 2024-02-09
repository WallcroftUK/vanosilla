﻿using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Mates.Events;

public class MateStayInsideMinilandEvent : PlayerEvent
{
    public IMateEntity MateEntity { get; init; }
    public bool IsOnCharacterEnter { get; init; }
}