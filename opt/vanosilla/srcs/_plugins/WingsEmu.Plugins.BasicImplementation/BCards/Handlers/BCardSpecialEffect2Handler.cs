// WingsEmu
// 
// Developed by NosWings Team

using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace WingsEmu.Plugins.BasicImplementations.BCards.Handlers;

public class BCardSpecialEffect2Handler : IBCardEffectAsyncHandler
{
    private readonly IBuffFactory _buffFactory;
    private readonly ICardsManager _cardsManager;
    private readonly IGameLanguageService _gameLanguage;
    private readonly IRandomGenerator _randomGenerator;

    public BCardSpecialEffect2Handler(IRandomGenerator randomGenerator, ICardsManager cardsManager, IBuffFactory buffFactory, IGameLanguageService gameLanguage)
    {
        _randomGenerator = randomGenerator;
        _cardsManager = cardsManager;
        _buffFactory = buffFactory;
        _gameLanguage = gameLanguage;
    }

    public BCardType HandledType => BCardType.SpecialEffects2;

    public void Execute(IBCardEffectContext ctx)
    {
        if (!(ctx.Sender is IPlayerEntity character))
        {
            return;
        }

        IBattleEntity sender = ctx.Sender;
        IBattleEntity target = ctx.Target;

        switch ((AdditionalTypes.SpecialEffects2)ctx.BCard.SubType)
        {
            case AdditionalTypes.SpecialEffects2.FocusEnemy:
                if (target is not IMonsterEntity monsterEntity)
                {
                    return;
                }

                sender.MapInstance.AddEntityToTargets(monsterEntity, sender);

                break;
            case AdditionalTypes.SpecialEffects2.TeleportInRadius:
                character.Session.SendGuriPacket(1, (byte)ctx.BCard.FirstData);
                break;
            case AdditionalTypes.SpecialEffects2.MainWeaponCausingChance:
            case AdditionalTypes.SpecialEffects2.SecondaryWeaponCausingChance:

                if (sender.IsSameEntity(target))
                {
                    return;
                }

                Buff b = _buffFactory.CreateBuff(ctx.BCard.SecondData, sender);
                if (b == null)
                {
                    return;
                }

                double debuffCounter = target.CheckForResistance(b, _cardsManager, out double buffCounter, out double specializedResistance);

                int randomNumber = _randomGenerator.RandomNumber();
                int debuffRandomNumber = _randomGenerator.RandomNumber();
                int buffRandomNumber = _randomGenerator.RandomNumber();
                int specializedRandomNumber = _randomGenerator.RandomNumber();
                switch (target)
                {
                    case IMonsterEntity monster:
                        if (!monster.CanBeDebuffed)
                        {
                            return;
                        }

                        break;
                }

                if (b.CardId == (short)BuffVnums.MEMORIAL && sender.BuffComponent.HasBuff((short)BuffVnums.MEMORIAL))
                {
                    return;
                }

                if (randomNumber > ctx.BCard.FirstData)
                {
                    return;
                }

                if (specializedRandomNumber >= (int)(specializedResistance * 100))
                {
                    if (!(target is IPlayerEntity c))
                    {
                        return;
                    }

                    string message = _gameLanguage.GetLanguage(GameDialogKey.BUFF_CHATMESSAGE_EFFECT_RESISTANCE, c.Session.UserLanguage);
                    c.Session.SendChatMessage(message, ChatMessageColorType.Buff);
                    return;
                }

                switch (b.BuffGroup)
                {
                    case BuffGroup.Bad when debuffRandomNumber >= (int)(debuffCounter * 100):
                    {
                        if (!(target is IPlayerEntity c))
                        {
                            return;
                        }

                        string message = _gameLanguage.GetLanguage(GameDialogKey.BUFF_CHATMESSAGE_EFFECT_RESISTANCE, c.Session.UserLanguage);
                        c.Session.SendChatMessage(message, ChatMessageColorType.Buff);
                        return;
                    }
                    case BuffGroup.Good when buffRandomNumber >= (int)(buffCounter * 100):
                    {
                        if (!(target is IPlayerEntity c))
                        {
                            return;
                        }

                        string message = _gameLanguage.GetLanguage(GameDialogKey.BUFF_CHATMESSAGE_EFFECT_RESISTANCE, c.Session.UserLanguage);
                        c.Session.SendChatMessage(message, ChatMessageColorType.Buff);
                        return;
                    }
                }

                Buff buff = _buffFactory.CreateBuff(ctx.BCard.SecondData, sender);
                target.AddBuffAsync(buff).ConfigureAwait(false).GetAwaiter().GetResult();
                break;
        }
    }
}