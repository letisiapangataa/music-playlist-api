using System;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Playlist.Domain;

public class KafkaEventBus : IEventBus
{
    private readonly string _topic;
    private readonly IProducer<Null, string> _producer;

    public KafkaEventBus(string bootstrapServers, string topic = "playlist-events")
    {
        _topic = topic;
        var config = new ProducerConfig { BootstrapServers = bootstrapServers };
        _producer = new ProducerBuilder<Null, string>(config).Build();
    }

    public async Task PublishAsync(DomainEvent evt, CancellationToken ct = default)
    {
        var payload = System.Text.Json.JsonSerializer.Serialize(evt, evt.GetType());
        await _producer.ProduceAsync(_topic, new Message<Null, string> { Value = payload }, ct);
    }
}
