using System;

namespace Phuntasia.Networking.Serialization
{
    public delegate void SerializeHandler<T>(ByteWriter writer, T value);

    public class Serializer<T>
    {
        public static SerializeHandler<T> serialize;
    }
}