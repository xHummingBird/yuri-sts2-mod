using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Yuri.YuriCode.Extensions;

namespace Yuri.YuriCode.Cards.Basic;

public class Backstep() : YuriCard(1, CardType.Skill,
    CardRarity.Basic, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(4, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        AudioHelper.PlayRandomDefend();
        await CommonActions.CardBlock(this, play);
        
        var ownerCreature = Owner?.Creature;

        if (ownerCreature != null && Owner?.Character is Character.Yuri yuri)
        {
            var startedInFormation = yuri.IsAtCombatHomePosition(ownerCreature);
            if (!startedInFormation)
            {
                await yuri.ReturnToCombatHome(Owner.Creature);
                await CommonActions.CardBlock(this, play);
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Block"].UpgradeValueBy(2m);
    }
}