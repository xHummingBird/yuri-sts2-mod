using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;

namespace Yuri.YuriCode.Powers;

public class ImprovisionistPower : YuriPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromKeyword(CardKeyword.Ethereal)
    ];

    public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
    {
        if (player != base.Owner.Player)
        {
            return;
        }
        IReadOnlyList<CardModel> readOnlyList = base.Owner.Player.Character.CardPool.GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint).Where(delegate(CardModel c)
        {
            CardRarity rarity = c.Rarity;
            bool flag = ((rarity == CardRarity.Basic || rarity == CardRarity.Ancient) ? true : false);
            return !flag;
        }).ToList();
        if (readOnlyList.Count > 0)
        {
            CardModel[] array = new CardModel[base.Amount];
            Rng combatCardGeneration = base.Owner.Player.RunState.Rng.CombatCardGeneration;
            for (int num = 0; num < base.Amount; num++)
            {
                CardCmd.ApplyKeyword(array[num] = CardFactory.GetDistinctForCombat(player, readOnlyList, 1, combatCardGeneration).First(), CardKeyword.Ethereal);
            }
            Flash();
            await CardPileCmd.AddGeneratedCardsToCombat(array, PileType.Hand, base.Owner.Player);
        }
    }
}