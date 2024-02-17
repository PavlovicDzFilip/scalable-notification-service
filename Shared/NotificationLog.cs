using MessagePack;

namespace Notifications;

[MessagePackObject]
public class NotificationLog
{
    [Key(0)]
    public long Id { get; set; }
    [Key(1)]
    public DateTime SentDate { get; set; }
}