using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Bookstore.Web.Helpers
{
    public class LocalAuthenticationMiddleware
    {
        private const string UserId = "FB6135C7-1464-4A72-B74E-4B63D343DD09";
        private readonly RequestDelegate _next;

        public LocalAuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.Value.StartsWith("/Authentication/Login"))
            {
                var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, UserId));
                identity.AddClaim(new Claim("sub", UserId));
                identity.AddClaim(new Claim("cognito:username", "localuser"));
                identity.AddClaim(new Claim(ClaimTypes.Email, "localuser@example.com"));
                identity.AddClaim(new Claim(ClaimTypes.GivenName, "Local"));
                identity.AddClaim(new Claim(ClaimTypes.Surname, "User"));

                var principal = new ClaimsPrincipal(identity);

                await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                context.Response.Redirect("/");
                return;
            }

            await _next(context);
        }
    }
}
