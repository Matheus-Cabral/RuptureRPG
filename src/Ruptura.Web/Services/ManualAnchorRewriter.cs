using System.Text;
using Markdig;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace Ruptura.Web.Services;

// Two independent bugs conspire to break every "jump to topic" link in the manuals' table of
// contents:
//
// 1. The TOC uses plain "#slug" fragment anchors — the standard, portable Markdown anchor style
//    (these .md files also serve as real docs in the repo, so they can't hardcode an app route
//    into every link). A bare "#slug" href resolves against the page's <base href> per the HTML
//    spec — which index.html sets to "/" — not the current path. So clicking one navigates to
//    "/#slug" (the marketing landing page at "/"), not a scroll on the current page.
//
// 2. Even pointed at the right page, the fragment wouldn't find its target: Markdig's built-in
//    AutoIdentifier ("GitHub" mode, via UseAdvancedExtensions()) does NOT reproduce GitHub.com's
//    real heading-slug algorithm — e.g. "### 3.1 Origem" becomes id="origem" (it drops the
//    leading "3.1 " and, elsewhere, strips accents), while the TOC's hand-written anchors assume
//    id="31-origem" (numbering kept, accents kept — verified by reverse-engineering the exact
//    algorithm against 5 known TOC entries, including the em-dash-produces-a-double-hyphen case
//    in "#9-interlúdio--o-tempo-entre-expedições"). So this recomputes every heading's id with
//    that real-GitHub-compatible slugger instead of trusting Markdig's own.
public static class ManualAnchorRewriter
{
    public static string RenderWithFixedAnchors(string markdown, MarkdownPipeline pipeline, string currentPath)
    {
        var document = Markdown.Parse(markdown, pipeline);

        var usedSlugs = new Dictionary<string, int>();
        foreach (var heading in document.Descendants<HeadingBlock>())
        {
            var slug = Slugify(ExtractText(heading.Inline));
            if (usedSlugs.TryGetValue(slug, out var count))
            {
                usedSlugs[slug] = count + 1;
                slug = $"{slug}-{count}";
            }
            else
            {
                usedSlugs[slug] = 1;
            }

            heading.GetAttributes().Id = slug;
        }

        foreach (var link in document.Descendants<LinkInline>())
        {
            if (link.Url is { Length: > 0 } url && url[0] == '#')
                link.Url = currentPath + url;
        }

        using var writer = new StringWriter();
        var renderer = new Markdig.Renderers.HtmlRenderer(writer);
        pipeline.Setup(renderer);
        renderer.Render(document);
        return writer.ToString();
    }

    // GitHub.com's real heading-slug algorithm: lowercase, drop every character that isn't a
    // Unicode letter/digit/space/hyphen (nothing inserted in its place — that's what produces the
    // double-hyphen when an em-dash sits between two spaces), then turn each remaining space into
    // a hyphen.
    private static string Slugify(string text)
    {
        var kept = new StringBuilder(text.Length);
        foreach (var c in text.ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(c) || c == ' ' || c == '-') kept.Append(c);
        }
        return kept.ToString().Replace(' ', '-');
    }

    private static string ExtractText(Inline? inline)
    {
        var sb = new StringBuilder();
        Walk(inline);
        return sb.ToString();

        void Walk(Inline? node)
        {
            for (; node is not null; node = node.NextSibling)
            {
                switch (node)
                {
                    case LiteralInline literal:
                        sb.Append(literal.Content.ToString());
                        break;
                    case ContainerInline container:
                        Walk(container.FirstChild);
                        break;
                }
            }
        }
    }
}
