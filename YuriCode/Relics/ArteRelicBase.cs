using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;
using Yuri.YuriCode.Cards.Ancient;
using Yuri.YuriCode.Character;
using Yuri.YuriCode.Mechanics;
using Yuri.YuriCode.Powers;

namespace Yuri.YuriCode.Relics;

public abstract class ArteRelicBase : YuriRelic
{
    private int _combo;

    public override RelicRarity Rarity => RelicRarity.Starter;

    public override bool ShowCounter =>
        CombatManager.Instance.IsInProgress;

    public override int DisplayAmount => Combo;
    
    [SavedProperty]
    public int StoredOverlimit { get; set; }

    [SavedProperty]
    public int PendingRewardCombo { get; set; }

    private bool _addedComboGoldReward;

    protected virtual int OverlimitPerTurn => BaseOverlimitPerTurn;
    
    protected virtual int BaseOverlimitPerTurn => 3;

    protected virtual int OverlimitPerCard => 2;
    
    public int Combo => _combo;

    public virtual int MaxCombo => 999;

    private int ComboInternal
    {
        get => _combo;
        set
        {
            AssertMutable();

            _combo = Math.Clamp(
                value,
                0,
                MaxCombo);

            UpdateDisplay();
        }
    }
    
    protected virtual int ModifyOverlimitGain(
        CardModel card,
        int amount)
    {
        return amount;
    }
    
    protected virtual int ModifyTurnStartOverlimitGain(int amount)
    {
        return amount;
    }

    public override Task BeforeCombatStart()
    {
        ComboInternal = 0;
        Status = RelicStatus.Normal;
        _addedComboGoldReward = false;
        int startingOverlimit = StoredOverlimit;

        if (Owner.GetRelic<Estoc>() != null)
            startingOverlimit += 10;

        if (Owner?.Character is Character.Yuri yuri)
        {
            yuri.ForgetCombatHomePosition();
            yuri.RememberCombatHomePosition(
                Owner.Creature,
                overwrite: true);
        }

        OverlimitManager.SetOverlimit(
            Owner,
            startingOverlimit
        );

        return Task.CompletedTask;
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        /*
         * Capture the final Combo before resetting it.
         * This value is used for post-combat reward thresholds.
         */
        PendingRewardCombo = Combo;

        /*
         * 50+ Combo grants exactly 20 Overlimit for the next combat.
         * Remaining Combo is no longer converted directly into Overlimit.
         */
        StoredOverlimit =
            PendingRewardCombo >= 40
                ? 20
                : 0;

        ComboInternal = 0;
        Status = RelicStatus.Normal;

        _addedComboGoldReward = false;

        if (Owner.Character is Character.Yuri yuri)
        {
            yuri.ForgetCombatHomePosition();
        }

        return Task.CompletedTask;
    }
    
    public override async Task AfterSideTurnStartLate(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (side != Owner.Creature.Side)
            return;

        if (Owner.Creature.IsDead)
            return;

        int amount = ModifyTurnStartOverlimitGain(
            OverlimitPerTurn);

        amount += (5 * Owner.Creature.GetPowerAmount<BraveVesperiaPower>());

        if (amount > 0)
        {
            OverlimitManager.GainOverlimit(
                Owner,
                amount);
        }

        await OverlimitManager.CheckOverlimitReady(
            Owner.Creature,
            null,
            Owner.Creature,
            null);
    }

