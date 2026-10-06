using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Yuri.YuriCode.Extensions;
using Yuri.YuriCode.Mechanics;
using Yuri.YuriCode.Powers;

namespace Yuri.YuriCode.Cards.Common;

public class SonicThrust() : YuriCard(1, CardType.Attack,
    CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => 
    [
        new DamageVar(7, ValueProp.Move),
        new DynamicVar("Overlimit", 4)
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<OverlimitPower>()
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var ownerCreature = Owner?.Creature;
        bool initialPosition = true;
        if (ownerCreature != null && Owner?.Character is Character.Yuri yuri)
        {
            if (!yuri.IsAtCombatHomePosition(Owner.Creature))
                initialPosition = false;
            
            float distance =
                yuri.DistanceToTarget(
                    ownerCreature,
                    play.Target
                );
            
            if (distance < 200f)
                await yuri.YuriDashTo(ownerCreature, play.Target, distance: 250f, forceMove: true, durationSeconds: 0.05f, overrideAnim: "retreat");
            else
                await yuri.YuriDashTo(ownerCreature, play.Target);
            AudioHelper.PlayRandomAttack();
            yuri.PlayAnimation(ownerCreature, "thrust", true);
            await Task.Delay((int)(0.1f * 1000f));
            SfxCmd.Play("res://Yuri/sfx/swing_2.wav");
            await Task.Delay((int)(0.033f * 1000f));
            yuri.PlayVfxOnTarget(
                play.Target,
                "res://Yuri/scenes/vfx.tscn",
                "hit"
            );
            yuri.PlayVfxOnTarget(
                play.Target,
                "res://Yuri/scenes/vfx.tscn",
                "hit"
            );
        }
        await CommonActions.CardAttack(this, play.Target)
            .WithHitFx(null, "res://Yuri/sfx/hit_3.wav")
            .Execute(choiceContext);
        OverlimitManager.GainOverlimit(Owner, DynamicVars["Overlimit"].IntValue);
        if (initialPosition)
            OverlimitManager.GainOverlimit(Owner, DynamicVars["Overlimit"].IntValue);
    }
    
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
        DynamicVars["Overlimit"].UpgradeValueBy(2);
    }
}