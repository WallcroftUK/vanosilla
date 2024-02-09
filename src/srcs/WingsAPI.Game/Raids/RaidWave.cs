﻿using System.Collections.Generic;

namespace WingsEmu.Game.Raids;

public class RaidWave
{
    public RaidWave(IEnumerable<ToSummon> monsters, short timeInSeconds)
    {
        Monsters = monsters;
        TimeInSeconds = timeInSeconds;
    }

    public IEnumerable<ToSummon> Monsters { get; }
    public short TimeInSeconds { get; }
}