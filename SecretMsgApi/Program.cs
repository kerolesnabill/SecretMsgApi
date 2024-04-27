using Microsoft.IdentityModel.Tokens;
using SecretMsgApi.Endpoints;
using SecretMsgApi.Filters;
using SecretMsgApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;
builder.Services.AddAuthentication()
    .AddJwtBearer(options => 
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = config["JwtSettings:Issuer"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(config["JwtSettings:Key"]!))
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

// app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapGroup("/signup").Signup().AddEndpointFilter<ValidationFilter<User>>();

app.Run();
