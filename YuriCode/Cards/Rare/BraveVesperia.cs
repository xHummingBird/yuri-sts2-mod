using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Yuri.YuriCode.Cards;
using Yuri.YuriCode.Extensions;
using Yuri.YuriCode.Powers;

namespace Yuri.YuriCode.Cards.Rare;

public class BraveVesperia() : YuriCard(
    2,
    CardType.Power,
    CardRarity.Rare,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<BraveVesperiaPower>(1),
        new EnergyVar(1)
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        AudioHelper.PlayRandomDefend();

        (await PowerCmd.Apply<BraveVesperiaPower>(choiceContext, base.Owner.Creature, DynamicVars["BraveVesperiaPower"].BaseValue, base.Owner.Creature, this))?.IncrementOverlimitPerTurn();
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}