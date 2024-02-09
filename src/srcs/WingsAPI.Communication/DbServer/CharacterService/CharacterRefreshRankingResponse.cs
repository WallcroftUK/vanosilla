﻿using System.Collections.Generic;
using ProtoBuf;
using WingsAPI.Data.Character;

namespace WingsAPI.Communication.DbServer.CharacterService
{
    [ProtoContract]
    public class CharacterRefreshRankingResponse
    {
        [ProtoMember(1)]
        public RpcResponseType ResponseType { get; init; }

        [ProtoMember(2)]
        public IReadOnlyList<CharacterDTO> TopCompliment { get; init; }

        [ProtoMember(3)]
        public IReadOnlyList<CharacterDTO> TopPoints { get; init; }

        [ProtoMember(4)]
        public IReadOnlyList<CharacterDTO> TopReputation { get; init; }
    }
}