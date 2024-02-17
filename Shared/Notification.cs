using MessagePack;

namespace Notifications;

[MessagePackObject]
public class Notification
{
    [Key(0)]
    public long Id { get; set; }
    [Key(1)]
    public string Payload { get; set; } = null!;
    [Key(2)]
    public DateTime SendDate { get; set; }
}
