using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace MyNotes.Security;

public static class PrivateAccessEndpoints
{
    public static void MapPrivateAccess(this WebApplication app)
    {
        app.MapGet("/private", (HttpContext context, IAntiforgery antiforgery, PrivateAccessStore access) =>
        {
            context.Response.Headers.CacheControl = "no-store";
            var returnUrl = SafeReturnUrl(context.Request.Query["returnUrl"]);
            if (access.IsUnlocked(context.User)) return Results.Redirect(returnUrl);
            var tokens = antiforgery.GetAndStoreTokens(context);
            var status = !access.IsConfigured ? "Private access has not been configured on this computer."
                : context.Request.Query["error"] == "1" ? "Password was not accepted. Try again." : "Enter your password to continue.";
            var encoder = HtmlEncoder.Default;
            var html = $$"""
                <!doctype html><html lang="en"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>Private area</title>
                <style>body{margin:0;background:#101416;color:#eee;font:16px 'Segoe UI',sans-serif;display:grid;place-items:center;min-height:100vh}.card{width:min(360px,80vw);padding:32px;border:1px solid #354038;border-radius:12px;background:#1b211e}h1{font-size:24px}p{color:#b0b9af;font-size:14px;line-height:1.6}label{display:block;margin:20px 0 8px}input{box-sizing:border-box;width:100%;padding:12px;border:1px solid #52604d;border-radius:4px;background:#101416;color:white;font:inherit}button{width:100%;margin:18px 0;padding:12px;background:#c5df8b;color:#182018;border:0;border-radius:4px;font-weight:700}a{color:#c5df8b;font-size:13px}</style></head>
                <body><main class="card"><h1>Private area</h1><p role="status">{{status}}</p>
                <form method="post" action="/private/unlock"><input type="hidden" name="{{encoder.Encode(tokens.FormFieldName)}}" value="{{encoder.Encode(tokens.RequestToken ?? "")}}">
                <input type="hidden" name="returnUrl" value="{{encoder.Encode(returnUrl)}}"><label for="password">Password</label><input id="password" name="password" type="password" autocomplete="current-password" required maxlength="256" autofocus><button type="submit">Unlock</button></form>
                <a href="/">Back to dashboard</a></main></body></html>
                """;
            return Results.Content(html, "text/html; charset=utf-8");
        });

        app.MapPost("/private/unlock", async (HttpContext context, IAntiforgery antiforgery, PrivateAccessStore access) =>
        {
            context.Response.Headers.CacheControl = "no-store";
            try { await antiforgery.ValidateRequestAsync(context); }
            catch (AntiforgeryValidationException) { return Results.BadRequest("Refresh the unlock page and try again."); }
            var form = await context.Request.ReadFormAsync();
            var returnUrl = SafeReturnUrl(form["returnUrl"]);
            if (!access.VerifyPassword(form["password"].ToString()))
                return Results.Redirect("/private?error=1&returnUrl=" + Uri.EscapeDataString(returnUrl));
            var identity = new ClaimsIdentity([
                new Claim(ClaimTypes.Name, "Owner"),
                new Claim(PrivateAccessStore.SessionClaim, access.CreateSession())
            ], CookieAuthenticationDefaults.AuthenticationScheme);
            await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity),
                new AuthenticationProperties { IsPersistent = false, ExpiresUtc = DateTimeOffset.UtcNow.Add(PrivateAccessStore.SessionLifetime) });
            return Results.Redirect(returnUrl);
        }).RequireRateLimiting("private-login");

        app.MapPost("/private/lock", async (HttpContext context, IAntiforgery antiforgery, PrivateAccessStore access) =>
        {
            try { await antiforgery.ValidateRequestAsync(context); }
            catch (AntiforgeryValidationException) { return Results.BadRequest("Refresh the page and try again."); }
            access.Revoke(context.User);
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Results.Redirect("/");
        });
    }

    private static string SafeReturnUrl(string? value)
        => value == "/moviereview" || value?.StartsWith("/moviereview/", StringComparison.Ordinal) == true
            ? value : "/moviereview";
}
