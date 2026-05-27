using VoiceInk.Windows.Core.Shell;
using Xunit;

namespace VoiceInk.Windows.Core.Tests.Shell;

public sealed class TrayIconRecoveryPolicyTests
{
    [Fact]
    public void ShouldRecover_OnlyForRegisteredTaskbarCreatedMessage()
    {
        Assert.False(TrayIconRecoveryPolicy.ShouldRecover(message: 0x0312, taskbarCreatedMessage: 0xC001));
        Assert.False(TrayIconRecoveryPolicy.ShouldRecover(message: 0xC001, taskbarCreatedMessage: 0));
        Assert.True(TrayIconRecoveryPolicy.ShouldRecover(message: 0xC001, taskbarCreatedMessage: 0xC001));
    }
}
