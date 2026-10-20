using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Audio;
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

    public static void Play()
    {
        /*
         * Do not call Stop() here because Stop() resumes vanilla music.
         * We only need to clean up the previous Overlimit player.
         */
        StopPlayer();

        /*
         * Stops only the game's current FMOD music.
         * This does not change the user's BGM setting and does not mute SFX.
         */
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

                /*
                 * Use Master rather than SFX.
                 * Its volume is manually scaled using the BGM setting.
                 */
                Bus = "Master",

                VolumeDb =
                    ScaleDbByBgmSetting(
                        MusicVolume)
            };

        _player = player;

        player.Finished += () =>
        {
            if (ReferenceEquals(_player, player))
            {
                _player = null;
            }

            if (GodotObject.IsInstanceValid(player))
            {
                player.QueueFree();
            }
        };

        Node? parent = NRun.Instance;

        if (parent == null)
            parent = NGame.Instance;

        if (parent == null)
        {
            _player = null;
            player.QueueFree();

            ResumeVanillaMusic();
            return;
        }

        parent.AddChild(player);
        player.Play();

        GD.Print(
            $"Playing looping Yuri Overlimit music: {MusicPath}");
    }

    public static void Stop()
    {
        bool wasPlaying =
            _player != null &&
            GodotObject.IsInstanceValid(_player);

        StopPlayer();

        /*
         * Only resume vanilla music if this class actually had an
         * Overlimit music player. This prevents repeated Stop() calls
         * from restarting the current track.
         */
        if (wasPlaying)
        {
            ResumeVanillaMusic();
        }

        GD.Print(
            "Stopped Yuri Overlimit music");
    }

    private static void StopPlayer()
    {
        AudioStreamPlayer? player = _player;

        /*
         * Clear the field first so deferred callbacks cannot process
         * the same player twice.
         */
        _player = null;

        if (player == null)
            return;

        if (!GodotObject.IsInstanceValid(player))
            return;

        player.Stop();
        player.QueueFree();
    }

    public static void RefreshVolume()
    {
        if (_player == null)
            return;

        if (!GodotObject.IsInstanceValid(_player))
            return;

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
            Mathf.DbToLinear(baseVolumeDb);

        linearVolume *= bgmSetting;

        return Mathf.LinearToDb(
            Math.Max(
                linearVolume,
                0.0001f));
    }

    private static void StopVanillaMusicOnly()
    {
        NRunMusicController? controller =
            NRunMusicController.Instance;

        if (controller == null)
            return;

        Node? proxy =
            AccessTools.Field(
                    typeof(NRunMusicController),
                    "_proxy")
                .GetValue(controller) as Node;

        proxy?.Call("stop_music");
    }

    private static void ResumeVanillaMusic()
    {
        NRunMusicController? controller =
            NRunMusicController.Instance;

        if (controller == null)
            return;

        Node? proxy =
            AccessTools.Field(
                    typeof(NRunMusicController),
                    "_proxy")
                .GetValue(controller) as Node;

        string? currentTrack =
            AccessTools.Field(
                    typeof(NRunMusicController),
                    "_currentTrack")
                .GetValue(controller) as string;

        proxy?.Call("stop_music");

        if (string.IsNullOrEmpty(currentTrack))
            return;

        proxy?.Call(
            "update_music",
            currentTrack);

        proxy?.Call(
            "update_global_parameter",
            "Progress",
            7);
    }
}

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