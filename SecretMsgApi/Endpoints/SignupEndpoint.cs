using SecretMsgApi.Models;
using SecretMsgApi.Services;

namespace SecretMsgApi.Endpoints
{
    public static class SignupEndpoint
    {
        public static RouteGroupBuilder Signup(this RouteGroupBuilder builder)
        {
            builder.MapPost("/", async (HttpContext context, User user) =>
            {
                (string? Error, string? Token) = UserService.Register(user);
                if (Token != null)
                {
                    await context.Response.WriteAsync(Token);
                    return; 
                }

                context.Response.StatusCode = 400;
                await context.Response.WriteAsync(Error!);
            });

            return builder;
        }
    }
}
