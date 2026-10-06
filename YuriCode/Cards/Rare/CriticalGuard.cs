using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Yuri.YuriCode.Extensions;

namespace Yuri.YuriCode.Cards.Rare;

public class CriticalGuard() : YuriCard(
    1,
    CardType.Skill,
    CardRarity.Rare,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(
            8,
            ValueProp.Move)
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        AudioHelper.PlayRandomDefend();

        await CommonActions.CardBlock(
            this,
            play);

        if (Owner?.Character is Character.Yuri yuri &&
            yuri.IsEnemyNearby(
                Owner.Creature))
        {
            await CommonActions.CardBlock(
                this,
                play);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3);
    }
}