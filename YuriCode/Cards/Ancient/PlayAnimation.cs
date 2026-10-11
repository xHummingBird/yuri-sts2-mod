using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Yuri.YuriCode.Extensions;

namespace Yuri.YuriCode.Cards.Ancient;

public class PlayAnimation() : YuriCard(-1, CardType.Status,
    CardRarity.Status, TargetType.None)
{
    public override bool CanBeGeneratedInCombat => false;
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
   
    }

    protected override void OnUpgrade()
    {
        
    }
}
    // bool returned = await yuri.ReturnToCombatHome(owner);
    //
    //     if (returned)
    // {
    //     GainBlock(Block);
    // }