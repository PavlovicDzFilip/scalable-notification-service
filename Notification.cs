namespace Notifications;

public class Notification
{
    public long Id { get; set; }
    public string Payload { get; set; } = null!;
    public DateTime SendDate { get; set; }
}