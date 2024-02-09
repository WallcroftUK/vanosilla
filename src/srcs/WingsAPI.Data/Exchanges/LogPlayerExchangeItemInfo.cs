﻿using WingsEmu.DTOs.Items;

namespace WingsAPI.Data.Exchanges;

public class LogPlayerExchangeItemInfo
{
    public short Amount { get; init; }
    public ItemInstanceDTO ItemInstance { get; init; }
    public byte Slot { get; set; }
}