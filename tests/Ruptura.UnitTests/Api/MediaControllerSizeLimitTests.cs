using FluentAssertions;
using Ruptura.API.Controllers;

namespace Ruptura.UnitTests.Api;

public class MediaControllerSizeLimitTests
{
    [Fact]
    public void ExceedsSizeLimit_WhenMaxIsZero_NeverExceedsRegardlessOfFileLength()
    {
        // 0 = unlimited (Global Constraint) — never treat 0 as "reject everything".
        MediaController.ExceedsSizeLimit(fileLength: long.MaxValue, maxFileSizeMb: 0).Should().BeFalse();
        MediaController.ExceedsSizeLimit(fileLength: 0, maxFileSizeMb: 0).Should().BeFalse();
    }

    [Fact]
    public void ExceedsSizeLimit_WhenFileLengthIsAtOrBelowTheConfiguredLimit_ReturnsFalse()
    {
        const int maxMb = 5;
        var maxBytes = (long)maxMb * 1024 * 1024;

        MediaController.ExceedsSizeLimit(maxBytes, maxMb).Should().BeFalse();
        MediaController.ExceedsSizeLimit(maxBytes - 1, maxMb).Should().BeFalse();
    }

    [Fact]
    public void ExceedsSizeLimit_WhenFileLengthExceedsTheConfiguredLimit_ReturnsTrue()
    {
        const int maxMb = 5;
        var maxBytes = (long)maxMb * 1024 * 1024;

        MediaController.ExceedsSizeLimit(maxBytes + 1, maxMb).Should().BeTrue();
    }
}
