﻿using System;
using PhoenixLib.ServiceBus.Routing;

namespace Plugin.PlayerLogs.Messages.Raid
{
    [MessageType("logs.raid.rewardreceived")]
    public class LogRaidRewardReceivedMessage : IPlayerActionLogMessage
    {
        public Guid RaidId { get; set; }
        public byte BoxRarity { get; set; }
        public DateTime CreatedAt { get; init; }
        public int ChannelId { get; init; }
        public long CharacterId { get; init; }
        public string CharacterName { get; init; }
        public string IpAddress { get; init; }
    }
}