using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Ruptura.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedOriginsAndBackgrounds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "CatalogEntries",
                columns: new[] { "Id", "CampaignId", "CreatedAt", "CreatedByGameMasterId", "DataJson", "Name", "Type", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("10000000-0000-0000-0000-000000000001"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"MainBenefit\":\"-1 dificuldade em testes de Disciplina/forma\\u00E7\\u00E3o em combate organizado\",\"PrimarySkill\":\"Espadas\",\"SecondarySkill\":\"Armaduras\",\"StartingEquipment\":\"Espada curta, armadura leve\",\"NarrativeHook\":\"Desertou ou foi dispensado de uma for\\u00E7a militar local\"}", "Soldado", 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-000000000002"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"MainBenefit\":\"-1 dificuldade em Rastreamento na natureza\",\"PrimarySkill\":\"Rastreamento\",\"SecondarySkill\":\"Arcos\",\"StartingEquipment\":\"Arco simples, capa\",\"NarrativeHook\":\"Vive das terras selvagens h\\u00E1 anos\"}", "Caçador", 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-000000000003"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"MainBenefit\":\"Pode identificar qualidade de materiais sem teste\",\"PrimarySkill\":\"Ferraria\",\"SecondarySkill\":\"Avalia\\u00E7\\u00E3o\",\"StartingEquipment\":\"Ferramentas de artes\\u00E3o\",\"NarrativeHook\":\"Aprendeu um of\\u00EDcio com um mestre exigente\"}", "Artesão", 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-000000000004"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"MainBenefit\":\"\\u002B1 recupera\\u00E7\\u00E3o extra em descanso longo\",\"PrimarySkill\":\"Sobreviv\\u00EAncia\",\"SecondarySkill\":\"Conhecimento de Animais\",\"StartingEquipment\":\"Foice, roupas simples\",\"NarrativeHook\":\"Cresceu trabalhando a terra\"}", "Camponês", 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-000000000005"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"MainBenefit\":\"1x por interl\\u00FAdio, resolve uma d\\u00FAvida factual sem gastar tempo de pesquisa\",\"PrimarySkill\":\"Hist\\u00F3ria (ou Teoria Arcana)\",\"SecondarySkill\":\"Linguagens\",\"StartingEquipment\":\"Livro pessoal\",\"NarrativeHook\":\"Passou a juventude entre pergaminhos\"}", "Estudioso", 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-000000000006"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"MainBenefit\":\"Pre\\u00E7os com o comerciante viajante 10% melhores\",\"PrimarySkill\":\"Com\\u00E9rcio\",\"SecondarySkill\":\"Avalia\\u00E7\\u00E3o\",\"StartingEquipment\":\"Bolsa de moedas extra\",\"NarrativeHook\":\"Cresceu entre balc\\u00F5es e negocia\\u00E7\\u00F5es\"}", "Comerciante", 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-000000000007"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"MainBenefit\":\"Possui 1 contato de influ\\u00EAncia acion\\u00E1vel (uso limitado)\",\"PrimarySkill\":\"Lideran\\u00E7a\",\"SecondarySkill\":\"Diplomacia\",\"StartingEquipment\":\"Anel de fam\\u00EDlia (sem valor comercial)\",\"NarrativeHook\":\"Perdeu t\\u00EDtulo ou heran\\u00E7a\"}", "Nobre Decaído", 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-000000000008"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"MainBenefit\":\"-1 dificuldade em Furtividade em ambiente urbano\",\"PrimarySkill\":\"Furtividade\",\"SecondarySkill\":\"Manipula\\u00E7\\u00E3o\",\"StartingEquipment\":\"Ferramentas de arrombamento\",\"NarrativeHook\":\"Tem um passado que a Guilda desconhece\"}", "Criminoso", 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-000000000009"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"MainBenefit\":\"1x por expedi\\u00E7\\u00E3o, realiza uma pequena b\\u00EAn\\u00E7\\u00E3o ritual (efeito menor)\",\"PrimarySkill\":\"Religi\\u00E3o\",\"SecondarySkill\":\"Rituais\",\"StartingEquipment\":\"S\\u00EDmbolo sagrado\",\"NarrativeHook\":\"Serviu um templo antes de ingressar na Guilda\"}", "Sacerdote", 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-000000000010"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"MainBenefit\":\"-1 dificuldade em Equil\\u00EDbrio/terreno inst\\u00E1vel\",\"PrimarySkill\":\"Nata\\u00E7\\u00E3o\",\"SecondarySkill\":\"Armas de Arremesso\",\"StartingEquipment\":\"Corda, faca\",\"NarrativeHook\":\"Passou anos em embarca\\u00E7\\u00F5es\"}", "Marinheiro", 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-000000000011"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"MainBenefit\":\"Nunca fica \\u0022perdido\\u0022 narrativamente (sempre sabe a dire\\u00E7\\u00E3o geral)\",\"PrimarySkill\":\"Navega\\u00E7\\u00E3o\",\"SecondarySkill\":\"Sobreviv\\u00EAncia\",\"StartingEquipment\":\"Cantil resistente\",\"NarrativeHook\":\"Nunca teve um lar fixo\"}", "Nômade", 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-000000000012"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"MainBenefit\":\"-1 dificuldade em identificar instabilidades em cavernas e t\\u00FAneis\",\"PrimarySkill\":\"Constru\\u00E7\\u00E3o\",\"SecondarySkill\":\"Percep\\u00E7\\u00E3o\",\"StartingEquipment\":\"Picareta\",\"NarrativeHook\":\"Trabalhou em minas antes de se tornar aventureiro\"}", "Mineiro", 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-000000000013"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"MainBenefit\":\"1x por expedi\\u00E7\\u00E3o, estabiliza um ferido grave sem instala\\u00E7\\u00E3o\",\"PrimarySkill\":\"Medicina\",\"SecondarySkill\":\"Po\\u00E7\\u00F5es\",\"StartingEquipment\":\"Kit m\\u00E9dico b\\u00E1sico\",\"NarrativeHook\":\"Cuidou de doentes numa vila ou tropa\"}", "Curandeiro", 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-000000000014"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"MainBenefit\":\"-1 dificuldade em testes sociais para obter informa\\u00E7\\u00E3o de estranhos\",\"PrimarySkill\":\"Diplomacia\",\"SecondarySkill\":\"Manipula\\u00E7\\u00E3o\",\"StartingEquipment\":\"Instrumento simples\",\"NarrativeHook\":\"Viajou de vila em vila contando hist\\u00F3rias\"}", "Menestrel", 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-000000000015"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"MainBenefit\":\"-1 dificuldade em Percep\\u00E7\\u00E3o para notar armadilhas/emboscadas em ambientes fechados\",\"PrimarySkill\":\"Percep\\u00E7\\u00E3o\",\"SecondarySkill\":\"Furtividade\",\"StartingEquipment\":\"Faca pequena escondida\",\"NarrativeHook\":\"Sobreviveu sozinho nas ruas\"}", "Órfão de Rua", 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-000000000016"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"MainBenefit\":\"Conhece 1 idioma/s\\u00EDmbolo raro exclusivo do grupo\",\"PrimarySkill\":\"Linguagens\",\"SecondarySkill\":\"Rastreamento\",\"StartingEquipment\":\"Nenhum (perdeu tudo)\",\"NarrativeHook\":\"Foi expulso de sua terra natal por um motivo que s\\u00F3 ele sabe\"}", "Exilado", 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-000000000017"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"MainBenefit\":\"Reconhece automaticamente s\\u00EDmbolos/rituais de cultos, sem teste\",\"PrimarySkill\":\"Rituais\",\"SecondarySkill\":\"Religi\\u00E3o\",\"StartingEquipment\":\"Adaga cerimonial\",\"NarrativeHook\":\"Abandonou um culto antes que fosse tarde demais\"}", "Ex-Cultista", 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-000000000018"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"MainBenefit\":\"Recebe 5 pontos extras de per\\u00EDcia para investir em Dungeonologia\",\"PrimarySkill\":\"Dungeonologia\",\"SecondarySkill\":\"Estrat\\u00E9gia\",\"StartingEquipment\":\"Mapa desatualizado da Guilda\",\"NarrativeHook\":\"Cresceu dentro da pr\\u00F3pria Guilda, filho de um veterano\"}", "Pupilo da Guilda", 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-000000000019"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"MainBenefit\":\"-1 dificuldade em Rastreamento de um alvo espec\\u00EDfico definido\",\"PrimarySkill\":\"Rastreamento\",\"SecondarySkill\":\"Intimida\\u00E7\\u00E3o\",\"StartingEquipment\":\"Grilh\\u00F5es, arco leve\",\"NarrativeHook\":\"Vivia de capturar fugitivos e criaturas fugidas\"}", "Caçador de Recompensas", 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("10000000-0000-0000-0000-000000000020"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"MainBenefit\":\"-1 dificuldade no primeiro teste de qualquer nova magia aprendida\",\"PrimarySkill\":\"Controle M\\u00E1gico\",\"SecondarySkill\":\"Teoria Arcana\",\"StartingEquipment\":\"Grim\\u00F3rio incompleto\",\"NarrativeHook\":\"Estudou magia formalmente, mas nunca se formou\"}", "Estudante Arcano", 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("20000000-0000-0000-0000-000000000001"), null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "{\"TriggeringEvent\":\"Explorou uma constru\\u00E7\\u00E3o antiga e escapou\",\"Benefit\":\"-1 dificuldade para identificar riscos estruturais/desabamentos\",\"Complication\":\"Algo daquela ru\\u00EDna ainda o procura\"}", "Sobrevivente de Ruína", 1, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("20000000-0000-0000-0000-000000000002"), null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "{\"TriggeringEvent\":\"Seu grupo anterior foi dizimado\",\"Benefit\":\"1x por expedi\\u00E7\\u00E3o, ignora a condi\\u00E7\\u00E3o de Surpreendido\",\"Complication\":\"Sofre rea\\u00E7\\u00F5es intensas a situa\\u00E7\\u00F5es que lembrem a emboscada\"}", "Sobreviveu a uma Emboscada", 1, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("20000000-0000-0000-0000-000000000003"), null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "{\"TriggeringEvent\":\"Passou tempo confinado, injustamente ou n\\u00E3o\",\"Benefit\":\"Vantagem para escapar de conten\\u00E7\\u00F5es f\\u00EDsicas (cordas, algemas)\",\"Complication\":\"Possui um registro criminal reconhec\\u00EDvel por autoridades\"}", "Foi Preso", 1, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("20000000-0000-0000-0000-000000000004"), null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "{\"TriggeringEvent\":\"Sua unidade foi dizimada em combate\",\"Benefit\":\"Resist\\u00EAncia maior ao medo em combate organizado\",\"Complication\":\"Um superior sobrevivente o culpa pela derrota\"}", "Serviu no Exército", 1, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("20000000-0000-0000-0000-000000000005"), null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "{\"TriggeringEvent\":\"Teve um mentor renomado que sumiu\",\"Benefit\":\"Pode invocar o nome do mestre para abrir portas em um c\\u00EDrculo espec\\u00EDfico\",\"Complication\":\"O desaparecimento do mestre esconde algo perigoso\"}", "Estudou com um Mestre", 1, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("20000000-0000-0000-0000-000000000006"), null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "{\"TriggeringEvent\":\"Per\\u00EDodo de mis\\u00E9ria extrema\",\"Benefit\":\"Aguenta mais tempo sem comida antes de sofrer penalidades\",\"Complication\":\"Deve favores a uma rede do submundo\"}", "Viveu nas Ruas", 1, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("20000000-0000-0000-0000-000000000007"), null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "{\"TriggeringEvent\":\"Recebeu um objeto de fam\\u00EDlia com hist\\u00F3ria\",\"Benefit\":\"O item herdado carrega uma pequena propriedade extra\",\"Complication\":\"Algu\\u00E9m mais tamb\\u00E9m quer aquele objeto de volta\"}", "Herdou uma Ferramenta", 1, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("20000000-0000-0000-0000-000000000008"), null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "{\"TriggeringEvent\":\"Achou um documento que n\\u00E3o deveria ter achado\",\"Benefit\":\"Conhece um fragmento raro de informa\\u00E7\\u00E3o (nome, s\\u00EDmbolo, local)\",\"Complication\":\"Outros sabem que ele tem o manuscrito e o procuram\"}", "Descobriu um Manuscrito", 1, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("20000000-0000-0000-0000-000000000009"), null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "{\"TriggeringEvent\":\"Foi tra\\u00EDdo por algu\\u00E9m de confian\\u00E7a\",\"Benefit\":\"-1 dificuldade para perceber trai\\u00E7\\u00E3o/mentira de aliados pr\\u00F3ximos\",\"Complication\":\"Penalidade em testes sociais para formar v\\u00EDnculos r\\u00E1pidos\"}", "Traído por um Aliado", 1, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("20000000-0000-0000-0000-000000000010"), null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "{\"TriggeringEvent\":\"Feito her\\u00F3ico publicamente reconhecido\",\"Benefit\":\"Reputa\\u00E7\\u00E3o positiva e acesso a favores menores na regi\\u00E3o\",\"Complication\":\"A vila cobra ajuda cont\\u00EDnua; recusar custa reputa\\u00E7\\u00E3o\"}", "Salvou uma Vila", 1, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("20000000-0000-0000-0000-000000000011"), null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "{\"TriggeringEvent\":\"Um familiar desapareceu ou morreu em uma expedi\\u00E7\\u00E3o\",\"Benefit\":\"-1 dificuldade em testes ligados a rastrear aquele tipo de perigo espec\\u00EDfico\",\"Complication\":\"Obsess\\u00E3o que pode lev\\u00E1-lo a riscos desnecess\\u00E1rios\"}", "Perdeu Alguém na Dungeon", 1, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("20000000-0000-0000-0000-000000000012"), null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "{\"TriggeringEvent\":\"Selou um pequeno acordo com uma entidade\",\"Benefit\":\"Pequeno benef\\u00EDcio sobrenatural (definido com o Mestre)\",\"Complication\":\"A entidade cobrar\\u00E1 algo em troca, em algum momento\"}", "Fez um Pacto Menor", 1, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("20000000-0000-0000-0000-000000000013"), null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "{\"TriggeringEvent\":\"Quase morreu de uma praga\",\"Benefit\":\"Resist\\u00EAncia aumentada contra doen\\u00E7as e venenos\",\"Complication\":\"Carrega uma sequela f\\u00EDsica leve e permanente\"}", "Sobreviveu a uma Doença Grave", 1, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("20000000-0000-0000-0000-000000000014"), null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "{\"TriggeringEvent\":\"Teve a reputa\\u00E7\\u00E3o manchada por um crime que n\\u00E3o cometeu\",\"Benefit\":\"B\\u00F4nus em Diplomacia quando precisa se defender de acusa\\u00E7\\u00F5es\",\"Complication\":\"Ainda \\u00E9 malvisto ou procurado em determinado lugar\"}", "Acusado Injustamente", 1, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("20000000-0000-0000-0000-000000000015"), null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "{\"TriggeringEvent\":\"Sabe de algo perigoso que n\\u00E3o deveria saber\",\"Benefit\":\"Possui informa\\u00E7\\u00E3o valiosa, negoci\\u00E1vel\",\"Complication\":\"Outros sabem que ele sabe \\u2014 e isso o torna um alvo\"}", "Guardião de um Segredo", 1, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("20000000-0000-0000-0000-000000000016"), null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "{\"TriggeringEvent\":\"Passou por um ritual incompleto\",\"Benefit\":\"Sensibilidade leve a presen\\u00E7as m\\u00E1gicas pr\\u00F3ximas\",\"Complication\":\"A marca do ritual \\u00E9 percept\\u00EDvel ou reage mal a certos est\\u00EDmulos\"}", "Marcado por um Ritual", 1, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("20000000-0000-0000-0000-000000000017"), null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "{\"TriggeringEvent\":\"Deve a vida a algu\\u00E9m que nunca identificou\",\"Benefit\":\"Possui um contato misterioso que pode ajudar 1x\",\"Complication\":\"N\\u00E3o sabe quem foi \\u2014 a d\\u00EDvida pode ser cobrada a qualquer momento\"}", "Resgatado por Estranhos", 1, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("20000000-0000-0000-0000-000000000018"), null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "{\"TriggeringEvent\":\"Um inc\\u00EAndio ou colapso destruiu sua vida anterior\",\"Benefit\":\"B\\u00F4nus de Vontade contra desespero e perda\",\"Complication\":\"N\\u00E3o possui posses, contatos ou apoio financeiro antigos\"}", "Perdeu Tudo em um Desastre", 1, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("20000000-0000-0000-0000-000000000019"), null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "{\"TriggeringEvent\":\"Viu de perto o fen\\u00F4meno mais temido do mundo\",\"Benefit\":\"Resist\\u00EAncia a p\\u00E2nico diante de fen\\u00F4menos dimensionais\",\"Complication\":\"Hipervigil\\u00E2ncia: penalidade em ambientes que lembram o evento\"}", "Testemunhou uma Ruptura", 1, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("20000000-0000-0000-0000-000000000020"), null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "{\"TriggeringEvent\":\"Cresceu dentro da pr\\u00F3pria institui\\u00E7\\u00E3o\",\"Benefit\":\"B\\u00F4nus em testes administrativos/burocr\\u00E1ticos internos da Guilda\",\"Complication\":\"Nunca teve vida \\u0022normal\\u0022: penalidade leve em situa\\u00E7\\u00F5es sociais fora da Guilda\"}", "Criado pela Guilda", 1, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000006"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000007"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000008"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000009"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000010"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000011"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000012"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000013"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000014"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000015"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000016"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000017"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000018"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000019"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000020"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000006"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000007"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000008"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000009"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000010"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000011"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000012"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000013"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000014"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000015"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000016"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000017"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000018"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000019"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000020"));
        }
    }
}
