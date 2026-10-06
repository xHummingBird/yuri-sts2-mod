using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Yuri.YuriCode.Extensions;
using Yuri.YuriCode.Mechanics;

namespace Yuri.YuriCode.Cards.Common;

public class LinkSlash() : YuriCard(
    2,
    CardType.Attack,
    CardRarity.Common,
    TargetType.AnyEnemy)
{
    protected override bool ShouldGlowGoldInternal =>
        HasPlayedArcaneArteThisTurn;

    private bool HasPlayedArcaneArteThisTurn =>
        CombatManager.Instance.History.CardPlaysFinished.Any(
            entry =>
                entry.HappenedThisTurn(CombatState) &&
                entry.CardPlay.Card.Owner == Owner &&
                entry.CardPlay.Card is IArcaneArte);

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(
            12,
            ValueProp.Move),
        new EnergyVar(1)
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var ownerCreature =
            Owner?.Creature;

        if (ownerCreature != null &&
            Owner?.Character is Character.Yuri yuri)
        {
            float distance =
                yuri.DistanceToTarget(
                    ownerCreature,
                    play.Target);

            /*
             * If Yuri is already too close, move backward first
             * so the attack animation has enough visual space.
             */
            if (distance < 200f)
            {
                await yuri.YuriDashTo(
                    ownerCreature,
                    play.Target,
                    distance: 250f,
                    forceMove: true,
                    durationSeconds: 0.05f,
                    overrideAnim: "retreat");
            }

            await yuri.YuriDashTo(
                ownerCreature,
                play.Target);

            AudioHelper.PlayRandomAttack();

            yuri.PlayAnimation(
                ownerCreature,
                "attack",
                true);

            SfxCmd.Play(
                "res://Yuri/sfx/swing_1.wav");

            await Task.Delay(
                (int)(0.1167f * 1000f));

            yuri.PlayVfxOnTarget(
                play.Target,
                "res://Yuri/scenes/vfx.tscn",
                "hit");
        }

        await CommonActions.CardAttack(
                this,
                play.Target)
            .WithHitFx(
                null,
                "res://Yuri/sfx/hit_2.wav")
            .Execute(
                choiceContext);
    }

    public override Task AfterCardPlayed(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner)
            return Task.CompletedTask;

        if (cardPlay.Card is IArcaneArte)
        {
            ReduceCost();
        }

        return Task.CompletedTask;
    }

    public override Task AfterCardEnteredCombat(
        CardModel card)
    {
        if (card != this)
            return Task.CompletedTask;

        if (IsClone)
            return Task.CompletedTask;

        /*
         * Link Slash may enter combat after an Arcane Arte
         * has already been played this turn.
         */
        if (HasPlayedArcaneArteThisTurn)
        {
            ReduceCost();
        }

        return Task.CompletedTask;
    }

    private void ReduceCost()
    {
        EnergyCost.SetThisTurn(
            0);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(
            4);
    }
}