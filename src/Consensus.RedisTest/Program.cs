using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Redis;
using StackExchange.Redis;
using System.Text.Json;

string redisKey = "test-1";
string redisListkey = $"{redisKey}-list";
string redisHtmlStringkey = $"{redisKey}-html";

// Write logs
var services = new ServiceCollection();
services.AddLogging(x =>
{
    x.AddRedis("localhost:6379", redisListkey);
    x.AddSimpleConsole();
});
var serviceProvider = services.BuildServiceProvider();
var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
logger.LogInformation($"This is a test log message - {DateTime.Now.ToString("t")}");

var multiplexer = ConnectionMultiplexer.Connect("localhost:6379");
var database = multiplexer.GetDatabase();

// Write emphemeral logs
int i = 0;
RedisValue item = database.ListGetByIndex(redisListkey, i);
while (item != RedisValue.Null)
{
    i++;

    string json = item.ToString();
    if (string.IsNullOrEmpty(json))
    {
        var logEntry = JsonSerializer.Deserialize<RedisLogEntry>(json);
        if (logEntry != null)
        {
            Console.WriteLine($"[{logEntry.Timestamp:s}] {logEntry.Message}");
        }
    }

    item = database.ListGetByIndex(redisListkey, i);
}

// Write and read HTML
string html = File.ReadAllText("../../output/responses/output-test1.html");
database.StringAppend(redisHtmlStringkey, html);
var redisValue = await database.StringGetAsync(redisHtmlStringkey);
if (!redisValue.IsNullOrEmpty)
{
    Console.WriteLine(redisValue.ToString().Substring(0,100));
}
