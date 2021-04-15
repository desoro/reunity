using System;

namespace Phuntasia.Networking.Serialization
{
    public interface ICustomSerialized
    {
        void Serialize(ByteWriter writer);
        void Deserialize(ByteReader reader);
    }
}