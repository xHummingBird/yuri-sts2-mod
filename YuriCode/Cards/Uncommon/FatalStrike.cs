using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Yuri.YuriCode.Extensions;
using Yuri.YuriCode.Powers;

namespace Yuri.YuriCode.Cards.Uncommon;

public class FatalStrike() : YuriCard(1, CardType.Skill,
    CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => 
    [
        new PowerVar<FatalStrikePower>(18m)
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<FatalStrikePower>()
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await PowerCmd.Apply<FatalStrikePower>(choiceContext, play.Target, DynamicVars["FatalStrikePower"].BaseValue, Owner.Creature, this);
        
        var ownerCreature = Owner?.Creature;
        
        if (FatalStrikePower.WillExecute(play.Target))
            if (ownerCreature != null && Owner?.Character is Character.Yuri yuri)
            {
                float distance =
                    yuri.DistanceToTarget(
                        ownerCreature,
                        play.Target
                    );

                if (distance < 200f)
                    await yuri.YuriDashTo(ownerCreature, play.Target, distance: 250f, forceMove: true,
                        durationSeconds: 0.05f, overrideAnim: "retreat");

                await yuri.YuriDashTo(ownerCreature, play.Target);
                AudioHelper.PlayRandomAttackHard();
                yuri.PlayAnimation(ownerCreature, "fatal_strike", true);
                await Task.Delay((int)(0.1f * 1000f));
                SfxCmd.Play("res://Yuri/sfx/swing_1.wav");
                yuri.YuriDashTo(ownerCreature, play.Target, distance: -250f, forceMove: true, durationSeconds: 0.1f,
                    overrideAnim: null);
                await Task.Delay((int)(0.05f * 1000f));
                yuri.PlayVfxOnTarget(
                    play.Target,
                    "res://Yuri/scenes/vfx.tscn",
                    "fatal_strike"
                );
                await FatalStrikePower.CheckAfterCardApplication(
                    play.Target
                );
                await Task.Delay((int)(0.25f * 1000f));
                await yuri.Retreat(ownerCreature);
            }
            else await FatalStrikePower.CheckAfterCardApplication(
                play.Target
            );;
    }
    
    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}