    public override async Task AfterCardPlayed(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        // if (!CombatManager.Instance.IsInProgress)
        //     return;

        CardModel card = cardPlay.Card;

        if (card.Owner != Owner)
            return;
        
        int gainPerCard = OverlimitPerCard;
        
        int comboGain = GetComboGainFromCard(card);

        if (comboGain > 0)
        {
            comboGain = ModifyComboGain(
                card,
                comboGain);
        }

        comboGain = Math.Max(0, comboGain);

        if (comboGain > 0)
        {
            GainCombo(comboGain);
        }

        if (!CombatManager.Instance.IsInProgress)
            return;

        int overlimitGain = gainPerCard + comboGain;

        overlimitGain = ModifyOverlimitGain(
            card,
            overlimitGain);

        if (overlimitGain > 0)
        {
            OverlimitManager.GainOverlimit(
                Owner,
                overlimitGain);
        }

        await OverlimitManager.CheckOverlimitReady(
            Owner.Creature,
            choiceContext,
            Owner.Creature,
            card);

        if (card is IBaseArte)
            await PowerCmd.Apply<ArcaneArteBoostPower>(choiceContext, Owner.Creature, 1, null, card, false);
        
        if (Owner.HasPower<OverlimitPower>() && card is IArcaneArte)
            await PowerCmd.Apply<MysticArteBoostPower>(choiceContext, Owner.Creature, 1, null, card, true);
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != Owner.Creature.Side)
            return;

        if (Owner.Creature.IsDead)
            return;

