using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Models;
using Yuri.YuriCode.Mechanics;

namespace Yuri.YuriCode.Relics;

public class BhodiBlastia : YuriRelic
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override async Task AfterSideTurnStart(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (!participants.Contains(Owner.Creature))
        {
            return;
        }

        if (Owner.PlayerCombatState.TurnNumber > 1)
        {
            return;
        }

        IReadOnlyList<CardModel> validCards =
            Owner.Character.CardPool
                .GetUnlockedCards(
                    Owner.UnlockState,
                    Owner.RunState.CardMultiplayerConstraint)
                .Where(c =>
                    c is IBaseArte ||
                    c is IArcaneArte)
                .ToList();

        if (validCards.Count == 0)
        {
            return;
        }

        Flash();

        List<CardModel> cards =
            CardFactory.GetDistinctForCombat(
                    Owner,
                    validCards,
                    1,
                    Owner.RunState.Rng.CombatCardGeneration)
                .ToList();

        await CardPileCmd.AddGeneratedCardsToCombat(
            cards,
            PileType.Hand,
            Owner);
    }
}