namespace Ruptura.Shared.Guilds;

// 8 GDD guild stages (§10.8), by floors conquered. Unaccented identifiers; UI localizes display.
// Order IS the stage index (Fundacao = 0 .. Divina = 7).
public enum GuildStage
{
    Fundacao,
    Menor,
    Regional,
    Reconhecida,
    Maior,
    Renomada,
    Lendaria,
    Divina
}
