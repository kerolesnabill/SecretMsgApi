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
                context.Response.StatusCode = 400;
                if (user.Email is null) {
                    await context.Response.WriteAsync("The email is required.");
                    return;
                }

                if (user.Password is null) {
                    await context.Response.WriteAsync("The password is required.");
                    return;
                }

                if (user.Name is null) {
                    await context.Response.WriteAsync("The name is required.");
                    return;
                }

                (string? Error, string? Token) = UserService.Register(user);
                if (Token != null)
                {
                    await context.Response.WriteAsync(Token);
                    return; 
                }

                await context.Response.WriteAsync(Error!);
            });

            return builder;
        }
    }
}
