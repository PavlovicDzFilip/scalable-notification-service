using StackExchange.Redis;

namespace Notifications.Consumer;

public class RedisNotificationLogCache(
    IDatabase database,
    ISerializer serializer) : INotificationLogCache
{
    private readonly RedisKey _redisKey = new RedisKey("notifications");
    public async Task AddAsync(NotificationLog notificationLog)
    {
        var data = serializer.Serialize(notificationLog);
        await database.HashSetAsync(
                       _redisKey,
                       [new HashEntry(notificationLog.Id.ToString(), data)]);
    }

    public async Task<bool> IsHandledAsync(long id)
    {
        return await database
            .HashExistsAsync(
                _redisKey,
                new RedisValue(id.ToString()));
    }
}

