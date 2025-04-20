using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Net.payOS;
using System.Text;
using WorkSmart.API.Extension;
using WorkSmart.API.Hubs;
using WorkSmart.Application.Services;
using WorkSmart.Core.Dto.MailDtos;
using WorkSmart.Core.Dto.PaymentDtos;
using WorkSmart.Core.Interface;
using WorkSmart.Repository.Repository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173","http://localhost:7141", "https://worksmart-fe.vercel.app") // Thay bằng origin thực tế của client, không sử dụng all vì không đi chung được  với AllowCredentials (bắt buộc) 
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials(); // Quan trọng cho SignalR
        });
});

builder.Services.AddControllers();

builder.Services.AddScopeCollection(builder.Configuration);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
builder.Services.AddDistributedMemoryCache();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})

.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidAudiences = new List<string> { "admin","employer","candidate" },
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

builder.Services.Configure<PayOSSettings>(builder.Configuration.GetSection("PayOS"));

builder.Services.AddSingleton<PayOS>(provider =>
{
    var config = provider.GetRequiredService<IOptions<PayOSSettings>>().Value;

    if (string.IsNullOrEmpty(config.ClientId) ||
        string.IsNullOrEmpty(config.ApiKey) ||
        string.IsNullOrEmpty(config.ChecksumKey))
    {
        throw new InvalidOperationException("PayOS configuration is missing or invalid.");
    }

    return new PayOS(config.ClientId,config.ApiKey,config.ChecksumKey);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseCors("AllowAll"); // Áp dụng chính sách AllowAll
app.UseCors("AllowAll"); // Áp dụng chính sách AllowAll


app.UseHttpsRedirection();

app.UseAuthorization();
app.UseAuthentication();

app.MapControllers();

app.MapHub<NotificationHub>("/notificationHub");
app.MapHub<ChatHub>("/chatHub");

app.Run();
