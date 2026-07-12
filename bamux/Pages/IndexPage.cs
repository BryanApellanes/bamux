using Bam.Presentation;

namespace Bam.Ux.Pages;

public class IndexPage : HtmlPage
{
    public IndexPage() : base("/", "bamux",
        """
        <h1>bamux</h1>
        <ul>
            <li><a href="/register">Register</a></li>
        </ul>
        """)
    {
    }
}
