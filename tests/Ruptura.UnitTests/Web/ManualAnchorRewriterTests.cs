using FluentAssertions;
using Markdig;
using Ruptura.Web.Services;

namespace Ruptura.UnitTests.Web;

public class ManualAnchorRewriterTests
{
    private static readonly MarkdownPipeline Pipeline =
        new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();

    [Fact]
    public void RenderWithFixedAnchors_PrefixesFragmentOnlyLinks_WithCurrentPath()
    {
        const string markdown = "[Origem](#31-origem)";

        var html = ManualAnchorRewriter.RenderWithFixedAnchors(markdown, Pipeline, "/manuals");

        html.Should().Contain("href=\"/manuals#31-origem\"");
    }

    [Fact]
    public void RenderWithFixedAnchors_LeavesExternalAndAbsoluteLinks_Untouched()
    {
        const string markdown = "[GitHub](https://github.com/example) and [Home](/dashboard)";

        var html = ManualAnchorRewriter.RenderWithFixedAnchors(markdown, Pipeline, "/manuals");

        html.Should().Contain("href=\"https://github.com/example\"");
        html.Should().Contain("href=\"/dashboard\"");
    }

    // Markdig's own AutoIdentifier ("GitHub" mode) does not reproduce GitHub.com's real slug
    // algorithm — it drops leading numbering and strips accents, which is why these cases exist:
    // they're reverse-engineered from the manuals' actual hand-written TOC anchors.
    [Theory]
    [InlineData("### 3.1 Origem", "31-origem")]
    [InlineData("## 3. Criação de Personagem", "3-criação-de-personagem")]
    [InlineData("### 3.3 Linhagem (Raça/Espécie)", "33-linhagem-raçaespécie")]
    [InlineData("## 9. Interlúdio — O Tempo Entre Expedições", "9-interlúdio--o-tempo-entre-expedições")]
    [InlineData("## 1. O Mundo em Poucas Palavras", "1-o-mundo-em-poucas-palavras")]
    public void RenderWithFixedAnchors_GeneratesGitHubCompatibleHeadingIds(string heading, string expectedId)
    {
        var html = ManualAnchorRewriter.RenderWithFixedAnchors(heading, Pipeline, "/manuals");

        html.Should().Contain($"id=\"{expectedId}\"");
    }

    [Fact]
    public void RenderWithFixedAnchors_DisambiguatesDuplicateHeadingText()
    {
        const string markdown = "## Exemplo\n\ntext\n\n## Exemplo\n\nmore text";

        var html = ManualAnchorRewriter.RenderWithFixedAnchors(markdown, Pipeline, "/manuals");

        html.Should().Contain("id=\"exemplo\"");
        html.Should().Contain("id=\"exemplo-1\"");
    }

    // Real end-to-end regression guard: every "#slug" link that appears anywhere in the actual
    // manual's rendered HTML must resolve to a heading id present in that same HTML — i.e. every
    // TOC entry (and any other in-page link) has a real landing spot. This is what would have
    // caught both bugs fixed here, and protects against the docs and the TOC drifting apart again
    // in a future edit.
    [Theory]
    [InlineData("Manual_do_Jogador.md")]
    [InlineData("Manual_do_Mestre.md")]
    [InlineData("Manual_do_Jogador.en.md")]
    [InlineData("Manual_do_Mestre.en.md")]
    public void RenderWithFixedAnchors_EveryFragmentLinkInTheRealManual_ResolvesToARealHeadingId(string fileName)
    {
        var markdown = File.ReadAllText(Path.Combine(FindRepoRoot(), "docs", "manuais", fileName));

        var html = ManualAnchorRewriter.RenderWithFixedAnchors(markdown, Pipeline, "/manuals");

        var ids = System.Text.RegularExpressions.Regex.Matches(html, "id=\"([^\"]+)\"")
            .Select(m => m.Groups[1].Value)
            .ToHashSet();
        // Markdig percent-encodes non-ASCII link hrefs on render (e.g. "ç" -> "%C3%A7") but
        // leaves `id` attributes as literal UTF-8 text — a real browser URI-decodes a fragment
        // before matching it against an element id, so undo that encoding here to compare
        // like-for-like instead of flagging a false mismatch.
        var linkedFragments = System.Text.RegularExpressions.Regex.Matches(html, "href=\"/manuals#([^\"]+)\"")
            .Select(m => Uri.UnescapeDataString(m.Groups[1].Value))
            .ToList();

        linkedFragments.Should().NotBeEmpty();
        linkedFragments.Should().OnlyContain(fragment => ids.Contains(fragment));
    }

    private static string FindRepoRoot()
    {
        var dir = AppContext.BaseDirectory;
        while (dir is not null && !File.Exists(Path.Combine(dir, "Ruptura.sln")))
            dir = Path.GetDirectoryName(dir.TrimEnd(Path.DirectorySeparatorChar));

        return dir ?? throw new InvalidOperationException("Could not locate repo root (Ruptura.sln) from " + AppContext.BaseDirectory);
    }
}
