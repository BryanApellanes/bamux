using Bam.DependencyInjection;
using Bam.Test;
using Bam.Ux.Pages;

namespace Bam.Ux.Tests.Unit;

[UnitTestMenu("Pages should", Selector = "pgs")]
public class PagesShould : UnitTestMenuContainer
{
    public PagesShould(ServiceRegistry serviceRegistry) : base(serviceRegistry)
    {
    }

    [UnitTest]
    public void MapIndexPageToRoot()
    {
        IndexPage page = new IndexPage();

        When.A<IndexPage>(
            "checks the route path",
            page,
            (p) => p.Path)
        .TheTest
        .ShouldPass(because =>
        {
            because.ItsTrue("path is /", page.Path == "/");
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void MapRegisterPageToRegisterRoute()
    {
        RegisterPage page = new RegisterPage();

        When.A<RegisterPage>(
            "checks the route path",
            page,
            (p) => p.Path)
        .TheTest
        .ShouldPass(because =>
        {
            because.ItsTrue("path is /register", page.Path == "/register");
        })
        .SoBeHappy()
        .UnlessItFailed();
    }

    [UnitTest]
    public void MapRegisterResultPageToRegisterResultRoute()
    {
        RegisterResultPage page = new RegisterResultPage();

        When.A<RegisterResultPage>(
            "checks the route path",
            page,
            (p) => p.Path)
        .TheTest
        .ShouldPass(because =>
        {
            because.ItsTrue("path is /register/result", page.Path == "/register/result");
        })
        .SoBeHappy()
        .UnlessItFailed();
    }
}
