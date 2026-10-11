using Godot;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Audio;
using MegaCrit.Sts2.Core.Saves;

namespace Yuri.YuriCode.Extensions;

public static class OverlimitMusic
{
    private const string MusicPath =
        "res://Yuri/sfx/overlimit_music.mp3";

    private static AudioStreamPlayer? _player;

    public static void Play()
    {
        Stop();

        // Mute game BGM
        NAudioManager.Instance?.SetBgmVol(0f);

        var stream = GD.Load<AudioStream>(MusicPath);

        if (stream == null)
        {
            GD.PrintErr($"Failed to load music: {MusicPath}");
            return;
        }

        _player = new AudioStreamPlayer
        {
            Stream = stream,
            Name = "YuriOverlimitMusic",
            Bus = "SFX",

            // Volume control:
            //  0f = full volume
            // -3f = ~70%
            // -6f = ~50%
            // -10f = quiet
            VolumeDb = -8f
        };

        NRun.Instance?.AddChild(_player);

        _player.Play();
    }

    public static void Stop()
    {
        if (_player != null &&
            GodotObject.IsInstanceValid(_player))
        {
            _player.Stop();
            _player.QueueFree();
        }

        _player = null;

        // Restore user's BGM setting
        if (SaveManager.Instance?.SettingsSave != null)
        {
            NAudioManager.Instance?.SetBgmVol(
                SaveManager.Instance.SettingsSave.VolumeBgm
            );
        }
    }
}