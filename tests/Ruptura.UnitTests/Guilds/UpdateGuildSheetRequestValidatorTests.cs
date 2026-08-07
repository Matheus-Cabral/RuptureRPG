using System.Text.Json;
using FluentAssertions;
using Ruptura.Application.Validators.Guilds;
using Ruptura.Shared.Guilds;

namespace Ruptura.UnitTests.Guilds;

public class UpdateGuildSheetRequestValidatorTests
{
    private readonly UpdateGuildSheetRequestValidator _validator = new();
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    private static UpdateGuildSheetRequest Request(string dataJson) =>
        new() { GuildName = "Guild", DataJson = dataJson, Version = 0 };

    [Fact]
    public void WithWellFormedBlob_Succeeds()
    {
        var json = JsonSerializer.Serialize(new GuildSheetData(), JsonOpts);
        _validator.Validate(Request(json)).IsValid.Should().BeTrue();
    }

    [Fact]
    public void WithEmptyObject_Succeeds()
    {
        // "{}" deserializes to a GuildSheetData whose modules/lists keep their initializers.
        _validator.Validate(Request("{}")).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("garbage")]
    [InlineData("\"garbage\"")]
    [InlineData("[]")]
    [InlineData("123")]
    [InlineData("null")]
    public void WithMalformedOrWrongShapeJson_Fails(string dataJson)
    {
        _validator.Validate(Request(dataJson)).IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("""{"resources":{"materials":[null]}}""")]
    [InlineData("""{"influence":[null]}""")]
    [InlineData("""{"legado":[null]}""")]
    public void WithNullListElements_Fails(string dataJson)
    {
        _validator.Validate(Request(dataJson)).IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("""{"identity":null}""")]
    [InlineData("""{"resources":null}""")]
    [InlineData("""{"knowledge":{"maps":null}}""")]
    [InlineData("""{"influence":null}""")]
    public void WithNullModulesOrLists_Fails(string dataJson)
    {
        _validator.Validate(Request(dataJson)).IsValid.Should().BeFalse();
    }

    [Fact]
    public void WithEmptyDataJson_Fails()
    {
        _validator.Validate(Request("")).IsValid.Should().BeFalse();
    }

    [Fact]
    public void WithEmptyGuildName_Fails()
    {
        var json = JsonSerializer.Serialize(new GuildSheetData(), JsonOpts);
        _validator.Validate(new UpdateGuildSheetRequest { GuildName = "", DataJson = json, Version = 0 })
            .IsValid.Should().BeFalse();
    }
}
