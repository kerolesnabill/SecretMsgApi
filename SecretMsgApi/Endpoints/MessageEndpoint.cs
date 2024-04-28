using SecretMsgApi.Filters;
using SecretMsgApi.Models;
using SecretMsgApi.Services;

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

            return builder;
        }
    }
}
