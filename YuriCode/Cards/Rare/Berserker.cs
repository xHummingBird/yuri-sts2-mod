using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Yuri.YuriCode.Extensions;

namespace Yuri.YuriCode.Cards.Rare;

public class Berserker() : YuriCard(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        foreach (CardModel card in PileType.Hand.GetPile(base.Owner).Cards.ToList())
        {
            if (card.Type == CardType.Attack && !card.EnergyCost.CostsX)
            {
                card.SetToFreeThisTurn();
            }
        }
    }
    
    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}