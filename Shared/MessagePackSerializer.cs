namespace Notifications;

public class MessagePackSerializer : ISerializer
{
    public T Deserialize<T>(ReadOnlyMemory<byte> data)
    {
        return MessagePack.MessagePackSerializer.Deserialize<T>(data);
    }

    public byte[] Serialize<T>(T data)
    {
        return MessagePack.MessagePackSerializer.Serialize<T>(data);
    }
}