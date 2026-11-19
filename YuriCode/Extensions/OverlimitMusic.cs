using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Audio;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;

namespace Yuri.YuriCode.Extensions;

public static class OverlimitMusic
{
    private const string MusicPath =
        "res://Yuri/sfx/overlimit_music.mp3";

    private const float MusicVolume = -8f;

    private static AudioStreamPlayer? _player;

    public static bool IsPlaying =>
        _player != null &&
        GodotObject.IsInstanceValid(_player) &&
        _player.Playing;

    // ============================================================
    // PLAY
    // ============================================================

    public static void Play(
        ulong netId)
    {
        if (!YuriConfig.EnableOverlimitMusic)
            return;

        /*
         * Synced Power hooks may execute on every client.
         * Only play the music when the Power belongs to this client.
         */
        if (!IsLocalPlayer(netId))
            return;

        /*
         * Do not call the public Stop() here because that would resume
         * vanilla music before immediately stopping it again.
         */
        StopPlayer();

        StopVanillaMusicOnly();

        AudioStream? stream =
            GD.Load<AudioStream>(MusicPath);

        if (stream == null)
        {
            GD.PrintErr(
                $"Failed to load Yuri Overlimit music: {MusicPath}");

            ResumeVanillaMusic();
            return;
        }

        if (stream is AudioStreamMP3 mp3)
        {
            mp3.Loop = true;
        }

        var player =
            new AudioStreamPlayer
            {
                Name = "YuriOverlimitMusic",
                Stream = stream,
                Bus = "Master",
                VolumeDb =
                    ScaleDbByBgmSetting(
                        MusicVolume)
            };

        _player = player;

        player.Finished += () =>
        {
            if (ReferenceEquals(
                    _player,
                    player))
            {
                _player = null;
            }

            if (GodotObject.IsInstanceValid(player))
            {
                player.QueueFree();
            }
        };

        Node? parent =
            NRun.Instance;

        if (parent == null)
        {
            parent =
                NGame.Instance;
        }

        if (parent == null)
        {
            _player = null;

            if (GodotObject.IsInstanceValid(player))
            {
                player.QueueFree();
            }

            ResumeVanillaMusic();
            return;
        }

        parent.AddChild(player);
        player.Play();

        GD.Print(
            $"Playing looping Yuri Overlimit music for local player {netId}: " +
            MusicPath);
    }

    // ============================================================
    // STOP
    // ============================================================

    public static void Stop(
        ulong netId)
    {
        /*
         * Another player's Overlimit ending must not stop the local
         * player's Overlimit music.
         */
        if (!IsLocalPlayer(netId))
            return;

        bool hadPlayer =
            _player != null &&
            GodotObject.IsInstanceValid(_player);

        StopPlayer();

        if (hadPlayer)
        {
            ResumeVanillaMusic();

            GD.Print(
                $"Stopped Yuri Overlimit music for local player {netId}");
        }
    }

    // ============================================================
    // PLAYER CLEANUP
    // ============================================================

    private static void StopPlayer()
    {
        AudioStreamPlayer? player =
            _player;

        /*
         * Clear the static reference first so deferred callbacks cannot
         * process the same player twice.
         */
        _player = null;

        if (player == null)
            return;

        if (!GodotObject.IsInstanceValid(player))
            return;

        player.Stop();
        player.QueueFree();
    }

    // ============================================================
    // LOCAL PLAYER CHECK
    // ============================================================

    private static bool IsLocalPlayer(
        ulong netId)
    {
        return RunManager.Instance != null
            && RunManager.Instance.NetService != null
            && netId ==
               RunManager.Instance.NetService.NetId;
    }

    // ============================================================
    // VOLUME
    // ============================================================

    public static void RefreshVolume()
    {
        if (_player == null)
            return;

        if (!GodotObject.IsInstanceValid(_player))
        {
            _player = null;
            return;
        }

        _player.VolumeDb =
            ScaleDbByBgmSetting(
                MusicVolume);
    }

    private static float ScaleDbByBgmSetting(
        float baseVolumeDb)
    {
        float bgmSetting =
            SaveManager.Instance?
                .SettingsSave?
                .VolumeBgm
            ?? 1f;

        if (bgmSetting <= 0f)
            return -80f;

        float linearVolume =
            Mathf.DbToLinear(
                baseVolumeDb);

        linearVolume *=
            bgmSetting;

        return Mathf.LinearToDb(
            Math.Max(
                linearVolume,
                0.0001f));
    }

    // ============================================================
    // VANILLA MUSIC
    // ============================================================

    private static void StopVanillaMusicOnly()
    {
        NRunMusicController? controller =
            NRunMusicController.Instance;

        if (controller == null)
            return;

        var proxyField =
            AccessTools.Field(
                typeof(NRunMusicController),
                "_proxy");

        if (proxyField?.GetValue(controller) is not Node proxy)
            return;

        proxy.Call(
            "stop_music");
    }

    private static void ResumeVanillaMusic()
    {
        NRunMusicController? controller =
            NRunMusicController.Instance;

        if (controller == null)
            return;

        var proxyField =
            AccessTools.Field(
                typeof(NRunMusicController),
                "_proxy");

        var currentTrackField =
            AccessTools.Field(
                typeof(NRunMusicController),
                "_currentTrack");

        if (proxyField?.GetValue(controller) is not Node proxy)
            return;

        string? currentTrack =
            currentTrackField?.GetValue(controller)
                as string;

        proxy.Call(
            "stop_music");

        if (string.IsNullOrEmpty(currentTrack))
            return;

        proxy.Call(
            "update_music",
            currentTrack);

        proxy.Call(
            "update_global_parameter",
            "Progress",
            7);
    }
}

// ================================================================
// BGM SLIDER SYNC
// ================================================================

[HarmonyPatch(
    typeof(NAudioManager),
    nameof(NAudioManager.SetBgmVol))]
public static class RefreshOverlimitMusicVolumePatch
{
    [HarmonyPostfix]
    public static void Postfix()
    {
        OverlimitMusic.RefreshVolume();
    }
}