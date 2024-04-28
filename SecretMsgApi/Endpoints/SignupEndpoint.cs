using SecretMsgApi.Filters;
using SecretMsgApi.Models;
using SecretMsgApi.Services;

namespace SecretMsgApi.Endpoints
{
    public static class SignupEndpoint
    {
        public static RouteGroupBuilder Signup(this RouteGroupBuilder builder)
        {
            builder.MapPost("/", async (HttpContext context, SignupModel model) =>
            {
                (string? Error, string? Token) = UserService.Register(model.Name, model.Email, model.Password);
                if (Token != null)
                {
                    await context.Response.WriteAsync(Token);
                    return; 
                }

                context.Response.StatusCode = 400;
                await context.Response.WriteAsync(Error!);
            }).AddEndpointFilter<ValidationFilter<SignupModel>>();

            return builder;
        }
    }
}
