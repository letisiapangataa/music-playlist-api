using Playlist.Domain;
using FluentAssertions;
using Xunit;

public class PlaylistTests
{
    [Fact]
    public void AddTrack_PublishesEvent()
    {
        var pl = new PlaylistAggregate("Chill");
        var bus = new InMemoryEventBus();
        var evt = pl.AddTrack(new Track("t1", "Lofi", "Anon"));
        bus.PublishAsync(evt).Wait();
        pl.Tracks.Should().HaveCount(1);
    }

    [Fact]
    public void RemoveTrack_Throws_WhenMissing()
    {
        var pl = new PlaylistAggregate("Chill");
        Action act = () => pl.RemoveTrack("missing");
        act.Should().Throw<KeyNotFoundException>();
    }
}
