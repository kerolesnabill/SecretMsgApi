using SecretMsgApi.Filters;
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
            }).AddEndpointFilter<ValidationFilter<User>>();

            builder.MapPut("/change-email", async(HttpContext context, ChangeEmailModel model) =>
            {
                int id = int.Parse(context.User.Claims.First().Value);
                string? error = UserService.UpdateUserEmail(id, model.NewEmail, model.Password);
                if (error != null)
                {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync(error);
                    return;
                }

                await context.Response.WriteAsync("The email was changed.");
            }).AddEndpointFilter<ValidationFilter<ChangeEmailModel>>();

            builder.MapPut("/change-password", async (HttpContext context, ChangePasswordModel model) => 
            {
                int id = int.Parse(context.User.Claims.First().Value);
                string? error = UserService.UpdateUserPassword(id, model.CurrentPassword, model.NewPassword);

                if(error != null) {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync(error);
                    return;
                }

                await context.Response.WriteAsync("The password was changed.");
            }).AddEndpointFilter<ValidationFilter<ChangePasswordModel>>();

            builder.MapPut("/change-username", async (HttpContext context, UsernameModel model) =>
            {
                int id = int.Parse(context.User.Claims.First().Value);
                string? error = UserService.UpdateUsername(id, model.Username);
                if(error != null)
                {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync(error);
                    return;
                }

                await context.Response.WriteAsync("Username was changed.");
            }).AddEndpointFilter<ValidationFilter<UsernameModel>>();

            return builder;
        }
    }
}
