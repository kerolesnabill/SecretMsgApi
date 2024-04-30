using Microsoft.IdentityModel.Tokens;
using SecretMsgApi.Endpoints;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;
builder.Services.AddCors();
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

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors(c => c.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
app.UseAuthentication();
app.UseAuthorization();

app.MapGroup("api/signup").Signup();
app.MapGroup("api/login").Login();
app.MapGroup("api/user").User().RequireAuthorization();
app.MapGroup("api/message").Message();

app.Run();
