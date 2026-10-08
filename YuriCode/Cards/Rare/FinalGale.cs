using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Yuri.YuriCode.Extensions;
using Yuri.YuriCode.Mechanics;
using Yuri.YuriCode.Powers;

namespace Yuri.YuriCode.Cards.Rare;

public class FinalGale() : YuriCard(7, CardType.Attack,
    CardRarity.Rare, TargetType.AllEnemies), IArcaneArte
{
    protected override bool ShouldGlowGoldInternal => base.Owner.HasPower<OverlimitPower>() || Owner.HasPower<ArcaneArteBoostPower>();
    
    public int ComboGain => 1;
    
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CalculationBaseVar(28m),
        new EnergyVar(1),
        new ExtraDamageVar(2),
        new CalculatedDamageVar(ValueProp.Move)
            .WithMultiplier((card, _) =>
            {
                bool boosted =
                    card.Owner.HasPower<ArcaneArteBoostPower>() ||
                    card.Owner.HasPower<OverlimitPower>();

                if (!boosted)
                    return 0m;

                return card.Owner.PlayerCombatState.AllCards
                    .Count(c => c is IArcaneArte);
            })
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        YuriStaticHoverTips.ArcaneArte,
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {

        var enemies = CombatState.HittableEnemies;
        var ownerCreature = Owner?.Creature;
        
        CenterCardCinematic.Start(RunManager.Instance.NetService.NetId);
        if (ownerCreature != null && Owner?.Character is Character.Yuri yuri)
        {
            await yuri.ReturnToCombatHome(Owner.Creature);
            
            SfxCmd.Play("res://Yuri/sounds/artes/final gale.wav");
            yuri.PlayAnimation(ownerCreature, "final_gale");
            await Task.Delay((int)(0.067f * 1000f));
            SfxCmd.Play("res://Yuri/sfx/swing_1.wav");
            await Task.Delay((int)(0.200f * 1000f));
            SfxCmd.Play("res://Yuri/sfx/swing_1.wav");
            await Task.Delay((int)(0.137f * 1000f));
            SfxCmd.Play("res://Yuri/sfx/FromTifa/final_heaven_swing_2.wav");
            await Task.Delay((int)(0.137f * 1000f));
            foreach (var enemy in enemies)
                yuri.PlayVfxOnTarget(
                    enemy,
                    "res://Yuri/scenes/vfx.tscn",
                    "hit"
                );
        }
        await CommonActions.CardAttack(this, play.Target)
            .WithHitFx(null, "res://Yuri/sfx/hit_2.wav")
            .Execute(choiceContext);
        CenterCardCinematic.End(RunManager.Instance.NetService.NetId);
    }
    
    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
    
    public override Task AfterCardEnteredCombat(CardModel card)
    {
        if (card != this)
            return Task.CompletedTask;

        if (IsClone)
            return Task.CompletedTask;

        int count = CombatManager.Instance.History.CardPlaysFinished.Count(
            e =>
                e.CardPlay.Player == Owner &&
                e.CardPlay.Card is IBaseArte);

        EnergyCost.AddThisCombat(-count * DynamicVars.Energy.IntValue);

        return Task.CompletedTask;
    }
    
    public override Task AfterCardPlayed(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner)
            return Task.CompletedTask;

        if (cardPlay.Card is not IBaseArte)
            return Task.CompletedTask;

        EnergyCost.AddThisCombat(-DynamicVars.Energy.IntValue);

        return Task.CompletedTask;
    }
}