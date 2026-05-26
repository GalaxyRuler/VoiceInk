using VoiceInk.Windows.Core.Audio;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.Audio;

public sealed class AudioInputPriorityListTests
{
    [Fact]
    public void Add_AppendsDeviceAndNormalizesPriorities()
    {
        var devices = new[]
        {
            new PrioritizedAudioInputDevice("Dock Microphone", 5)
        };

        var result = AudioInputPriorityList.Add(devices, "USB Microphone");

        Assert.Equal(
            [
                new PrioritizedAudioInputDevice("Dock Microphone", 0),
                new PrioritizedAudioInputDevice("USB Microphone", 1)
            ],
            result);
    }

    [Fact]
    public void Add_IgnoresDuplicateNames()
    {
        var devices = new[]
        {
            new PrioritizedAudioInputDevice("USB Microphone", 0)
        };

        var result = AudioInputPriorityList.Add(devices, "USB Microphone");

        Assert.Equal(devices, result);
    }

    [Fact]
    public void Remove_RemovesDeviceAndNormalizesPriorities()
    {
        var devices = new[]
        {
            new PrioritizedAudioInputDevice("Dock Microphone", 0),
            new PrioritizedAudioInputDevice("USB Microphone", 1),
            new PrioritizedAudioInputDevice("Built-in Microphone", 2)
        };

        var result = AudioInputPriorityList.Remove(devices, "USB Microphone");

        Assert.Equal(
            [
                new PrioritizedAudioInputDevice("Dock Microphone", 0),
                new PrioritizedAudioInputDevice("Built-in Microphone", 1)
            ],
            result);
    }

    [Fact]
    public void MoveUp_SwapsWithPreviousDevice()
    {
        var devices = new[]
        {
            new PrioritizedAudioInputDevice("Dock Microphone", 0),
            new PrioritizedAudioInputDevice("USB Microphone", 1)
        };

        var result = AudioInputPriorityList.MoveUp(devices, "USB Microphone");

        Assert.Equal(
            [
                new PrioritizedAudioInputDevice("USB Microphone", 0),
                new PrioritizedAudioInputDevice("Dock Microphone", 1)
            ],
            result);
    }

    [Fact]
    public void MoveDown_SwapsWithNextDevice()
    {
        var devices = new[]
        {
            new PrioritizedAudioInputDevice("Dock Microphone", 0),
            new PrioritizedAudioInputDevice("USB Microphone", 1)
        };

        var result = AudioInputPriorityList.MoveDown(devices, "Dock Microphone");

        Assert.Equal(
            [
                new PrioritizedAudioInputDevice("USB Microphone", 0),
                new PrioritizedAudioInputDevice("Dock Microphone", 1)
            ],
            result);
    }
}
