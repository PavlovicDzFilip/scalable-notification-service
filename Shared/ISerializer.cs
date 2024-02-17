namespace Notifications;

public interface ISerializer
{
    byte[] Serialize<T>(T data);
    T Deserialize<T>(ReadOnlyMemory<byte> data);
}
