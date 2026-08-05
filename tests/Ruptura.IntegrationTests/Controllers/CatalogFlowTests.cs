using System.Net.Http.Json;
using Bogus;
using FluentAssertions;
using Ruptura.IntegrationTests.Helpers;
using Ruptura.Shared.Campaigns;
using Ruptura.Shared.Catalog;
using Ruptura.Shared.Common;

namespace Ruptura.IntegrationTests.Controllers;

public class CatalogFlowTests(IntegrationTestFactory factory)
    : IClassFixture<IntegrationTestFactory>
{
    private static readonly Faker Faker = new();

    [Fact]
    public async Task FullFlow_ReadOfficialCreateHomebrewEditDelete_Succeeds()
    {
        var client = factory.CreateClient();
        var gm = await AuthHelper.RegisterGameMasterAsync(client, Faker.Internet.Email());
        AuthHelper.SetBearerToken(client, gm.AccessToken);

        var campaignResponse = await client.PostAsJsonAsync("api/campaigns", new CreateCampaignRequest
        {
            Name = "Catalog Flow Campaign"
        });
        var campaign = (await campaignResponse.Content
            .ReadFromJsonAsync<ApiResponse<CampaignResponse>>())!.Data!;

        // 1. Official skills are visible immediately (seeded data).
        var skillsResponse = await client.GetAsync($"api/catalog?type=Skill&campaignId={campaign.Id}");
        var skills = (await skillsResponse.Content
            .ReadFromJsonAsync<ApiResponse<IEnumerable<CatalogEntryResponse>>>())!.Data!.ToList();
        skills.Should().Contain(s => s.Name == "Espadas" && s.IsGlobal);

        // 2. Create a homebrew Talent.
        var createResponse = await client.PostAsJsonAsync("api/catalog", new CreateCatalogEntryRequest
        {
            CampaignId = campaign.Id,
            Type = "Talent",
            Name = "Coragem Inabalável",
            DataJson = "{\"Category\":\"Combate\",\"Effect\":\"teste\",\"PowerTier\":\"menor\"}"
        });
        var created = (await createResponse.Content.ReadFromJsonAsync<ApiResponse<CatalogEntryResponse>>())!.Data!;
        created.IsGlobal.Should().BeFalse();

        // 3. Edit it.
        var updateResponse = await client.PutAsJsonAsync($"api/catalog/{created.Id}", new UpdateCatalogEntryRequest
        {
            Name = "Coragem Inabalável (Revisado)", DataJson = created.DataJson
        });
        updateResponse.EnsureSuccessStatusCode();

        var talentsResponse = await client.GetAsync($"api/catalog?type=Talent&campaignId={campaign.Id}");
        var talents = (await talentsResponse.Content
            .ReadFromJsonAsync<ApiResponse<IEnumerable<CatalogEntryResponse>>>())!.Data!;
        talents.Should().Contain(t => t.Name == "Coragem Inabalável (Revisado)");

        // 4. Delete it.
        var deleteResponse = await client.DeleteAsync($"api/catalog/{created.Id}");
        deleteResponse.EnsureSuccessStatusCode();

        var talentsAfterDelete = (await (await client.GetAsync($"api/catalog?type=Talent&campaignId={campaign.Id}")).Content
            .ReadFromJsonAsync<ApiResponse<IEnumerable<CatalogEntryResponse>>>())!.Data!;
        talentsAfterDelete.Should().NotContain(t => t.Id == created.Id);
    }
}