        if (Owner.Character is Character.Yuri yuri)
        {
            await yuri.ReturnToCombatHome(
                Owner.Creature);
        }
    }

    private int GetComboGainFromCard(CardModel card)
    {
        if (card.Type != CardType.Attack)
            return 0;

        /*
         * Artes use their explicitly defined hit count rather than RepeatVar.
         * Check Arcane Arte first in case an Arcane Arte also implements
         * IBaseArte.
         */
        if (card is IArcaneArte arcaneArte)
        {
            return Math.Max(
                0,
                arcaneArte.ComboGain);
        }

        if (card is IBaseArte baseArte)
        {
            return Math.Max(
                0,
                baseArte.ComboGain);
        }

        /*
         * Ordinary Attacks gain Combo from RepeatVar.
         * If there is no RepeatVar, they count as one hit.
         */
        int repeat = TryGetRepeatVarFromCard(card) ?? 1;

        return Math.Max(1, repeat);
    }

    protected virtual int ModifyComboGain(
        CardModel card,
        int amount)
    {
        return amount;
    }

    private int? TryGetRepeatVarFromCard(CardModel card)
    {
        object? dynamicVars = GetPropertyValue(
            card,
            "DynamicVars");

        if (dynamicVars == null)
            return null;

        object? repeatVar = GetPropertyValue(
            dynamicVars,
            "Repeat");

        if (repeatVar == null)
            return null;

        object? intValue = GetPropertyValue(
            repeatVar,
            "IntValue");

        return intValue is int value
            ? value
            : null;
    }

    private static object? GetPropertyValue(
        object instance,
        string propertyName)
    {
        try
        {
            PropertyInfo? property = instance
                .GetType()
                .GetProperty(
                    propertyName,
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic);

            return property?.GetValue(instance);
        }
        catch
        {
            return null;
        }
    }

    public void GainCombo(int amount)
    {
        if (amount <= 0)
            return;

        ComboInternal += amount;
    }

    public void LoseCombo(int amount)
    {
        if (amount <= 0)
            return;

        ComboInternal -= amount;
    }

    public void SetCombo(int amount)
    {
        ComboInternal = amount;
    }

    public void ConsumeCombo(int amount)
    {
        LoseCombo(amount);
    }

    public void ConsumeAllCombo()
    {
        ComboInternal = 0;
    }

    public int SpendAllCombo()
    {
        int spent = Combo;

        ConsumeAllCombo();

        return spent;
    }

    public bool HasCombo(int amount)
    {
        return Combo >= amount;
    }

    public int GetComboForUI()
    {
        return Combo;
    }

    public int GetMaxComboForUI()
    {
        return MaxCombo;
    }

    private void UpdateDisplay()
    {
        Status = Combo > 0
            ? RelicStatus.Active
            : RelicStatus.Normal;

        InvokeDisplayAmountChanged();
    }
    
    public override bool TryModifyRewards(
        Player player,
        List<Reward> rewards,
        AbstractRoom? room)
    {
        if (player != Owner)
            return false;

        if (room == null)
            return false;

        if (!room.RoomType.IsCombatRoom())
            return false;

        /*
         * Avoid adding the reward more than once if STS2 rebuilds
         * or rechecks the reward list.
         */
        if (_addedComboGoldReward)
            return false;

        if (PendingRewardCombo < 20)
            return false;

        rewards.Add(
            new GoldReward(
                10,
                player
            )
        );

        _addedComboGoldReward = true;

        return true;
    }

    public override bool ShouldForcePotionReward(
        Player player,
        RoomType roomType)
    {
        if (player != Owner)
            return false;

        if (!roomType.IsCombatRoom())
            return false;

        return PendingRewardCombo >= 60;
    }
    
    public override bool TryModifyCardRewardOptions(
    Player player,
    List<CardCreationResult> rewardOptions,
    CardCreationOptions creationOptions)
{
    if (player != Owner)
        return false;

    /*
     * Requires at least 100 Combo from the combat
     * that generated this reward.
     */
    if (PendingRewardCombo < 80)
        return false;

    /*
     * Only modify encounter-generated card rewards.
     */
    if (creationOptions.Source != CardCreationSource.Encounter)
       return false;

    if (!creationOptions.Flags.HasFlag(
            CardCreationFlags.IsCardReward))
    {
        return false;
    }

    if (!creationOptions.Flags.HasFlag(
            CardCreationFlags.IsFromCombat))
    {
        return false;
    }

    /*
     * First attempt to avoid adding an Arte that is already
     * present in the reward options.
     */
    bool allowDupes = false;

    List<CardModel> possibleCards =
        creationOptions
            .GetPossibleCards(player)
            .ToList();

    IEnumerable<CardModel> eligibleCards =
        possibleCards.Where(card =>
            ArteCardPoolFilter(
                card,
                rewardOptions,
                allowDupes: false
            )
        );

    /*
     * If every available Arte is already represented,
     * allow a duplicate rather than losing the bonus option.
     */
    if (!eligibleCards.Any())
    {
        allowDupes = true;

        eligibleCards =
            possibleCards.Where(card =>
                ArteCardPoolFilter(
                    card,
                    rewardOptions,
                    allowDupes: true
                )
            );
    }

    if (!eligibleCards.Any())
        return false;

    CardCreationOptions arteOptions =
        new CardCreationOptions(
            creationOptions.CardPools,
            CardCreationSource.Other,
            creationOptions.RarityOdds,
            card =>
            {
                Func<CardModel, bool>? originalFilter =
                    creationOptions.CardPoolFilter;

                bool passesOriginalFilter =
                    originalFilter == null ||
                    originalFilter(card);

                return passesOriginalFilter &&
                       ArteCardPoolFilter(
                           card,
                           rewardOptions,
                           allowDupes
                       );
            }
        )
        .WithFlags(
            CardCreationFlags.NoModifyHooks |
            CardCreationFlags.NoCardPoolModifications
        );

    CardModel? arteCard =
        CardFactory.CreateForReward(
                Owner,
                1,
                arteOptions
            )
            .FirstOrDefault()
            ?.Card;

    if (arteCard == null)
        return false;

    var result =
        new CardCreationResult(arteCard);

    result.ModifyCard(
        arteCard,
        this
    );

    rewardOptions.Add(result);

    return true;
}

private static bool ArteCardPoolFilter(
    CardModel card,
    List<CardCreationResult> rewardOptions,
    bool allowDupes)
{
    bool isArte =
        card is IBaseArte ||
        card is IArcaneArte;

    if (!isArte)
        return false;

    if (allowDupes)
        return true;

    return rewardOptions.TrueForAll(
        option =>
            option.originalCard.Id != card.Id
    );
}
}