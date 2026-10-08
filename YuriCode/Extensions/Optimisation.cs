using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace Yuri.YuriCode.Extensions;

public static class YuriAssets
{
    private static PackedScene? _yuriScene;
    private static PackedScene? _vfxScene;

    private const string YuriScenePath = "res://Yuri/scenes/yuri.tscn";
    private const string VfxPath = "res://Yuri/scenes/vfx.tscn";
    
    public static bool IsYuriInRun(IRunState runState)
    {
        return runState?.Players?.Any(
            p => p?.Character is Yuri.YuriCode.Character.Yuri
        ) ?? false;
    }

    public static PackedScene? YuriScene
    {
        get
        {
            _yuriScene = LoadOrReload(_yuriScene, YuriScenePath, "Yuri scene");
            return _yuriScene;
        }
    }

    public static PackedScene? IceScene
    {
        get
        {
            _vfxScene = LoadOrReload(_vfxScene, VfxPath, "Ice VFX");
            return _vfxScene;
        }
    }

    private static PackedScene? LoadOrReload(PackedScene? cachedScene, string path, string label)
    {
        if (cachedScene != null && GodotObject.IsInstanceValid(cachedScene))
            return cachedScene;

        GD.Print($"YuriAssets: Loading {label} from {path}");

        var scene = GD.Load<PackedScene>(path);

        if (scene == null)
        {
            GD.PrintErr($"YuriAssets: FAILED to load {label}: {path}");
            return null;
        }

        GD.Print($"YuriAssets: Loaded {label}");
        return scene;
    }

    public static void EnsurePreloaded()
    {
        _ = YuriScene;
        _ = IceScene;
        
        GD.Print("YuriAssets: EnsurePreloaded finished");
    }
}

[HarmonyPatch(typeof(Hook), nameof(Hook.AfterActEntered))]
public static class YuriAfterActEnteredPreloadPatch
{
    [HarmonyPrefix]
    public static void Prefix(IRunState runState)
    {
        // var player = runState?.Players?.FirstOrDefault();
        //
        // if (player?.Character is not Character.Yuri)
        //     return;
        if (!YuriAssets.IsYuriInRun(runState))
            return;
        
        GD.Print("AfterActEntered: Yuri detected → preloading");

        YuriAssets.EnsurePreloaded();
    }
}


[HarmonyPatch(typeof(Hook), nameof(Hook.AfterRoomEntered))]
public static class YuriAfterRoomEnteredPreloadPatch
{
    [HarmonyPrefix]
    public static void Prefix(IRunState runState, AbstractRoom room)
    {
        // var player = runState?.Players?.FirstOrDefault();
        //
        // if (player?.Character is not Character.Yuri)
        //     return;
        if (!YuriAssets.IsYuriInRun(runState))
            return;
        
        GD.Print($"AfterRoomEntered: Yuri detected → preloading. Room = {room.GetType().Name}");

        YuriAssets.EnsurePreloaded();
    }
}
