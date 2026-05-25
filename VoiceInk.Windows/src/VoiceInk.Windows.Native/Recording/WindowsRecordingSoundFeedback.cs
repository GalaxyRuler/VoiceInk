using System.Media;
using NAudio.Wave;
using VoiceInk.Windows.Core.Recording;

namespace VoiceInk.Windows.Native.Recording;

public sealed class WindowsRecordingSoundFeedback : IRecordingSoundFeedback, IDisposable
{
    private readonly object playbackGate = new();
    private readonly List<ActivePlayback> activePlaybacks = [];

    public void PlayStartSound(RecordingSoundPlaybackSettings settings) =>
        Play(settings, SystemSounds.Asterisk);

    public void PlayStopSound(RecordingSoundPlaybackSettings settings) =>
        Play(settings, SystemSounds.Exclamation);

    private void Play(RecordingSoundPlaybackSettings settings, SystemSound fallbackSound)
    {
        if (settings.UsesCustomSound
            && File.Exists(settings.CustomSoundPath)
            && TryPlayCustomSound(settings.CustomSoundPath))
        {
            return;
        }

        PlaySystemSound(fallbackSound);
    }

    private bool TryPlayCustomSound(string filePath)
    {
        AudioFileReader? reader = null;
        WaveOutEvent? output = null;
        ActivePlayback? playback = null;

        try
        {
            reader = new AudioFileReader(filePath);
            output = new WaveOutEvent();
            output.Init(reader);
            playback = new ActivePlayback(reader, output);
            reader = null;
            output = null;

            playback.Output.PlaybackStopped += (_, _) =>
            {
                lock (playbackGate)
                {
                    activePlaybacks.Remove(playback);
                }

                playback.Dispose();
            };

            lock (playbackGate)
            {
                activePlaybacks.Add(playback);
            }

            playback.Output.Play();
            return true;
        }
        catch
        {
            lock (playbackGate)
            {
                if (playback is not null)
                {
                    activePlaybacks.Remove(playback);
                }
            }

            playback?.Dispose();
            output?.Dispose();
            reader?.Dispose();
            return false;
        }
    }

    private static void PlaySystemSound(SystemSound sound)
    {
        try
        {
            sound.Play();
        }
        catch
        {
            // Sound feedback should never block recording.
        }
    }

    public void Dispose()
    {
        ActivePlayback[] playbacks;
        lock (playbackGate)
        {
            playbacks = activePlaybacks.ToArray();
            activePlaybacks.Clear();
        }

        foreach (var playback in playbacks)
        {
            playback.Dispose();
        }
    }

    private sealed class ActivePlayback(AudioFileReader reader, WaveOutEvent output) : IDisposable
    {
        private int disposed;

        public WaveOutEvent Output { get; } = output;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref disposed, 1) != 0)
            {
                return;
            }

            Output.Dispose();
            reader.Dispose();
        }
    }
}
