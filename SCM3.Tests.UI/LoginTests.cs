using Microsoft.Playwright;

namespace SCM3.Tests.UI;

[Collection(WebAppCollection.Name)]
public class LoginTests(WebAppFixture app)
{
    [Fact]
    public async Task Login_WithValidDemoCredentials_ReachesAuthenticatedShell()
    {
        var page = await app.Browser.NewPageAsync();
        try
        {
            await page.GotoAsync($"{app.WebBaseUrl}/login");

            await page.FillAsync("input[name=username]", "scmadmin");
            await page.FillAsync("input[name=password]", "test");
            await page.ClickAsync("button[type=submit]");

            // The /login POST redirects to "/", which renders inside MainLayout —
            // wait for the shell chrome rather than a specific page's content.
            var brand = page.Locator(".scm3-brand");
            await brand.WaitForAsync();

            await Microsoft.Playwright.Assertions.Expect(brand).ToHaveTextAsync("SCM3 Portal");
            await Microsoft.Playwright.Assertions.Expect(page.Locator(".scm3-user")).ToHaveTextAsync("Demo SCM Admin");
            await Microsoft.Playwright.Assertions.Expect(page.Locator(".scm3-logout")).ToBeVisibleAsync();
            Assert.DoesNotContain("/login", page.Url);
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task Login_WithWrongPassword_StaysOnLoginAndShowsError()
    {
        var page = await app.Browser.NewPageAsync();
        try
        {
            await page.GotoAsync($"{app.WebBaseUrl}/login");

            await page.FillAsync("input[name=username]", "scmadmin");
            await page.FillAsync("input[name=password]", "wrong-password");
            await page.ClickAsync("button[type=submit]");

            var error = page.Locator(".scm3-login-error");
            await Microsoft.Playwright.Assertions.Expect(error).ToHaveTextAsync("Invalid username or password.");
            Assert.Contains("/login", page.Url);
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task Login_WithUnknownUsername_StaysOnLoginAndShowsError()
    {
        var page = await app.Browser.NewPageAsync();
        try
        {
            await page.GotoAsync($"{app.WebBaseUrl}/login");

            await page.FillAsync("input[name=username]", "not-a-real-user");
            await page.FillAsync("input[name=password]", "test");
            await page.ClickAsync("button[type=submit]");

            var error = page.Locator(".scm3-login-error");
            await Microsoft.Playwright.Assertions.Expect(error).ToHaveTextAsync("Invalid username or password.");
            Assert.Contains("/login", page.Url);
        }
        finally
        {
            await page.CloseAsync();
        }
    }
}
