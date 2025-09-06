// (C) 2025 Letisia Pangata'a (@letisiapangataa)
using Microsoft.AspNetCore.Http.HttpResults;
using Playlist.Domain;

var builder = WebApplication.CreateBuilder(args);
// Choose event bus: InMemory, RabbitMQ, or Kafka
var useKafka = builder.Configuration.GetValue<bool>("UseKafkaEventBus");
if (useKafka)
{
    builder.Services.AddSingleton<IEventBus>(_ => new KafkaEventBus("localhost:9092"));
}
else
{
    builder.Services.AddSingleton<IEventBus, InMemoryEventBus>();
}
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

// In-memory store
var store = new Dictionary<Guid, PlaylistAggregate>();

app.MapPost("/playlists", (CreatePlaylist req) =>
{
    var pl = new PlaylistAggregate(req.Name);
    store[pl.Id] = pl;
    return Results.Created($"/playlists/{pl.Id}", new { pl.Id, pl.Name, Tracks = pl.Tracks });
});

app.MapGet("/playlists/{id:guid}", (Guid id) =>
{
    return store.TryGetValue(id, out var pl) ? Results.Ok(new { pl.Id, pl.Name, Tracks = pl.Tracks }) : Results.NotFound();
});

app.MapPost("/playlists/{id:guid}/tracks", async (Guid id, AddTrack req, IEventBus bus) =>
{
    if (!store.TryGetValue(id, out var pl)) return Results.NotFound();
    try
    {
        var evt = pl.AddTrack(new Track(req.Id, req.Title, req.Artist));
        await bus.PublishAsync(evt);
        return Results.Accepted($"/playlists/{pl.Id}", new { Message = "Track added" });
    }
    catch (Exception ex) { return Results.BadRequest(new { Error = ex.Message }); }
});

app.MapDelete("/playlists/{id:guid}/tracks/{trackId}", async (Guid id, string trackId, IEventBus bus) =>
{
    if (!store.TryGetValue(id, out var pl)) return Results.NotFound();
    try
    {
        var evt = pl.RemoveTrack(trackId);
        await bus.PublishAsync(evt);
        return Results.Ok(new { Message = "Track removed" });
    }
    catch (Exception ex) { return Results.BadRequest(new { Error = ex.Message }); }
});

app.Run();

record CreatePlaylist(string Name);
record AddTrack(string Id, string Title, string Artist);
