using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Yuri.YuriCode.Cards.Ancient;
using Yuri.YuriCode.Cards.Common;
using Yuri.YuriCode.Mechanics;
using Yuri.YuriCode.Relics;

namespace Yuri.YuriCode.Powers;

public class OverlimitPower : YuriPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;
    
    private IEnumerable<CardModel> GetSavageWolfFury()
    {
        var hand =
            PileType.Hand.GetPile(Owner.Player);

        return hand.Cards
            .OfType<SavageWolfFury>();
    }
    
    public override async Task AfterApplied(
        Creature? applier,
        CardModel? cardSource)
    {
        var existingCard = Owner.Player.PlayerCombatState?
            .AllCards
            .OfType<SavageWolfFury>()
            .FirstOrDefault();

        if (existingCard == null)
        {
            var card =
                CombatState.CreateCard<SavageWolfFury>(
                    Owner.Player);

            if (Owner.Player.GetRelic<VesperiaNoTwo>() != null)
            {
                CardCmd.Upgrade(card);
            }

            await CardPileCmd.AddGeneratedCardToCombat(
                card,
                PileType.Hand,
                Owner.Player);
        }
        else if (existingCard.Pile?.Type != PileType.Hand)
        {
            await CardPileCmd.Add(
                existingCard,
                PileType.Hand);
        }
        
        var ownerCreature = Owner;
        var yuri = Owner?.Player.Character as Character.Yuri;
        if (ownerCreature != null && yuri != null)
        {
            SfxCmd.Play("res://Yuri/sounds/overlimit.wav");
            SfxCmd.Play("res://Yuri/sfx/mystic_arte_activate_2.wav");
            float duration = yuri.PlayAnimation(ownerCreature, "overlimit").total;
            await Task.Delay((int)(0.5f * 1000f));
        }
    }
    
    public override bool TryModifyEnergyCostInCombatLate(
        CardModel card,
        decimal originalCost,
        out decimal modifiedCost)
    {
        modifiedCost = originalCost;

        if (card.Owner.Creature != Owner)
        {
            return false;
        }

        if (card is not CrushingEagle)
        {
            return false;
        }

        if (card.Pile?.Type is not (PileType.Hand or PileType.Play))
        {
            return false;
        }

        // ArcaneArteBoostPower takes priority, preventing both powers
        // from independently reducing Crushing Eagle's cost.
        if (Owner.GetPower<ArcaneArteBoostPower>() != null)
        {
            return false;
        }

        modifiedCost = decimal.Max(0m, originalCost - 1m);
        return modifiedCost != originalCost;
    }
    
    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player)
            return;

        if (Owner.GetPowerAmount<OverlimitPower>() <= 1)
        {
            foreach (CardModel card in
                     GetSavageWolfFury().ToList())
            {
                await CardCmd.Exhaust(
                    choiceContext,
                    card);
            }
            OverlimitManager.SetOverlimit(Owner.Player, 0);
        }

        await PowerCmd.Decrement(this);
    }
    
    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (side != base.Owner.Side)
            return;

        var player = Owner.Player;
        var playerState = player.PlayerCombatState;

        if (playerState == null)
            return;
        
        if (playerState.AllCards.OfType<SavageWolfFury>().All(c => c.Pile?.Type != PileType.Hand))
        {
            var cards = playerState.AllCards
                .OfType<SavageWolfFury>()
                .Where(c =>
                    c.Pile == null ||
                    c.Pile.Type != PileType.Hand);

            await CardPileCmd.Add(
                cards,
                PileType.Hand);
        }
    }
}