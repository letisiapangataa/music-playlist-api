namespace Playlist.Domain;

public record Track(string Id, string Title, string Artist);
public class PlaylistAggregate
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Name { get; }
    private readonly List<Track> _tracks = new();
    public IReadOnlyList<Track> Tracks => _tracks;

    public PlaylistAggregate(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name required");
        Name = name.Trim();
    }

    public DomainEvent AddTrack(Track t)
    {
        if (_tracks.Exists(x => x.Id == t.Id))
            throw new InvalidOperationException("Duplicate track");
        _tracks.Add(t);
        return new TrackAdded(Id, t.Id);
    }

    public DomainEvent RemoveTrack(string trackId)
    {
        var removed = _tracks.RemoveAll(x => x.Id == trackId);
        if (removed == 0) throw new KeyNotFoundException("Track not found");
        return new TrackRemoved(Id, trackId);
    }
}

public abstract record DomainEvent(Guid PlaylistId, DateTimeOffset OccurredAt);
public record TrackAdded(Guid PlaylistId, string TrackId) : DomainEvent(PlaylistId, DateTimeOffset.UtcNow);
public record TrackRemoved(Guid PlaylistId, string TrackId) : DomainEvent(PlaylistId, DateTimeOffset.UtcNow);

public interface IEventBus { Task PublishAsync(DomainEvent evt, CancellationToken ct = default); }

public sealed class InMemoryEventBus : IEventBus
{
    public List<DomainEvent> Published { get; } = new();
    public Task PublishAsync(DomainEvent evt, CancellationToken ct = default)
    {
        Published.Add(evt);
        return Task.CompletedTask;
    }
}
