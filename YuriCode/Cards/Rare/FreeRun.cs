using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Yuri.YuriCode.Extensions;
using Yuri.YuriCode.Mechanics;
using Yuri.YuriCode.Powers;

namespace Yuri.YuriCode.Cards.Rare;

public class FreeRun() : YuriCard(2, CardType.Skill,
    CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(15, ValueProp.Move),
        new DynamicVar("Overlimit", 15),
        new EnergyVar(2)
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<OverlimitPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        AudioHelper.PlayRandomDefend();
        await CommonActions.CardBlock(this, play);
        
        var ownerCreature = Owner?.Creature;
        OverlimitManager.GainOverlimit(Owner, DynamicVars["Overlimit"].IntValue);
        await PowerCmd.Apply<EnergyNextTurnPower>(choiceContext, ownerCreature, 2, ownerCreature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Block"].UpgradeValueBy(5m);
        DynamicVars["Overlimit"].UpgradeValueBy(5);
    }
}