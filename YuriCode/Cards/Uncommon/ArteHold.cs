using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using Yuri.YuriCode.Extensions;
using Yuri.YuriCode.Powers;

namespace Yuri.YuriCode.Cards.Uncommon;

public class ArteHold() : YuriCard(
    0,
    CardType.Skill,
    CardRarity.Uncommon,
    TargetType.Self)
{
protected override IEnumerable<DynamicVar> CanonicalVars =>
[
    new PowerVar<ArteHoldPower>(5),
];

protected override async Task OnPlay(
    PlayerChoiceContext choiceContext,
    CardPlay cardPlay)
{
    AudioHelper.PlayRandomDefend();

    await PowerCmd.Apply<ArteHoldPower>(
        choiceContext,
        base.Owner.Creature,
        DynamicVars["ArteHoldPower"].BaseValue,
        base.Owner.Creature,
        this
    );
    
}

protected override void OnUpgrade()
{
    DynamicVars["ArteHoldPower"].UpgradeValueBy(2m);   
}
}