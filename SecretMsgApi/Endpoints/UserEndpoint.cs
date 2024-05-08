using SecretMsgApi.Filters;
using SecretMsgApi.Models;
using SecretMsgApi.Services;
using System.Text.Json;

namespace SecretMsgApi.Endpoints
{
    public static class UserEndpoint
    {
        public static RouteGroupBuilder User(this RouteGroupBuilder builder)
        {
            builder.MapGet("/{username:alpha}", async (HttpContext context, string username) =>
            {
                User? user = UserService.GetUserByUsername(username);
                if (user == null || user.Available == false)
                {
                    context.Response.StatusCode = 404;
                    await context.Response.WriteAsync("User not found or not available.");
                    return;
                }

                if (user.ShowLastSeen == false)
                    user.LastSeen = null;

                var data = new { 
                    username = user.Username,
                    name = user.Name,
                    bio = user.Bio,
                    image = user.Image,
                    lastSeen = user.LastSeen,
                };

                await context.Response.WriteAsJsonAsync(data);
            });

            builder.MapGet("me", async (HttpContext context) =>
            {
                int id = int.Parse(context.User.Claims.First().Value);
                User? user = UserService.GetUser(id);
                if(user == null)
                {
                    context.Response.StatusCode = 404;
                    await context.Response.WriteAsync("User not found.");
                    return;
                }

                user.Password = null;
                await context.Response.WriteAsJsonAsync(user);
            }).RequireAuthorization();

            builder.MapPut("/me", async (HttpContext context, UpdateUserModel user) =>
            {
                int id = int.Parse(context.User.Claims.First().Value);
                (string? Error, string? Message) = UserService.UpdateUser(id, user);
                if(Error != null)
                {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync(Error);
                    return;
                }

                await context.Response.WriteAsync(Message!);
            }).RequireAuthorization().AddEndpointFilter<ValidationFilter<UpdateUserModel>>();

            builder.MapPut("/me/change-email", async(HttpContext context, ChangeEmailModel model) =>
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
            }).RequireAuthorization().AddEndpointFilter<ValidationFilter<ChangeEmailModel>>();

            builder.MapPut("/me/change-password", async (HttpContext context, ChangePasswordModel model) => 
            {
                int id = int.Parse(context.User.Claims.First().Value);
                string? error = UserService.UpdateUserPassword(id, model.CurrentPassword, model.NewPassword);

                if(error != null) {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync(error);
                    return;
                }

                await context.Response.WriteAsync("The password was changed.");
            }).RequireAuthorization().AddEndpointFilter<ValidationFilter<ChangePasswordModel>>();

            builder.MapPut("/me/change-username", async (HttpContext context, UsernameModel model) =>
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
            }).RequireAuthorization().AddEndpointFilter<ValidationFilter<UsernameModel>>();

            return builder;
        }
    }
}
