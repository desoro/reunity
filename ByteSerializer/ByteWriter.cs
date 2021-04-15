using System;
using System.Collections.Generic;
using System.Text;

namespace Phuntasia.Networking.Serialization
{
    public class ByteWriter : IDisposable
    {
        public int Position => _position;

        static readonly Pool<ByteWriter> _pool;
        static readonly UTF8Encoding _encoding;
        static readonly byte[] _stringBuffer;
        readonly byte[] _buffer;
        int _position;

        static ByteWriter()
        {
            _pool = new Pool<ByteWriter>();
            _encoding = new UTF8Encoding(false, true);
            _stringBuffer = new byte[byte.MaxValue];
        }

        public static ByteWriter Get()
        {
            var writer = _pool.Next();
            writer.Initialize();
            return writer;
        }

        public ByteWriter(byte[] buffer)
        {
            _buffer = buffer;
        }

        public ByteWriter()
        {
            _buffer = new byte[NetworkSettings.MaxMessageSize];
        }

        public void Initialize()
        {
            _position = 0;
        }

        public void Dispose()
        {
            _pool.Return(this);
        }

        public byte[] ToArray()
        {
            var data = new byte[_position];

            Array.ConstrainedCopy(_buffer, 0, data, 0, _position);

            return data;
        }

        public ArraySegment<byte> ToSegment()
        {
            return new ArraySegment<byte>(_buffer, 0, _position);
        }

        public void WriteByte(byte value)
        {
            _buffer[_position++] = value;
        }

        public void WriteByteAt(byte value, int position)
        {
            _buffer[position] = value;
        }

        public void WriteBool(bool value)
        {
            WriteByte((byte)(value ? 1 : 0));
        }

        public void WriteSByte(sbyte value)
        {
            WriteByte((byte)value);
        }

        public void WriteUShort(ushort value)
        {
            WriteByte((byte)(value >> 0));
            WriteByte((byte)(value >> 8));
        }

        public void WriteShort(short value)
        {
            WriteUShort((ushort)value);
        }

        public void WriteChar(char value)
        {
            WriteShort((short)value);
        }

        public void WriteUInt(uint value)
        {
            WriteByte((byte)(value >> 0));
            WriteByte((byte)(value >> 8));
            WriteByte((byte)(value >> 16));
            WriteByte((byte)(value >> 24));
        }

        public void WriteInt(int value)
        {
            WriteUInt((uint)value);
        }

        public void WriteFloat(float value)
        {
            var converter = new UIntFloat
            {
                floatValue = value
            };

            WriteUInt(converter.intValue);
        }

        public void WriteULong(ulong value)
        {
            WriteByte((byte)(value >> 0));
            WriteByte((byte)(value >> 8));
            WriteByte((byte)(value >> 16));
            WriteByte((byte)(value >> 24));
            WriteByte((byte)(value >> 32));
            WriteByte((byte)(value >> 40));
            WriteByte((byte)(value >> 48));
            WriteByte((byte)(value >> 56));
        }

        public void WriteLong(long value)
        {
            WriteULong((ulong)value);
        }

        public void WriteDouble(double value)
        {
            var converter = new ULongDouble
            {
                doubleValue = value
            };

            WriteULong(converter.longValue);
        }

        public void WriteDecimal(decimal value)
        {
            var converter = new ULongDecimal
            {
                decimalValue = value
            };

            WriteULong(converter.longValue1);
            WriteULong(converter.longValue2);
        }

        public void WriteString(string value)
        {
            if (value == null)
            {
                WriteByte(0);
                return;
            }

            var size = _encoding.GetBytes(value, 0, value.Length, _stringBuffer, 0);

            if (size >= byte.MaxValue)
            {
                throw new Exception($"WriteString: {size} >= {byte.MaxValue}");
            }

            WriteByte(checked((byte)(size + 1)));
            WriteBytes(_stringBuffer, 0, size);
        }

        public void WriteDateTime(DateTime value)
        {
            WriteLong(value.Ticks);
        }

        public void WriteTimeSpan(TimeSpan value)
        {
            WriteLong(value.Ticks);
        }

        public void WriteGuid(Guid value)
        {
            var data = value.ToByteArray();
            WriteBytes(data, 0, data.Length);
        }

        public void WriteVector2D(Vector2D value)
        {
            WriteFloat(value.x);
            WriteFloat(value.y);
        }

        public void WriteVector3D(Vector3D value)
        {
            WriteFloat(value.x);
            WriteFloat(value.y);
            WriteFloat(value.z);
        }

        public void WriteVector4D(Vector4D value)
        {
            WriteFloat(value.x);
            WriteFloat(value.y);
            WriteFloat(value.z);
            WriteFloat(value.w);
        }

        public void WriteSegmentPacked(ArraySegment<byte> value)
        {
            WriteUShort((ushort)value.Count);
            WriteBytes(value.Array, value.Offset, value.Count);
        }

        public void WriteBytes(byte[] data)
        {
            Array.ConstrainedCopy(data, 0, _buffer, _position, data.Length);

            _position += data.Length;
        }

        public void WriteBytes(byte[] data, int offset, int count)
        {
            Array.ConstrainedCopy(data, offset, _buffer, _position, count);

            _position += count;
        }

        public void WriteArray<T>(T[] value)
        {
            WriteUShort((ushort)value.Length);

            for (int i = 0; i < value.Length; i++)
            {
                SerializationRegistry.Serialize(this, value[i]);
            }
        }

        public void WriteList<T>(List<T> value)
        {
            WriteUShort((ushort)value.Count);

            for (int i = 0; i < value.Count; i++)
            {
                SerializationRegistry.Serialize(this, value[i]);
            }
        }

        public void WriteDictionary<TKey, TValue>(Dictionary<TKey, TValue> value)
        {
            WriteUShort((ushort)value.Count);

            foreach (var kvp in value)
            {
                SerializationRegistry.Serialize(this, kvp.Key);
                SerializationRegistry.Serialize(this, kvp.Value);
            }
        }

        public void WriteTuple(ValueTuple value)
        {
        }

        public void WriteTuple1<T1>(ValueTuple<T1> value)
        {
            SerializationRegistry.Serialize(this, value.Item1);
        }

        public void WriteTuple2<T1, T2>(ValueTuple<T1, T2> value)
        {
            SerializationRegistry.Serialize(this, value.Item1);
            SerializationRegistry.Serialize(this, value.Item2);
        }

        public void WriteTuple3<T1, T2, T3>(ValueTuple<T1, T2, T3> value)
        {
            SerializationRegistry.Serialize(this, value.Item1);
            SerializationRegistry.Serialize(this, value.Item2);
            SerializationRegistry.Serialize(this, value.Item3);
        }

        public void WriteTuple4<T1, T2, T3, T4>(ValueTuple<T1, T2, T3, T4> value)
        {
            SerializationRegistry.Serialize(this, value.Item1);
            SerializationRegistry.Serialize(this, value.Item2);
            SerializationRegistry.Serialize(this, value.Item3);
            SerializationRegistry.Serialize(this, value.Item4);
        }

        public void Write<T>(T value)
        {
            SerializationRegistry.Serialize(this, value);
        }
    }
}