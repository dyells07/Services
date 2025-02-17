using JWT_Refresh.Data;
using JWT_Refresh.Hubs;
using JWT_Refresh.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<UserContext>(opts =>
    opts.UseSqlServer(builder.Configuration.GetConnectionString("UserDB")));

builder.Services.AddTransient<ITokenService, TokenService>();
builder.Services.AddSignalR();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHub<ChatHub>("/chatHub");
});

app.Run();
