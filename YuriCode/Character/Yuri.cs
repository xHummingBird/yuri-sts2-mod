using BaseLib.Abstracts;
using BaseLib.Utils.NodeFactories;
using Yuri.YuriCode.Extensions;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Yuri.YuriCode.Cards.Basic;
using Yuri.YuriCode.Cards.Common;
using Yuri.YuriCode.Relics;

namespace Yuri.YuriCode.Character;

public class Yuri : PlaceholderCharacterModel
{
    public const string CharacterId = "Yuri";

    public static readonly Color Color = new("ffffff");
    
    private Vector2? _originalPosition;
    private Vector2? _combatHomePosition;
    private bool _isReturningHome;

    private const float PositionTolerance = 2f;

    public override Color NameColor => Color;
    public override CharacterGender Gender => CharacterGender.Masculine;
    public override int StartingHp => 73;

    public override IEnumerable<CardModel> StartingDeck =>
    [
        ModelDb.Card<StrikeYuri>(),
        ModelDb.Card<StrikeYuri>(),
        ModelDb.Card<StrikeYuri>(),
        ModelDb.Card<StrikeYuri>(),
        ModelDb.Card<AzureEdge>(),
        ModelDb.Card<AzureWolfStrike>(),
        ModelDb.Card<DefendYuri>(),
        ModelDb.Card<DefendYuri>(),
        ModelDb.Card<DefendYuri>(),
        ModelDb.Card<DefendYuri>(),
    ];

    public override IReadOnlyList<RelicModel> StartingRelics =>
    [
        ModelDb.Relic<SecondStar>()
    ];

    public override CardPoolModel CardPool => ModelDb.CardPool<YuriCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<YuriRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<YuriPotionPool>();

    /*  PlaceholderCharacterModel will utilize placeholder basegame assets for most of your character assets until you
        override all the other methods that define those assets.
        These are just some of the simplest assets, given some placeholders to differentiate your character with.
        You don't have to, but you're suggested to rename these images. */
    public override Control CustomIcon
    {
        get
        {
            var icon = NodeFactory<Control>.CreateFromResource(CustomIconTexturePath);
            icon.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            return icon;
        }
    }
    
    public override CustomEnergyCounter? CustomEnergyCounter =>
        new CustomEnergyCounter(EnergyCounterPaths, new Color(0.2f, 0.2f, 0.2f), new Color(1f, 1f, 1f));
    
    private string EnergyCounterPaths(int i)
    {
        return i switch
        {
            1 => "charui/big_energy.png".ImagePath(),
            _ => "charui/blank.png".ImagePath()
        };
    }

    public override string CustomIconTexturePath => "character_icon_yuri.png".CharacterUiPath();
    public override string CustomCharacterSelectIconPath => "char_select_yuri.png".CharacterUiPath();
    public override string CustomCharacterSelectLockedIconPath => "char_select_char_name_locked.png".CharacterUiPath();
    public override string CustomMapMarkerPath => "map_marker_yuri.png".CharacterUiPath();
    
    private const string CustomVisualScenePath = "res://Yuri/scenes/yuri.tscn";
    public override string CustomRestSiteAnimPath => "res://Yuri/scenes/yuri_rest_site.tscn";
    
    public override string CustomCharacterSelectBg => "res://Yuri/images/charui/char_selection_bg_yuri.tscn";
    public override string CustomMerchantAnimPath => "res://Yuri/scenes/yuri_merchant.tscn";
    public override string CharacterSelectSfx => "res://Yuri/sounds/run_start.wav";
    
    public override NCreatureVisuals? CreateCustomVisuals()
    {
        YuriAssets.EnsurePreloaded();
        return NodeFactory<NCreatureVisuals>.CreateFromScene(CustomVisualScenePath);
    }
    
    // public override CreatureAnimator? GenerateAnimator(MegaSprite controller) => null;
    
