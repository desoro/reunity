using System;

namespace Phuntasia.Networking.Serialization
{
    public delegate T DeserializeHandler<T>(ByteReader reader);

    public class Deserializer<T>
    {
        public static DeserializeHandler<T> deserialize;
    }
}