using System;
using System.Linq;
using System.Threading.Tasks;
using PhoenixLib.Logging;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.DTOs.BCards;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Items;
using WingsEmu.Game.Networking;
using WingsEmu.Game.SnackFood;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Character;
using WingsEmu.Packets.Enums.Chat;

namespace WingsEmu.Plugins.BasicImplementations.ItemUsage.Main;

public class FoodHandler : IItemHandler
{
    private readonly IBCardEffectHandlerContainer _bCardEffectHandlerContainer;
    private readonly SnackFoodConfiguration _configuration;
    private readonly IGameLanguageService _gameLanguage;

    public FoodHandler(IGameLanguageService gameLanguage, SnackFoodConfiguration configuration, IBCardEffectHandlerContainer bCardEffectHandlerContainer)
    {
        _gameLanguage = gameLanguage;
        _configuration = configuration;
        _bCardEffectHandlerContainer = bCardEffectHandlerContainer;
    }

    public ItemType ItemType => ItemType.Food;
    public long[] Effects => new long[] { 0 };

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        IPlayerEntity character = session.PlayerEntity;
        IGameItem gameItem = e.Item.ItemInstance.GameItem;
        DateTime now = DateTime.UtcNow;

        if (character.RainbowBattleComponent.IsInRainbowBattle)
        {
            session.SendChatMessage(session.GetLanguage(GameDialogKey.ITEM_MESSAGE_CANT_USE), ChatMessageColorType.Yellow);
            return;
        }

        if (character.LastFood > now)
        {
            Log.Debug($"{nameof(FoodHandler)} Food in cooldown (You can only use food every {_configuration.DelayBetweenFood})");
            return;
        }

        if (!session.PlayerEntity.IsAlive())
        {
            return;
        }

        if (session.PlayerEntity.IsOnVehicle)
        {
            return;
        }

        if (!session.PlayerEntity.IsSitting)
        {
            await session.RestAsync(true);
        }

        bool softCapReached = character.AddFood(gameItem);
        if (softCapReached)
        {
            string message = _gameLanguage.GetLanguage(character.Gender == GenderType.Male ? GameDialogKey.INFORMATION_MESSAGE_NOT_HUNGRY_MALE : GameDialogKey.INFORMATION_MESSAGE_NOT_HUNGRY_FEMALE,
                session.UserLanguage);
            session.SendChatMessage(message, ChatMessageColorType.PlayerSay);
            return;
        }

        if (gameItem.BCards.Any())
        {
            // Checks, if item has additional HP/MP inside item.BCards
            foreach (BCardDTO bCard in gameItem.BCards)
            {
                if (bCard.Type != (short)BCardType.HPMP)
                {
                    _bCardEffectHandlerContainer.Execute(character, character, bCard);
                    continue;
                }

                int firstDataValue = bCard.FirstDataValue(character.Level);
                int secondDataValue = bCard.SecondDataValue(character.Level);

                switch ((AdditionalTypes.HPMP)bCard.SubType)
                {
                    case AdditionalTypes.HPMP.ReceiveAdditionalHP:
                        character.AddAdditionalFood(character.MaxHp, firstDataValue, firstDataValue >= 0, secondDataValue);
                        break;
                    case AdditionalTypes.HPMP.ReceiveAdditionalMP:
                        character.AddAdditionalFood(character.MaxMp, firstDataValue, firstDataValue < 0, secondDataValue);
                        break;
                }
            }
        }

        character.LastFood = DateTime.UtcNow.AddMilliseconds(_configuration.DelayBetweenFood);

        await session.RemoveItemFromInventory(item: e.Item);
        session.SendEffect(EffectType.Eat);
    }
}