    public (float total, float[] impacts) PlayAnimation(
        Creature creature,
        string trigger,
        bool? forceRestart = null)
    {
        if (creature == null || string.IsNullOrEmpty(trigger))
            return (0f, Array.Empty<float>());

        var node = NCombatRoom.Instance?.GetCreatureNode(creature);

        if (node?.Visuals == null)
            return (0f, Array.Empty<float>());

        var animPlayer =
            node.Visuals.GetNodeOrNull<AnimationPlayer>(
                "AnimationPlayer"
            );

        if (animPlayer == null)
            return (0f, Array.Empty<float>());

        string godotTrigger = trigger.ToLowerInvariant() switch
        {
            "hit" => "hurt",
            "idle" => "idle",
            "attack" => "attack_yuri",
            "dead" => "die",
            "die" => "die",
            _ => trigger
        };

        if (!animPlayer.HasAnimation(godotTrigger))
            return (0f, Array.Empty<float>());

        var anim =
            animPlayer.GetAnimation(godotTrigger);

        float totalLength =
            (float)anim.Length;

        if (forceRestart == true &&
            animPlayer.CurrentAnimation == godotTrigger)
        {
            animPlayer.Stop();
        }

        animPlayer.Play(godotTrigger);

        if (godotTrigger != "idle" &&
            godotTrigger != "die")
        {
            animPlayer.Queue("idle");
        }

        return (totalLength, Array.Empty<float>());
    }
    
    
    
    public float DistanceToTarget(
        Creature player,
        Creature target)
    {
        var node = NCombatRoom.Instance?.GetCreatureNode(player);
        var targetNode = NCombatRoom.Instance?.GetCreatureNode(target);

        if (node == null || targetNode == null)
            return float.MaxValue;

        return Mathf.Abs(
            node.GlobalPosition.X -
            targetNode.GlobalPosition.X
        );
    }
    
    public async Task DashTo(
        Creature player,
        Creature target,
        float durationSeconds = 0.3f,
        float distance = 200f,
        bool dashBehind = false,
        string? overrideAnim = null)
    {
        var node = NCombatRoom.Instance?.GetCreatureNode(player);
        var targetNode = NCombatRoom.Instance?.GetCreatureNode(target);
        if (node == null || targetNode == null) return;

        if (!_originalPosition.HasValue)
            _originalPosition = node.GlobalPosition;
        
        PlayAnimation(player, overrideAnim ??"dash");
		
        bool playerIsLeftOfTarget = node.GlobalPosition.X < targetNode.GlobalPosition.X;
		
        Vector2 offsetDir = playerIsLeftOfTarget ? Vector2.Left : Vector2.Right;
		
        if (dashBehind)
            offsetDir = -offsetDir;

        Vector2 targetPos = targetNode.GlobalPosition + offsetDir * distance;

        var tween = node.CreateTween();
        tween.TweenProperty(node, "global_position", targetPos, durationSeconds)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.Out);

