using SecretMsgApi.Filters;
using SecretMsgApi.Models;
using SecretMsgApi.Services;
using System.Text.Json;

namespace SecretMsgApi.Endpoints
{
    public static class MessageEndpoint
    {
        public static RouteGroupBuilder Message(this RouteGroupBuilder builder)
        {
            builder.MapPost("/", async (HttpContext context, AddMessageModel message) =>
            {
                int id = message.UserId?? 0;
                string? error = MessageService.AddMessage(id, message.Body);
                if (error != null)
                {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync(error);
                    return;
                }

                await context.Response.WriteAsync("Your message was successfully sent.");
            }).AddEndpointFilter<ValidationFilter<AddMessageModel>>();

            builder.MapGet("/", async (HttpContext context) =>
            {
                int id = int.Parse(context.User.Claims.First().Value);
                var (error, messages) = MessageService.GetMessages(id);
                if (error != null)
                {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync(error);
                    return;
                }

                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(messages));
            }).RequireAuthorization();

            builder.MapGet("/{messageId:int}", async (HttpContext context, int messageId) =>
            {
                int userId = int.Parse(context.User.Claims.First().Value);
                var (error, message) = MessageService.GetMessage(userId, messageId);
                if (error != null)
                {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync(error);
                    return;
                }

                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(message));
            }).RequireAuthorization();

            return builder;
        }
    }
}
