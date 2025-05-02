using EXO.WebServer;
using EXO.WebServer.Server;
using EXO.WebServer.Server.Routers;
using System.Net.WebSockets;
using Serilog;
using Serilog.Events;
using EXO.WebServer.Server.Rooms;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Transients...
builder.Services.AddTransient<IMessageRouter, RoomRouter>();
builder.Services.AddTransient<ClientFactory>();

// Singletons...
builder.Services.AddSingleton<ClientManager>();
builder.Services.AddSingleton<RoomManager>();
builder.Services.AddSingleton<IServer, RoomServer>();

// Logging...
builder.Services.AddLogging();


builder.WebHost.UseKestrel();

// 1) Create and configure the Logger before the host is built
Log.Logger = new LoggerConfiguration()
    // set your minimum levels
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    // add enrichers for extra context
    .Enrich.FromLogContext()
    // configure the Console sink
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
    )
    .CreateLogger();


builder.Host.UseSerilog();

// Grab the config...
var config = builder.Configuration;

var app = builder.Build();

var wsOpts = new WebSocketOptions()
{
    KeepAliveInterval = TimeSpan.FromMinutes(2),
};

wsOpts.AllowedOrigins.Add("*");

app.UseHttpsRedirection();
app.UseRouting();
app.UseWebSockets(wsOpts);
app.UseAuthorization();
app.MapControllers();

app.Map("/echo", async context =>
{
    if (!context.WebSockets.IsWebSocketRequest)
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("Requires WebSocket");
        return;
    }

    using var ws = await context.WebSockets.AcceptWebSocketAsync();
    var buffer = new byte[4 * 1024];
    var seg = new ArraySegment<byte>(buffer);

    while (ws.State == WebSocketState.Open)
    {
        var result = await ws.ReceiveAsync(seg, CancellationToken.None);
        if (result.MessageType == WebSocketMessageType.Close)
            break;

        await ws.SendAsync(seg[..result.Count],
                           result.MessageType,
                           result.EndOfMessage,
                           CancellationToken.None);
    }

    await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Bye", CancellationToken.None);
});


// Run the application
app.Run();
