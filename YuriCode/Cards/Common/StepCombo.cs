using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Yuri.YuriCode.Extensions;

namespace Yuri.YuriCode.Cards.Common;

public class StepCombo() : YuriCard(
    1,
    CardType.Attack,
    CardRarity.Common,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CalculationBaseVar(0m),
        new ExtraDamageVar(1m),
        new CalculatedDamageVar(ValueProp.Move)
            .WithMultiplier(
                (card, _) => card.Owner.Creature.Block)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.Static(
            StaticHoverTip.Block)
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(
            play.Target);

        var ownerCreature =
            Owner?.Creature;

        if (ownerCreature != null &&
            Owner?.Character is Character.Yuri yuri)
        {
            await yuri.YuriDashTo(
                ownerCreature,
                play.Target);

            AudioHelper.PlayRandomAttack();

            yuri.PlayAnimation(
                ownerCreature,
                "attack",
                true);

            await Task.Delay(
                (int)(0.1167f * 1000f));

            yuri.PlayVfxOnTarget(
                play.Target,
                "res://Yuri/scenes/vfx.tscn",
                "hit");
        }

        await DamageCmd.Attack(
                DynamicVars.CalculatedDamage)
            .FromCard(this, play)
            .Targeting(play.Target)
            .WithHitFx(
                null,
                null,
                "res://Yuri/sfx/hit_2.wav")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}