using FluentAssertions;
using Ruptura.Web.Services;
using Xunit;

namespace Ruptura.UnitTests.Web;

public class ConfirmServiceTests
{
    [Fact]
    public void AskAsync_SetsCurrentRequest_AndRaisesOnChange()
    {
        var sut = new ConfirmService();
        var raised = false;
        sut.OnChange += () => raised = true;

        _ = sut.AskAsync("Title", "Message", "Yes", "No");

        sut.Current.Should().NotBeNull();
        sut.Current!.Title.Should().Be("Title");
        raised.Should().BeTrue();
    }

    [Fact]
    public async Task Resolve_True_CompletesTaskWithTrue_AndClearsCurrent()
    {
        var sut = new ConfirmService();
        var task = sut.AskAsync("Delete?", "Sure?", "Delete", "Cancel");

        sut.Resolve(true);

        (await task).Should().BeTrue();
        sut.Current.Should().BeNull();
    }

    [Fact]
    public async Task Resolve_False_CompletesTaskWithFalse()
    {
        var sut = new ConfirmService();
        var task = sut.AskAsync("Delete?", "Sure?", "Delete", "Cancel");

        sut.Resolve(false);

        (await task).Should().BeFalse();
    }

    [Fact]
    public async Task AskAsync_CalledAgainBeforeResolve_CancelsThePriorRequestAsFalse()
    {
        var sut = new ConfirmService();
        var first = sut.AskAsync("First", "m", "Yes", "No");

        var second = sut.AskAsync("Second", "m", "Yes", "No");

        (await first).Should().BeFalse();
        sut.Current!.Title.Should().Be("Second");
        _ = second;
    }
}
