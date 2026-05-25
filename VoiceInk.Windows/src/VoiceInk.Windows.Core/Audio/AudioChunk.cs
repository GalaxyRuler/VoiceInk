namespace VoiceInk.Windows.Core.Audio;

public sealed record AudioChunk
{
    public AudioChunk(byte[] pcm16Bytes, int sampleRate, int channelCount)
    {
        Pcm16Bytes = pcm16Bytes.ToArray();
        SampleRate = sampleRate;
        ChannelCount = channelCount;
    }

    public byte[] Pcm16Bytes { get; }

    public int SampleRate { get; }

    public int ChannelCount { get; }
}
