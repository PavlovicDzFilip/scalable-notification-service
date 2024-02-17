using System;

namespace Notifications;

public interface ISerializer
{
    ReadOnlyMemory<byte> Serialize<T>(T data);
    T Deserialize<T>(ReadOnlyMemory<byte> data);
}
