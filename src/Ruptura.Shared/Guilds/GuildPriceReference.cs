namespace Ruptura.Shared.Guilds;

public record GuildBasePrice(string Key, int BasePrice);

public static class GuildPriceReference
{
    // GDD §10.6.1 base prices in Prata. `Key` is a resx key suffix (Guild.Price.<Key>).
    public static readonly IReadOnlyList<GuildBasePrice> Items =
    [
        new("Ration", 1),        // Ração de comida (1 dia)
        new("Lodging", 2),       // Estadia simples (1 noite)
        new("LaborerWage", 3),   // Salário diário — Operário
        new("ArtisanWage", 8),   // Salário diário — Artesão/Pesquisador
    ];
}
