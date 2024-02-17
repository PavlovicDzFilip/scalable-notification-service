using System.Text;

namespace Notifications;

public class JsonSerializer : ISerializer
{
    public byte[] Serialize<T>(T data)
    {
        return Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(data));
    }
    public T Deserialize<T>(ReadOnlyMemory<byte> data)
    {
        return System.Text.Json.JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(data.Span))!;
    }
}
