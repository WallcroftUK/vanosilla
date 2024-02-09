﻿using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.ServiceBus;
using WingsAPI.Game.Extensions.PacketGeneration;
using WingsEmu.DTOs.Mails;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Items;
using WingsEmu.Game.Mails;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Chat;

namespace WingsEmu.Plugins.DistributedGameEvents.Mails
{
    public class MailReceivedMessageConsumer : IMessageConsumer<MailReceivedMessage>
    {
        private readonly IGameLanguageService _gameLanguage;
        private readonly IGameItemInstanceFactory _itemInstanceFactory;
        private readonly ISessionManager _sessionManager;

        public MailReceivedMessageConsumer(IGameLanguageService gameLanguage, ISessionManager sessionManager, IGameItemInstanceFactory itemInstanceFactory)
        {
            _gameLanguage = gameLanguage;
            _sessionManager = sessionManager;
            _itemInstanceFactory = itemInstanceFactory;
        }

        public async Task HandleAsync(MailReceivedMessage e, CancellationToken cancellation)
        {
            IClientSession session = _sessionManager.GetSessionByCharacterId(e.CharacterId);
            if (session == null)
            {
                return;
            }

            foreach (CharacterMailDto dto in e.MailDtos.ToArray())
            {
                GameItemInstance itemInstance = _itemInstanceFactory.CreateItem(dto.ItemInstance);

                var newMail = new CharacterMail(dto, session.GetNextMailSlot(), itemInstance);
                session.PlayerEntity.MailNoteComponent.AddMail(newMail);
                session.SendParcel(newMail);
                session.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.MAIL_CHATMESSAGE_NEW_MAIL_RECEIVE, session.UserLanguage), ChatMessageColorType.Green);
            }
        }
    }
}