using FluentAssertions;
using Ruptura.Web.Services;
using Xunit;

namespace Ruptura.UnitTests.Web;

public class ToastServiceTests
{
    [Fact]
    public void Show_AddsMessageAndRaisesOnChange()
    {
        var sut = new ToastService();
        var raised = false;
        sut.OnChange += () => raised = true;

        sut.Show("Saved", ToastLevel.Success);

        sut.Messages.Should().ContainSingle(m => m.Text == "Saved" && m.Level == ToastLevel.Success);
        raised.Should().BeTrue();
    }

    [Fact]
    public void Success_UsesSuccessLevel()
    {
        var sut = new ToastService();

        sut.Success("Done");

        sut.Messages.Single().Level.Should().Be(ToastLevel.Success);
    }

    [Fact]
    public void Error_UsesErrorLevel()
    {
        var sut = new ToastService();

        sut.Error("Failed");

        sut.Messages.Single().Level.Should().Be(ToastLevel.Error);
    }

    [Fact]
    public void Dismiss_RemovesMessageById_AndRaisesOnChange()
    {
        var sut = new ToastService();
        sut.Show("Bye");
        var id = sut.Messages.Single().Id;
        var raised = false;
        sut.OnChange += () => raised = true;

        sut.Dismiss(id);

        sut.Messages.Should().BeEmpty();
        raised.Should().BeTrue();
    }

    [Fact]
    public void Dismiss_UnknownId_DoesNothing()
    {
        var sut = new ToastService();
        sut.Show("Stays");

        sut.Dismiss(Guid.NewGuid());

        sut.Messages.Should().ContainSingle();
    }
}
