using SecretMsgApi.Filters;
using SecretMsgApi.Models;
using SecretMsgApi.Services;

namespace SecretMsgApi.Endpoints
{
    public static class LoginEnpoint
    {
        public static RouteGroupBuilder Login(this RouteGroupBuilder builder)
        {
            builder.MapPost("/", async (HttpContext context, LoginModel model) =>
            {
                string? token = UserService.Login(model.Email, model.Password);
                if(token is null) 
                { 
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync("Incorrect email or password.");
                    return; 
                }

                await context.Response.WriteAsync(token);
            }).AddEndpointFilter<ValidationFilter<LoginModel>>();

            return builder;
        }
    }
}
