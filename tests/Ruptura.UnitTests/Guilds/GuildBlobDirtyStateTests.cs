using FluentAssertions;
using Ruptura.Shared.Guilds;
using Xunit;

namespace Ruptura.UnitTests.Guilds;

public class GuildBlobDirtyStateTests
{
    [Fact] public void Clean_WhenBlobAndNameMatchBaseline() =>
        GuildBlobDirtyState.IsDirty("{\"a\":1}", "{\"a\":1}", "Guild", "Guild").Should().BeFalse();

    [Fact] public void Dirty_WhenBlobDiffers() =>
        GuildBlobDirtyState.IsDirty("{\"a\":2}", "{\"a\":1}", "Guild", "Guild").Should().BeTrue();

    [Fact] public void Dirty_WhenNameDiffers() =>
        GuildBlobDirtyState.IsDirty("{\"a\":1}", "{\"a\":1}", "New Name", "Guild").Should().BeTrue();
}
