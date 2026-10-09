using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Yuri.YuriCode.Extensions;

namespace Yuri.YuriCode.Cards.Ancient;

public class SkipAnimation() : YuriCard(0, CardType.Skill,
    CardRarity.Token, TargetType.Self)
{
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