        await node.ToSignal(tween, Tween.SignalName.Finished);
    }
    
    public void RememberCombatHomePosition(
        Creature creature,
        bool overwrite = false)
    {
        var node = NCombatRoom.Instance?.GetCreatureNode(creature);
        if (node == null)
            return;

        if (!_combatHomePosition.HasValue || overwrite)
            _combatHomePosition = node.Position;
    }

    public void ForgetCombatHomePosition()
    {
        _combatHomePosition = null;
        _isReturningHome = false;
    }

    public bool IsAtCombatHomePosition(Creature creature)
    {
        if (!_combatHomePosition.HasValue)
            return true;

        var node = NCombatRoom.Instance?.GetCreatureNode(creature);
        if (node == null)
            return true;

        return node.Position.DistanceTo(_combatHomePosition.Value)
               <= PositionTolerance;
    }
    
    public async Task DashPast(
        Creature player,
        Creature target,
        string? attackAnim = null,
        float durationSeconds = 0.3f,
        float behindDistance = 200f,
        float overshoot = 0f)
    {
        var node = NCombatRoom.Instance?.GetCreatureNode(player);
        var targetNode = NCombatRoom.Instance?.GetCreatureNode(target);
        if (node == null || targetNode == null) return;

        if (!_originalPosition.HasValue)
            _originalPosition = node.GlobalPosition;

        Vector2 frontDir = (player.Side == CombatSide.Player) ? Vector2.Left : Vector2.Right;
        Vector2 behindDir = -frontDir;

        Vector2 endPos = targetNode.GlobalPosition + behindDir * (behindDistance + overshoot);

        PlayAnimation(player, attackAnim);

        var tween = node.CreateTween();
        tween.TweenProperty(node, "global_position", endPos, durationSeconds)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.Out);

        await node.ToSignal(tween, Tween.SignalName.Finished);
    }
    
    public bool IsCommitted(Creature creature)
    {
        return !IsAtCombatHomePosition(creature);
    }
    
    public bool NeedsMovement(
        Creature player,
        Creature target,
        float desiredDistance)
    {
        var playerNode = NCombatRoom.Instance?.GetCreatureNode(player);
        var targetNode = NCombatRoom.Instance?.GetCreatureNode(target);

        if (playerNode == null || targetNode == null)
            return false;

        return playerNode.GlobalPosition.DistanceTo(
            targetNode.GlobalPosition
        ) > desiredDistance;
    }
    
    public async Task ReturnToCombatHome(
        Creature player,
        string? animation = "retreat",
        bool goIdle = true,
        float durationSeconds = 0.3f)
    {
        GD.Print($"Returning To: {_combatHomePosition}");
        
        if (_isReturningHome)
            return;

        var node = NCombatRoom.Instance?.GetCreatureNode(player);

        if (node == null || !_combatHomePosition.HasValue)
            return;

        if (node.Position.DistanceTo(_combatHomePosition.Value)
            <= PositionTolerance)
        {
            node.Position = _combatHomePosition.Value;

            if (goIdle)
                PlayAnimation(player, "idle");

            return;
        }

        _isReturningHome = true;
        
        

        try
        {
            if (!string.IsNullOrEmpty(animation))
                PlayAnimation(player, animation);

            var tween = node.CreateTween();

            tween.TweenProperty(
                    node,
                    "position",
                    _combatHomePosition.Value,
                    durationSeconds
                )
                .SetTrans(Tween.TransitionType.Quad)
                .SetEase(Tween.EaseType.InOut);

            await node.ToSignal(tween, Tween.SignalName.Finished);

            node.Position = _combatHomePosition.Value;

            if (goIdle)
                PlayAnimation(player, "idle");
        }
        finally
        {
            _isReturningHome = false;
        }
    }
    
    public Control PlayCutIn(string animName)
    {
        var scene = GD.Load<PackedScene>(
            "res://Yuri/scenes/cutin.tscn");

        var cutIn = scene.Instantiate<Control>();

        NGame.Instance.AddChild(cutIn);

        var visualAnchor =
            cutIn.GetNode<Node2D>("Visuals");

        var viewportSize =
            cutIn.GetViewportRect().Size;

        visualAnchor.Position =
            viewportSize / 2f;

        var animPlayer =
            cutIn.GetNode<AnimationPlayer>("AnimationPlayer");

        if (animPlayer.HasAnimation(animName))
            animPlayer.Play(animName);

        return cutIn;
    }
    
    public async Task<bool> YuriDashTo(
        Creature player,
        Creature target,
        float durationSeconds = 0.2f,
        float distance = 250f,
        float tolerance = 50f,
        bool forceMove = false,
        bool forceLeftSide = true,
        string? overrideAnim = null)
    {
        var node = NCombatRoom.Instance?.GetCreatureNode(player);
        var targetNode = NCombatRoom.Instance?.GetCreatureNode(target);

        if (node == null || targetNode == null)
            return false;

        float currentDistance =
            Mathf.Abs(node.GlobalPosition.X - targetNode.GlobalPosition.X);

        bool playerIsLeftOfTarget =
            node.GlobalPosition.X < targetNode.GlobalPosition.X;

        bool ignoreForceLeft =
            target.Monster is Crusher ||
            target.Monster is Rocket;

        bool needsSideCorrection =
            forceLeftSide &&
            !ignoreForceLeft &&
            !playerIsLeftOfTarget;

        bool needsDistanceCorrection =
            currentDistance <= Mathf.Abs(distance) + tolerance;

        if (!forceMove &&
            needsDistanceCorrection &&
            !needsSideCorrection)
        {
            return false;
        }

        Vector2 offsetDir;

        if (forceLeftSide && !ignoreForceLeft)
        {
            // Always stand on enemy's left
            offsetDir = Vector2.Left;
        }
        else
        {
            // Existing dynamic behaviour for Crusher/Rocket
            offsetDir =
                playerIsLeftOfTarget
                    ? Vector2.Left
                    : Vector2.Right;
        }

        Vector2 targetPos =
            targetNode.GlobalPosition + offsetDir * distance;

        PlayAnimation(player, overrideAnim ?? "dash");

        var tween = node.CreateTween();

        tween.TweenProperty(
                node,
                "global_position",
                targetPos,
                durationSeconds
            )
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.Out);

        await node.ToSignal(tween, Tween.SignalName.Finished);

        return true;
    }
    
    public async Task BounceEnemy(
        Creature enemy,
        float height = 200f,
        float upTime = 0.15f,
        float downTime = 0.2f)
    {
        var node = NCombatRoom.Instance?.GetCreatureNode(enemy);

        if (node?.Visuals == null)
            return;

        var visuals = node.Visuals;
        visuals.Position = Vector2.Zero;

        var tween = visuals.CreateTween();

        tween.TweenProperty(
                visuals,
                "position",
                Vector2.Up * height,
                upTime)
            .SetEase(Tween.EaseType.Out);

        tween.TweenProperty(
                visuals,
                "position",
                Vector2.Zero,
                downTime)
            .SetEase(Tween.EaseType.In);

        await visuals.ToSignal(
            tween,
            Tween.SignalName.Finished);
    }
    
    public async Task LaunchEnemy(
        Creature enemy,
        float height = 250f,
        float duration = 0.2f)
    {
        var node = NCombatRoom.Instance?.GetCreatureNode(enemy);

        if (node?.Visuals == null)
            return;

        var tween = node.Visuals.CreateTween();

        tween.TweenProperty(
                node.Visuals,
                "position",
                Vector2.Up * height,
                duration)
            .SetEase(Tween.EaseType.Out);

        await node.Visuals.ToSignal(
            tween,
            Tween.SignalName.Finished);
    }
    
    public async Task DropEnemy(
        Creature enemy,
        float duration = 0.25f)
    {
        var node = NCombatRoom.Instance?.GetCreatureNode(enemy);

        if (node?.Visuals == null)
            return;

        var tween = node.Visuals.CreateTween();

        tween.TweenProperty(
                node.Visuals,
                "position",
                Vector2.Zero,
                duration)
            .SetEase(Tween.EaseType.In);

        await node.Visuals.ToSignal(
            tween,
            Tween.SignalName.Finished);
    }
    
    public async Task Retreat(
        Creature player,
        string? animation = "retreat",
        bool goIdle = true,
        float duration = 0.3f)
    {
        var node = NCombatRoom.Instance?.GetCreatureNode(player);
        if (node == null || !_originalPosition.HasValue) return;

        if (!string.IsNullOrEmpty(animation))
            PlayAnimation(player, animation);

        var tween = node.CreateTween();
        tween.TweenProperty(node, "global_position", _originalPosition.Value, duration)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.InOut);

        await node.ToSignal(tween, Tween.SignalName.Finished);

        _originalPosition = null;

        var visuals = node.Visuals.GetNodeOrNull<Node2D>("Visuals");
        if (visuals != null)
            visuals.Position = Vector2.Zero;

        if (goIdle)
            PlayAnimation(player, "idle");
    }
    
    public void DoScreenShake(ShakeStrength strength = ShakeStrength.Medium,
        ShakeDuration duration = ShakeDuration.Short)
    {
        NGame.Instance?.ScreenShake(strength, duration);
    }

    public Node2D PlayVfxOnTarget(Creature target, string path, string animName)
    {
        var targetNode = NCombatRoom.Instance?.GetCreatureNode(target);
        if (targetNode?.Visuals == null)
            return null;

        var scene = GD.Load<PackedScene>(path);
        var vfx = scene.Instantiate<Node2D>();

        targetNode.Visuals.AddChild(vfx);
        vfx.Position = Vector2.Zero;

        var animPlayer = vfx.GetNode<AnimationPlayer>("AnimationPlayer");

        if (animPlayer.HasAnimation(animName))
            animPlayer.Play(animName);

        return vfx;
    }
    
    [HarmonyPatch(typeof(NCreature), nameof(NCreature.SetAnimationTrigger))]
    public static class NCreatureSetTriggerPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(NCreature __instance, string trigger)
        {
            // This ensures the engine's triggers automatically drive your AnimationPlayer.
            if (__instance.Entity?.Player?.Character is Yuri character)
            {
                character.PlayAnimation(__instance.Entity, trigger);
                return false; // skip default skeletal animation path
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(NCreature), nameof(NCreature.StartDeathAnim))]
    public static class YuriStartDeathAnimPatch
    {
        [HarmonyPostfix]
        public static void Postfix(NCreature __instance, ref float __result)
        {
            if (__instance.Entity?.Player?.Character is Yuri character)
            {
                AudioHelper.PlayRandomGameover();
                character.PlayAnimation(__instance.Entity, "die");
                var animPlayer = __instance.Visuals.GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
                __result = animPlayer?.GetAnimation("die")?.Length ?? 1.5f;
            }
        }
    }
    
    [HarmonyPatch(typeof(Hook), nameof(Hook.AfterCombatVictory))]
    public static class YuriVictoryAnimationPatch
    {
        [HarmonyPostfix]
        public static void Postfix(IRunState runState, CombatState? combatState)
        {
            var creatures = combatState?.Creatures?.Where(c => c.IsPlayer);

            if (creatures == null)
                return;

            foreach (var creature in creatures)
            {
                if (creature.Player?.Character is not Yuri)
                    continue;

                var node = NCombatRoom.Instance?.GetCreatureNode(creature);
                var animPlayer = node?.Visuals?.GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
                
                if (animPlayer == null)
                    continue;
                AudioHelper.PlayRandomVictory();
                animPlayer.Play("victory");
            }
        }
    }
    
   [HarmonyPatch(typeof(Hook), nameof(Hook.AfterDamageReceived))]
    public static class YuriDamageAnimationPatch
    {
        [HarmonyPostfix]
        public static void Postfix(Creature target, DamageResult result, ValueProp props, Creature? dealer)
        {
            if (target.Player?.Character is not Yuri character)
                return;
            
            if (dealer == null || dealer.Side != CombatSide.Enemy)
                return;
            
            if (props.HasFlag(ValueProp.SkipHurtAnim) || props.HasFlag(ValueProp.Unpowered))
                return;

            if (result.WasFullyBlocked && result.BlockedDamage > 0)
            {
                character.PlayAnimation(target, "block"); 
            }
            
            else if (result.UnblockedDamage > 0 && !target.IsDead)
            {
                character.PlayAnimation(target, "hit");
                if (target.CurrentHp < 20)
                {
                    AudioHelper.PlayRandomDamagedCritical();
                }
                else if (result.UnblockedDamage < 10)
                {
                    AudioHelper.PlayRandomDamaged();
                }
                else
                {
                    AudioHelper.PlayRandomDamagedHigh();
                }
            }
        }
    }
}
