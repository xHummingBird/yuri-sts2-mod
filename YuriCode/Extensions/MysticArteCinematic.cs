using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace Yuri.YuriCode.Extensions;

public static class MysticArteCinematic
{
    private static bool IsLocalPlayer(ulong netId)
    {
        return RunManager.Instance != null
               && RunManager.Instance.NetService != null
               && netId == RunManager.Instance.NetService.NetId;
    }

    private static readonly Dictionary<CanvasItem, Color>
        OriginalModulates = new();

    private static bool _isActive;

    public static async Task Start(
        ulong netId,
        Creature yuriCreature)
    {
        if (!IsLocalPlayer(netId))
            return;

        if (_isActive)
            return;

        _isActive = true;

        CenterCardCinematic.Start(netId);

        var combatRoom = NCombatRoom.Instance;

        if (combatRoom == null)
        {
            _isActive = false;
            return;
        }

        var yuriNode =
            combatRoom.GetCreatureNode(yuriCreature);

        var itemsToFade =
            new List<CanvasItem>();
        
        foreach (var creatureNode in combatRoom.CreatureNodes)
        {
            if (creatureNode == yuriNode)
                continue;

            var visual =
                creatureNode.Visuals?
                    .GetNodeOrNull<CanvasItem>("Visuals");

            if (visual != null)
            {
                if (!OriginalModulates.ContainsKey(visual))
                    OriginalModulates[visual] =
                        visual.Modulate;

                itemsToFade.Add(visual);
            }

            var hpBar =
                creatureNode.GetNodeOrNull<CanvasItem>("%HealthBar");

            if (hpBar != null)
            {
                if (!OriginalModulates.ContainsKey(hpBar))
                    OriginalModulates[hpBar] =
                        hpBar.Modulate;

                itemsToFade.Add(hpBar);
            }
            
            var intents =
                creatureNode.GetNodeOrNull<CanvasItem>("Intents");

            if (intents != null)
            {
                if (!OriginalModulates.ContainsKey(intents))
                {
                    OriginalModulates[intents] =
                        intents.Modulate;
                }

                itemsToFade.Add(intents);
            }
        }
        
        await FadeItems(
            itemsToFade,
            Colors.Transparent,
            0.3f
        );
    }

    private static async Task FadeItems(
        IEnumerable<CanvasItem> items,
        Color targetColor,
        float duration)
    {
        var combatRoom = NCombatRoom.Instance;

        if (combatRoom == null)
        {
            _isActive = false;
            return;
        }

        var tween =
            combatRoom.CreateTween()
                .SetParallel();

        bool anyItems = false;

        foreach (var item in items)
        {
            if (item == null ||
                !GodotObject.IsInstanceValid(item))
                continue;

            anyItems = true;

            tween.TweenProperty(
                item,
                "modulate",
                targetColor,
                duration
            );
        }

        if (!anyItems)
            return;

        await combatRoom.ToSignal(
            tween,
            Tween.SignalName.Finished
        );
    }

    public static async Task End(
        ulong netId)
    {
        if (!IsLocalPlayer(netId))
            return;

        if (!_isActive)
            return;

        var combatRoom = NCombatRoom.Instance;

        if (combatRoom != null)
        {
            var tween =
                combatRoom
                    .CreateTween()
                    .SetParallel();

            foreach (var pair in OriginalModulates)
            {
                if (!GodotObject.IsInstanceValid(pair.Key))
                    continue;

                tween.TweenProperty(
                    pair.Key,
                    "modulate",
                    pair.Value,
                    0.2f
                );
            }

            await combatRoom.ToSignal(
                tween,
                Tween.SignalName.Finished
            );
        }

        OriginalModulates.Clear();

        CenterCardCinematic.End(netId);

        _isActive = false;
    }
    
    public static async Task FadeTarget(
        Creature target,
        bool fadeIn,
        float duration = 0.1f)
    {
        var combatRoom = NCombatRoom.Instance;

        if (combatRoom == null)
            return;

        var targetNode =
            combatRoom.GetCreatureNode(target);

        if (targetNode?.Visuals == null)
            return;

        var visual =
            targetNode.Visuals
                .GetNodeOrNull<CanvasItem>("Visuals");

        if (visual == null)
            return;

        if (!OriginalModulates.ContainsKey(visual))
        {
            OriginalModulates[visual] =
                visual.Modulate;
        }

        var tween =
            combatRoom.CreateTween();

        tween.TweenProperty(
            visual,
            "modulate",
            fadeIn
                ? OriginalModulates[visual]
                : Colors.Transparent,
            duration
        );

        await combatRoom.ToSignal(
            tween,
            Tween.SignalName.Finished
        );
    }
}