using SecretMsgApi.Models;
using SecretMsgApi.Services;

namespace SecretMsgApi.Endpoints
{
    public static class LoginEnpoint
    {
        public static RouteGroupBuilder Login(this RouteGroupBuilder builder)
        {
            builder.MapPost("/", async (HttpContext context, User user) =>
            {
                context.Response.StatusCode = 400;
                if(String.IsNullOrEmpty(user.Email)) {
                    await context.Response.WriteAsync("Email is required.");
                    return; }

                if (String.IsNullOrEmpty(user.Password)) {
                    await context.Response.WriteAsync("Password is required.");
                    return; }

                string? token = UserService.Login(user.Email, user.Password);
                if(token is null) { 
                    await context.Response.WriteAsync("Incorrect email or password.");
                return; }

                context.Response.StatusCode = 200;
                await context.Response.WriteAsync(token);
            });

            return builder;
        }
    }
}
