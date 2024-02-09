﻿using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Chat;

namespace WingsEmu.Plugins.BasicImplementations.Event.Characters;

public class GenerateGoldEventHandler : IAsyncEventProcessor<GenerateGoldEvent>
{
    private readonly IGameLanguageService _gameLanguage;
    private readonly IItemsManager _itemsManager;
    private readonly GameMinMaxConfiguration _minMaxConfiguration;

    public GenerateGoldEventHandler(IGameLanguageService gameLanguage, IItemsManager itemsManager, GameMinMaxConfiguration minMaxConfiguration)
    {
        _gameLanguage = gameLanguage;
        _itemsManager = itemsManager;
        _minMaxConfiguration = minMaxConfiguration;
    }

    public async Task HandleAsync(GenerateGoldEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        long val = e.Amount;

        if (session.PlayerEntity.Gold + val > _minMaxConfiguration.MaxGold)
        {
            long difference = session.PlayerEntity.Gold + val - _minMaxConfiguration.MaxGold;
            session.PlayerEntity.Gold = _minMaxConfiguration.MaxGold;
            if (e.FallBackToBank)
            {
                session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.BANK_SHOUTMESSAGE_EXCEED_GOLD_SENT_TO_BANK, session.UserLanguage), MsgMessageType.Middle);
                session.Account.BankMoney += difference;
            }
            else
            {
                session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.INFORMATION_MESSAGE_MAX_GOLD, session.UserLanguage), MsgMessageType.Middle);
            }

            session.RefreshGold();
            return;
        }

        session.PlayerEntity.Gold += val;

        if (e.SendMessage)
        {
            string gold = _gameLanguage.GetLanguage(GameDataType.Item, _itemsManager.GetItem((short)ItemVnums.GOLD).Name, session.UserLanguage);

            session.SendChatMessage(_gameLanguage.GetLanguageFormat(GameDialogKey.INVENTORY_CHATMESSAGE_X_ITEM_ACQUIRED, session.UserLanguage, val, gold), ChatMessageColorType.Yellow);
        }

        session.RefreshGold();
    }
}