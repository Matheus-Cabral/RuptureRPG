namespace Ruptura.Shared.Guilds;

// Everything GuildStatsCalculator computes — never persisted, recomputed on read.
public class GuildDerivedStats
{
    public GuildStage Stage { get; set; }
    public int StageIndex { get; set; }               // 0..7, == (int)Stage

    public int Cg { get; set; }                        // total Capacidade da Guilda
    public int CgInfra { get; set; }
    public int CgPesquisa { get; set; }
    public int CgLogistica { get; set; }
    public int CgRecursos { get; set; }

    public int Cs { get; set; }                        // Capacidade de Suporte (doctrine-adjusted)
    public int Ci { get; set; }                        // Capacidade Institucional
    public int Cf { get; set; }                        // Capacidade de Formação

    public decimal InflationIndex { get; set; }        // doctrine-adjusted (Comercial -1 stage)

    public int DailyMaintenance { get; set; }          // doctrine-adjusted (Logística -10%)
    public int WorkerIncomePerDay { get; set; }        // Operário count × 2 Prata/day

    public int StorageCapacity { get; set; }           // Armazém level × 50
    public int ResidencyCapacity { get; set; }         // Dormitório level × 2

    public int DoctrineLimit { get; set; }             // 2 + Câmara do Conselho level, capped 4
    public int ActiveDoctrineCount { get; set; }
    public bool ActiveDoctrineOverflow { get; set; }   // ActiveDoctrineCount > DoctrineLimit (advisory)

    public int ActiveBuildingCount { get; set; }       // constructible, IsActive
    public bool ActiveBuildingOverflow { get; set; }   // ActiveBuildingCount > Cs
}
