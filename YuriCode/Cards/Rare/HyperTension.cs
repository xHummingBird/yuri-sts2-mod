using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Yuri.YuriCode.Extensions;
using Yuri.YuriCode.Mechanics;
using Yuri.YuriCode.Powers;

namespace Yuri.YuriCode.Cards.Rare;

public class HyperTension() : YuriCard(0, CardType.Skill,
    CardRarity.Rare, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];
    
    protected override IEnumerable<DynamicVar> CanonicalVars => 
    [
        new DynamicVar("Overlimit", 30)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<OverlimitPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        AudioHelper.PlayRandomDefend();
        OverlimitManager.GainOverlimit(Owner, DynamicVars["Overlimit"].IntValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Overlimit"].UpgradeValueBy(10);
    }
}