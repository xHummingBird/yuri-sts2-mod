using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Yuri.YuriCode.Extensions;
using Yuri.YuriCode.Mechanics;
using Yuri.YuriCode.Relics;

namespace Yuri.YuriCode.Cards.Basic;

public class AzureEdge() : YuriCard(0, CardType.Attack,
    CardRarity.Basic, TargetType.AnyEnemy), IBaseArte
{
    public int ComboGain => 1;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => 
    [
        new DamageVar(3, ValueProp.Move)
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        YuriStaticHoverTips.BaseArte
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var ownerCreature = Owner?.Creature;
        CenterCardCinematic.Start(RunManager.Instance.NetService.NetId);
        if (ownerCreature != null && Owner?.Character is Character.Yuri yuri)
        {
            bool closeToEnemy = false;
            float distance =
                yuri.DistanceToTarget(
                    ownerCreature,
                    play.Target
                );
            
            if (distance < 200f)
                await yuri.YuriDashTo(ownerCreature, play.Target, distance: 250f, forceMove: true, durationSeconds: 0.05f, overrideAnim: "retreat");
            
            int hitCount = 1;
            if (distance <= 300f)
                closeToEnemy = true;
            if (closeToEnemy)
            {
                Owner.GetRelic<ArteRelicBase>()
                    ?.GainCombo(1);
                hitCount = 2;
            }
            else
                await yuri.YuriDashTo(ownerCreature, play.Target, distance: 400f);
            
            SfxCmd.Play("res://Yuri/sounds/artes/souhajin.wav");
            yuri.PlayAnimation(ownerCreature, "souhajin");
            await Task.Delay((int)(0.2f * 1000f));
            SfxCmd.Play("res://Yuri/sfx/swing_2.wav");
            if (closeToEnemy)
            {
                await Task.Delay((int)(0.05f * 1000f));
                yuri.PlayVfxOnTarget(
                    play.Target,
                    "res://Yuri/scenes/vfx.tscn",
                    "hit"
                );
                SfxCmd.Play("res://Yuri/sfx/FromTifa/kick_critical_1.wav");
                SfxCmd.Play("res://Yuri/sfx/hit_2.wav");
                await CommonActions.CardAttack(this, play.Target, hitCount)
                    .WithHitFx(null, null )
                    .Execute(choiceContext);
                await Task.Delay((int)(0.200f * 1000f));
                Owner.GetRelic<ArteRelicBase>()?.GainCombo(1);
            }
            else
            {
                await Task.Delay((int)(0.15f * 1000f));
                await CommonActions.CardAttack(this, play.Target)
                    .WithHitFx(null, "res://Yuri/sfx/FromTifa/kick_critical_1.wav")
                    .Execute(choiceContext);
                await Task.Delay((int)(0.200f * 1000f));
            }
        }
        else
          await CommonActions.CardAttack(this, play.Target)
            .WithHitFx(null, "res://Yuri/sfx/hit_2.wav")
            .Execute(choiceContext);
        CenterCardCinematic.End(RunManager.Instance.NetService.NetId);
        
    }
    
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(1);
    }
}

    // bool moved = await yuri.DashTo(
    //         Owner.Creature,
    //         target,
    //         distance: 200f
    //     );
    //
    //     if (!moved)
    // {
    //     // Yuri was already close enough.
    //     // Apply bonus damage or another proximity effect here.
    // }
