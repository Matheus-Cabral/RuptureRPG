using FluentAssertions;
using Microsoft.Extensions.Options;
using Ruptura.Infrastructure.Services;
using Ruptura.Infrastructure.Settings;

namespace Ruptura.UnitTests.Infrastructure;

public class LocalFileStorageServiceTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "ruptura-media-tests-" + Guid.NewGuid());
    private readonly LocalFileStorageService _sut;

    public LocalFileStorageServiceTests()
    {
        _sut = new LocalFileStorageService(Options.Create(new MediaSettings { RootPath = _root }));
    }

    public void Dispose()
    {
        if (Directory.Exists(_root)) Directory.Delete(_root, recursive: true);
    }

    [Fact]
    public async Task SaveAsync_ThenOpenReadAsync_RoundTripsTheSameBytes()
    {
        var bytes = "hello world"u8.ToArray();
        await _sut.SaveAsync(new MemoryStream(bytes), "character-sheets/abc/portrait-1.jpg");

        await using var stream = await _sut.OpenReadAsync("character-sheets/abc/portrait-1.jpg");
        stream.Should().NotBeNull();
        using var reader = new StreamReader(stream!);
        (await reader.ReadToEndAsync()).Should().Be("hello world");
    }

    [Fact]
    public async Task OpenReadAsync_WhenFileDoesNotExist_ReturnsNull()
    {
        var result = await _sut.OpenReadAsync("character-sheets/does-not-exist/x.jpg");
        result.Should().BeNull();
    }

    [Fact]
    public void Exists_ReflectsWhetherTheFileIsOnDisk()
    {
        _sut.Exists("character-sheets/abc/nope.jpg").Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_RemovesTheFile()
    {
        await _sut.SaveAsync(new MemoryStream("x"u8.ToArray()), "journal-entries/e1/img.png");
        await _sut.DeleteAsync("journal-entries/e1/img.png");

        _sut.Exists("journal-entries/e1/img.png").Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_WhenFileDoesNotExist_DoesNotThrow()
    {
        var act = async () => await _sut.DeleteAsync("journal-entries/nope/nope.png");
        await act.Should().NotThrowAsync();
    }

    [Theory]
    [InlineData("../escape.jpg")]
    [InlineData("character-sheets/../../escape.jpg")]
    [InlineData("character-sheets/abc/../../../escape.jpg")]
    public async Task SaveAsync_RejectsPathTraversal(string maliciousPath)
    {
        var act = async () => await _sut.SaveAsync(new MemoryStream("x"u8.ToArray()), maliciousPath);
        await act.Should().ThrowAsync<ArgumentException>();
    }
}
