﻿using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Bazaar.Events;

public class BazaarItemChangePriceEvent : PlayerEvent
{
    public BazaarItemChangePriceEvent(long bazaarItemId, long newPricePerItem, bool confirmed)
    {
        BazaarItemId = bazaarItemId;
        NewPricePerItem = newPricePerItem;
        Confirmed = confirmed;
    }

    public long BazaarItemId { get; }

    public long NewPricePerItem { get; }

    public bool Confirmed { get; }
}