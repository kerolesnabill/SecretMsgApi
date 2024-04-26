using SecretMsgApi.Endpoints;
using SecretMsgApi.Filters;
using SecretMsgApi.Models;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGroup("/signup").Signup().AddEndpointFilter<ValidationFilter<User>>();

app.Run();
