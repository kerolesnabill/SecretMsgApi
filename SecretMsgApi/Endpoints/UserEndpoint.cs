using SecretMsgApi.Models;
using SecretMsgApi.Services;

namespace SecretMsgApi.Endpoints
{
    public static class UserEndpoint
    {
        public static RouteGroupBuilder User(this RouteGroupBuilder builder)
        {
            builder.MapPut("/", async (HttpContext context, User user) =>
            {
                user.Id = int.Parse(context.User.Claims.First().Value);
                (string? Error, string? Message) = UserService.UpdateUser(user);
                if(Error != null)
                {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync(Error);
                    return;
                }

                await context.Response.WriteAsync(Message!);
            });

            return builder;
        }
    }
}
