using System;
using System.Collections.Generic;

namespace Phuntasia.Networking.Serialization
{
    public class SerializationRegistry
    {
        static readonly LogChannel Log = LogManager.GetLog<SerializationRegistry>();
        static readonly HashSet<Type> _customTypes;

        public static IReadOnlyCollection<Type> CustomTypes => _customTypes;

        static SerializationRegistry()
        {
            _customTypes = new HashSet<Type>();

            RegisterSupportedWriters();
            RegisterSupportedReaders();
        }

        static void RegisterSupportedWriters()
        {
            Serializer<byte>.serialize = (w, v) => w.WriteByte(v);
            Serializer<sbyte>.serialize = (w, v) => w.WriteSByte(v);
            Serializer<bool>.serialize = (w, v) => w.WriteBool(v);
            Serializer<ushort>.serialize = (w, v) => w.WriteUShort(v);
            Serializer<short>.serialize = (w, v) => w.WriteShort(v);
            Serializer<char>.serialize = (w, v) => w.WriteChar(v);
            Serializer<uint>.serialize = (w, v) => w.WriteUInt(v);
            Serializer<int>.serialize = (w, v) => w.WriteInt(v);
            Serializer<ulong>.serialize = (w, v) => w.WriteULong(v);
            Serializer<long>.serialize = (w, v) => w.WriteLong(v);
            Serializer<float>.serialize = (w, v) => w.WriteFloat(v);
            Serializer<double>.serialize = (w, v) => w.WriteDouble(v);
            Serializer<decimal>.serialize = (w, v) => w.WriteDecimal(v);
            Serializer<string>.serialize = (w, v) => w.WriteString(v);
            Serializer<DateTime>.serialize = (w, v) => w.WriteDateTime(v);
            Serializer<TimeSpan>.serialize = (w, v) => w.WriteTimeSpan(v);
            Serializer<Guid>.serialize = (w, v) => w.WriteGuid(v);

            Serializer<Vector2D>.serialize = (w, v) => w.WriteVector2D(v);
            Serializer<Vector3D>.serialize = (w, v) => w.WriteVector3D(v);
            Serializer<Vector4D>.serialize = (w, v) => w.WriteVector4D(v);
        }

        static void RegisterSupportedReaders()
        {
            Deserializer<byte>.deserialize = (r) => r.ReadByte();
            Deserializer<sbyte>.deserialize = (r) => r.ReadSByte();
            Deserializer<bool>.deserialize = (r) => r.ReadBool();
            Deserializer<ushort>.deserialize = (r) => r.ReadUShort();
            Deserializer<short>.deserialize = (r) => r.ReadShort();
            Deserializer<char>.deserialize = (r) => r.ReadChar();
            Deserializer<uint>.deserialize = (r) => r.ReadUInt();
            Deserializer<int>.deserialize = (r) => r.ReadInt();
            Deserializer<ulong>.deserialize = (r) => r.ReadULong();
            Deserializer<long>.deserialize = (r) => r.ReadLong();
            Deserializer<float>.deserialize = (r) => r.ReadFloat();
            Deserializer<double>.deserialize = (r) => r.ReadDouble();
            Deserializer<decimal>.deserialize = (r) => r.ReadDecimal();
            Deserializer<string>.deserialize = (r) => r.ReadString();
            Deserializer<DateTime>.deserialize = (r) => r.ReadDateTime();
            Deserializer<TimeSpan>.deserialize = (r) => r.ReadTimeSpan();
            Deserializer<Guid>.deserialize = (r) => r.ReadGuid();

            Deserializer<Vector2D>.deserialize = (r) => r.ReadVector2D();
            Deserializer<Vector3D>.deserialize = (r) => r.ReadVector3D();
            Deserializer<Vector4D>.deserialize = (r) => r.ReadVector4D();
        }

        public static void Register<T>(SerializeHandler<T> serializer, DeserializeHandler<T> deserializer)
        {
            Serializer<T>.serialize = serializer;
            Deserializer<T>.deserialize = deserializer;

            _customTypes.Add(typeof(T));

            Log.Verbose?.Invoke($"registered {typeof(T)}");
        }

        public static void Serialize<T>(ByteWriter writer, T value)
        {
            if (Serializer<T>.serialize == null)
            {
                throw new Exception($"{typeof(T)} is not a registered type.");
            }

            Serializer<T>.serialize(writer, value);
        }

        public static T Deserialize<T>(ByteReader reader)
        {
            if (Deserializer<T>.deserialize == null)
            {
                throw new Exception($"{typeof(T)} is not a registered type.");
            }

            return Deserializer<T>.deserialize(reader);
        }
    }
}