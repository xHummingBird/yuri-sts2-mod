using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Yuri.YuriCode.Extensions;
using Yuri.YuriCode.Mechanics;
using Yuri.YuriCode.Powers;

namespace Yuri.YuriCode.Cards.Uncommon;

public class GuardianField() : YuriCard(2, CardType.Attack,
    CardRarity.Uncommon, TargetType.AnyEnemy), IArcaneArte
{
    protected override bool ShouldGlowGoldInternal => base.Owner.HasPower<OverlimitPower>() || Owner.HasPower<ArcaneArteBoostPower>();
    
    public int ComboGain => 1;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => 
    [
        new DamageVar(10, ValueProp.Move),
        new BlockVar(10, ValueProp.Move)
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var ownerCreature = Owner?.Creature;
        var blockAmount = DynamicVars.Damage.PreviewValue;
        if (ownerCreature != null && Owner?.Character is Character.Yuri yuri)
        {
            float distance =
                yuri.DistanceToTarget(
                    ownerCreature,
                    play.Target
                );
            
            if (distance < 200f)
                await yuri.YuriDashTo(ownerCreature, play.Target, distance: 250f, forceMove: true, durationSeconds: 0.05f, overrideAnim: "retreat");
            
            await yuri.YuriDashTo(ownerCreature, play.Target);
            var splashTargets = CombatState
                .GetTeammatesOf(play.Target)
                .Where(enemy =>
                    enemy != play.Target &&
                    enemy.IsHittable &&
                    yuri.DistanceToTarget(ownerCreature, enemy) <= 250f)
                .ToList();
            
            SfxCmd.Play("res://Yuri/sounds/artes/guardian_field.wav");
            yuri.PlayAnimation(ownerCreature, "guardian_field");
            await Task.Delay((int)(0.150f * 1000f));
            SfxCmd.Play("res://Yuri/sfx/swing_1.wav");
            await Task.Delay((int)(0.1f * 1000f));
            SfxCmd.Play("res://Yuri/sfx/FromTifa/final_heaven_hit.wav");
            yuri.PlayVfxOnTarget(
                play.Target,
                "res://Yuri/scenes/vfx.tscn",
                "hit"
            );
            await using AttackContext context = await AttackCommand.CreateContextAsync(CombatState, choiceContext, play);

            List<DamageResult> primaryResults = (await CreatureCmd.Damage(choiceContext, play.Target, DynamicVars.Damage.BaseValue, ValueProp.Move, this, play)
            ).ToList();

            context.AddHit(primaryResults);
            
            var hit = primaryResults.FirstOrDefault();

            if (hit != null && splashTargets.Count > 0)
            {
                decimal splashDamage =
                    hit.TotalDamage +
                    hit.OverkillDamage;

                context.AddHit(
                    await CreatureCmd.Damage(
                        choiceContext,
                        splashTargets,
                        splashDamage,
                        ValueProp.Unpowered | ValueProp.Move,
                        ownerCreature,
                        this,
                        play
                    )
                );

                foreach (var enemy in splashTargets)
                {
                    yuri.PlayVfxOnTarget(
                        enemy,
                        "res://Yuri/scenes/vfx.tscn",
                        "hit"
                    );
                    await FatalStrikePower.CheckAfterCardApplication(
                        enemy
                    );
                }
            }
            await Task.Delay((int)(0.350f * 1000f));
            
        }
        else
            await CommonActions.CardAttack(this, play.Target)
                .WithHitFx(null, "res://Yuri/sfx/hit_2.wav")
                .Execute(choiceContext);
        await FatalStrikePower.CheckAfterCardApplication(
            play.Target
        );
        await CommonActions.CardBlock(this, play);
        if (Owner.HasPower<OverlimitPower>() || Owner.HasPower<ArcaneArteBoostPower>())
            await CommonActions.CardBlock(this, play);
    }
    
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
        DynamicVars.Block.UpgradeValueBy(3);
    }
}