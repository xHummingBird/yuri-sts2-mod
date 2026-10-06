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

namespace Yuri.YuriCode.Cards.Ancient;

public class AzureStorm() : YuriCard(0, CardType.Attack,
    CardRarity.Ancient, TargetType.AnyEnemy), IBaseArte
{
    public int ComboGain => 2;
    
    protected override IEnumerable<DynamicVar> CanonicalVars => 
    [
        new DamageVar(4, ValueProp.Move)
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
            
            int hitCount = 2;
            if (distance <= 300f)
                closeToEnemy = true;
            if (closeToEnemy)
            {
                Owner.GetRelic<ArteRelicBase>()
                    ?.GainCombo(2);
                hitCount = 4;
            }
            else
                await yuri.YuriDashTo(ownerCreature, play.Target, distance: 400f);
            
            SfxCmd.Play("res://Yuri/sounds/artes/azure_storm.wav");
            yuri.PlayAnimation(ownerCreature, "azure_storm");
            int currentHit = 0;
            await Task.Delay((int)(0.1f * 1000f));
            if (closeToEnemy)
            {
                await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                    .WithHitCount(hitCount)
                    .FromCard(this, play)
                    .Targeting(play.Target)
                    .WithNoAttackerAnim()
                    //.WithWaitBeforeHit(0.1f, 0.1f)
                    .WithHitVfxNode(target =>
                    {
                        yuri.PlayVfxOnTarget(
                            target,
                            "res://Yuri/scenes/vfx.tscn",
                            "hit"
                        );

                        if (currentHit % 2 == 0)
                        {
                            SfxCmd.Play("res://Yuri/sfx/hit_2.wav");
                            SfxCmd.Play("res://Yuri/sfx/swing_2.wav");
                        }
                        else
                            SfxCmd.Play("res://Yuri/sfx/FromTifa/kick_critical_1.wav");

                        currentHit++;
                        return null;
                    }).Execute(choiceContext);
            }
            
            else
            {
                await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                    .WithHitCount(hitCount)
                    .FromCard(this, play)
                    .Targeting(play.Target)
                    .WithNoAttackerAnim()
                    .WithWaitBeforeHit(0.1f, 0.1f)
                    .WithHitVfxNode(target =>
                    {
                        yuri.PlayVfxOnTarget(
                            target,
                            "res://Yuri/scenes/vfx.tscn",
                            "hit"
                        );
                        return null;
                    })
                    .BeforeDamage(async () =>
                    {
                        SfxCmd.Play("res://Yuri/sfx/swing_2.wav");
                        await Task.Delay(100);
                        SfxCmd.Play("res://Yuri/sfx/FromTifa/kick_critical_1.wav");
                    })
                    .Execute(choiceContext);
            }
        }
        else
          await CommonActions.CardAttack(this, play.Target, hitCount:2)
            .WithHitFx(null, "res://Yuri/sfx/hit_2.wav")
            .Execute(choiceContext);
        CenterCardCinematic.End(RunManager.Instance.NetService.NetId);
        
    }
    
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(1);
    }
}