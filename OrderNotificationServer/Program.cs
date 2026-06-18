using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

// ── Configuration ────────────────────────────────────────────────────────────
var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var bootstrapServers = config["Kafka:BootstrapServers"]!;
var topicName        = config["Kafka:TopicName"]!;
const string GroupId = "order-notification-server";

// ── Consumer setup ───────────────────────────────────────────────────────────
var consumerConfig = new ConsumerConfig
{
    BootstrapServers  = bootstrapServers,
    GroupId           = GroupId,
    AutoOffsetReset   = AutoOffsetReset.Earliest,  // replay from the start on first run
    EnableAutoCommit  = false                       // manual commit for reliability
};

using var consumer = new ConsumerBuilder<string, string>(consumerConfig)
    .SetErrorHandler((_, error) =>
        Console.Error.WriteLine($"[ERROR] Kafka: {error.Reason}"))
    .SetPartitionsAssignedHandler((_, partitions) =>
        Console.WriteLine($"[INFO]  Partitions assigned: {string.Join(", ", partitions)}"))
    .Build();

consumer.Subscribe(topicName);

Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
Console.WriteLine("║          OrderNotificationServer  –  started             ║");
Console.WriteLine($"║  Topic  : {topicName,-47}║");
Console.WriteLine($"║  Brokers: {bootstrapServers,-47}║");
Console.WriteLine($"║  Group  : {GroupId,-47}║");
Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
Console.WriteLine("Waiting for order events… (Ctrl+C to stop)");
Console.WriteLine();

// ── Graceful cancellation ────────────────────────────────────────────────────
var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;   // prevent hard process exit; let the loop exit cleanly
    cts.Cancel();
};

// ── Consume loop ─────────────────────────────────────────────────────────────
try
{
    while (!cts.Token.IsCancellationRequested)
    {
        try
        {
            var result = consumer.Consume(cts.Token);
            if (result?.Message?.Value is null) continue;

            var order = JsonSerializer.Deserialize<OrderEventMessage>(result.Message.Value);
            if (order is null) continue;

            var separator = new string('─', 60);
            Console.WriteLine(separator);
            Console.WriteLine($"  EVENT     : {order.EventType}");
            Console.WriteLine($"  Order ID  : {order.OrderId}");
            Console.WriteLine($"  User ID   : {order.UserId}");
            Console.WriteLine($"  Total     : ${order.TotalSum:F2}");
            Console.WriteLine($"  Items     : {order.ItemCount}");
            Console.WriteLine($"  Date      : {order.OrderDate:yyyy-MM-dd}");
            Console.WriteLine($"  Partition : {result.Partition.Value}   Offset: {result.Offset.Value}");
            Console.WriteLine(separator);
            Console.WriteLine();

            consumer.Commit(result);
        }
        catch (ConsumeException ex)
        {
            Console.Error.WriteLine($"[ERROR] Consume failed: {ex.Error.Reason}");
        }
    }
}
catch (OperationCanceledException)
{
    // Normal shutdown — fall through to Close()
}
finally
{
    consumer.Close();
    Console.WriteLine("\n[INFO] OrderNotificationServer stopped.");
}

// ── Local model (mirrors Services.OrderEventMessage) ─────────────────────────
record OrderEventMessage(
    int      OrderId,
    int      UserId,
    double   TotalSum,
    DateOnly OrderDate,
    int      ItemCount,
    string   EventType
);
