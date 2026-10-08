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

namespace Yuri.YuriCode.Cards.Uncommon;

public class LoneWolfCharge() : YuriCard(2, CardType.Attack,
    CardRarity.Rare, TargetType.AllEnemies), IArcaneArte
{
    protected override bool ShouldGlowGoldInternal => base.Owner.HasPower<OverlimitPower>() || Owner.HasPower<ArcaneArteBoostPower>();
    
    public int ComboGain => 1;
    
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(15m, ValueProp.Move),
        new EnergyVar(1),
        new PowerVar<WeakPower>(2)
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        YuriStaticHoverTips.ArcaneArte,
        HoverTipFactory.FromPower<WeakPower>()
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var ownerCreature = Owner?.Creature;
        
        var enemies = CombatState.HittableEnemies.ToList();
        
        var leftMostEnemy = enemies[0];
        
        CenterCardCinematic.Start(RunManager.Instance.NetService.NetId);
        if (ownerCreature != null && Owner?.Character is Character.Yuri yuri)
        {
            float distance =
                yuri.DistanceToTarget(
                    ownerCreature,
                    leftMostEnemy
                );
            
            if (distance < 200f)
                await yuri.YuriDashTo(ownerCreature, leftMostEnemy, distance: 200f, forceMove: true, durationSeconds: 0.05f, overrideAnim: "retreat");
            
            await yuri.YuriDashTo(ownerCreature, leftMostEnemy, distance: 200f);
            SfxCmd.Play("res://Yuri/sounds/artes/lone_wolf_charge.wav");
            yuri.PlayAnimation(ownerCreature, "lone_wolf");
            await Task.Delay((int)(0.35f * 1000f));
            SfxCmd.Play("res://Yuri/sfx/FromTifa/final_heaven_swing_2.wav");
            await Task.Delay((int)(0.05f * 1000f));
            YuriExtensions.CombatHelpers.YuriFakeHit(ownerCreature, leftMostEnemy, "res://Yuri/sfx/FromTifa/final_heaven_hit.wav", "hit");
            
            await Task.Delay((int)(0.1f * 1000f));
            foreach (var enemy in enemies)
                yuri.PlayVfxOnTarget(
                    play.Target,
                    "res://Yuri/scenes/vfx.tscn",
                    "hit_red"
                );
        }
        await CommonActions.CardAttack(this, play.Target)
            .WithHitFx(null, "res://Yuri/sfx/FromTifa/chi_sfx_2.wav")
            .Execute(choiceContext);
        await Task.Delay((int)(0.4f * 1000f));
        CenterCardCinematic.End(RunManager.Instance.NetService.NetId);
        foreach (var enemy in enemies)
            await FatalStrikePower.CheckAfterCardApplication(
                enemy
            );
        if (Owner.HasPower<OverlimitPower>() || Owner.HasPower<ArcaneArteBoostPower>())
            await PowerCmd.Apply<FreeSkillPower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
    }
    
    public override Task AfterCardEnteredCombat(CardModel card)
    {
        if (card != this)
        {
            return Task.CompletedTask;
        }
        if (base.IsClone)
        {
            return Task.CompletedTask;
        }
        int amount = CombatManager.Instance.History.CardPlaysFinished.Count((CardPlayFinishedEntry e) => e.CardPlay.Card.Type == CardType.Skill && e.CardPlay.Player == base.Owner && e.HappenedThisTurn(base.CombatState));
        ReduceCostBy(amount);
        return Task.CompletedTask;
    }

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != base.Owner)
        {
            return Task.CompletedTask;
        }
        if (cardPlay.Card.Type != CardType.Skill)
        {
            return Task.CompletedTask;
        }
        ReduceCostBy(1);
        return Task.CompletedTask;
    }

    private void ReduceCostBy(int amount)
    {
        base.EnergyCost.AddThisTurn(-amount);
    }
    
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(5);
        DynamicVars.Weak.UpgradeValueBy(1);
    }
}