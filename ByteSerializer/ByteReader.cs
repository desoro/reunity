using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Phuntasia.Networking.Serialization
{
    public class ByteReader : IDisposable
    {
        public int Length => _length;
        public int Position => _position;
        public bool HasReachedEnd => _position == _length;

        static readonly Pool<ByteReader> _pool;
        static readonly UTF8Encoding _encoding;
        ArraySegment<byte> _buffer;
        int _length;
        int _position;

        static ByteReader()
        {
            _pool = new Pool<ByteReader>();
            _encoding = new UTF8Encoding(false, true);
        }

        public static ByteReader Get(ArraySegment<byte> segment)
        {
            var reader = _pool.Next();
            reader.Initialize(segment);
            return reader;
        }

        public void Initialize(ArraySegment<byte> segment)
        {
            _buffer = segment;
            _length = _buffer.Count;
            _position = 0;
        }

        public void Dispose()
        {
            _pool.Return(this);
        }

        public byte ReadByte()
        {
            if (_position + 1 > _length)
            {
                throw new EndOfStreamException("ReadByte out of range");
            }

            return _buffer.Array[_buffer.Offset + _position++];
        }

        public bool ReadBool()
        {
            return ReadByte() == 1;
        }

        public sbyte ReadSByte()
        {
            return (sbyte)ReadByte();
        }

        public ushort ReadUShort()
        {
            ushort value = 0;
            value |= (ushort)(ReadByte() << 0);
            value |= (ushort)(ReadByte() << 8);
            return value;
        }

        public short ReadShort()
        {
            return (short)ReadUShort();
        }

        public char ReadChar()
        {
            return (char)ReadShort();
        }

        public uint ReadUInt()
        {
            uint value = 0;
            value |= (uint)(ReadByte() << 0);
            value |= (uint)(ReadByte() << 8);
            value |= (uint)(ReadByte() << 16);
            value |= (uint)(ReadByte() << 24);
            return value;
        }

        public int ReadInt()
        {
            return (int)ReadUInt();
        }

        public ulong ReadULong()
        {
            ulong value = 0;
            value |= ((ulong)ReadByte()) << 0;
            value |= ((ulong)ReadByte()) << 8;
            value |= ((ulong)ReadByte()) << 16;
            value |= ((ulong)ReadByte()) << 24;
            value |= ((ulong)ReadByte()) << 32;
            value |= ((ulong)ReadByte()) << 40;
            value |= ((ulong)ReadByte()) << 48;
            value |= ((ulong)ReadByte()) << 56;
            return value;
        }

        public long ReadLong()
        {
            return (long)ReadULong();
        }

        public float ReadFloat()
        {
            var converter = new UIntFloat
            {
                intValue = ReadUInt()
            };

            return converter.floatValue;
        }

        public double ReadDouble()
        {
            var converter = new ULongDouble
            {
                longValue = ReadULong()
            };

            return converter.doubleValue;
        }

        public decimal ReadDecimal()
        {
            var converter = new ULongDecimal
            {
                longValue1 = ReadULong(),
                longValue2 = ReadULong()
            };

            return converter.decimalValue;
        }

        public string ReadString()
        {
            var size = ReadByte();

            if (size == 0)
            {
                return null;
            }

            var realSize = size - 1;

            if (realSize >= byte.MaxValue)
            {
                throw new EndOfStreamException($"ReadString: {realSize} >= {byte.MaxValue}");
            }

            var data = ReadSegment(realSize);

            return _encoding.GetString(data.Array, data.Offset, data.Count);
        }

        public DateTime ReadDateTime()
        {
            return new DateTime(ReadLong());
        }

        public TimeSpan ReadTimeSpan()
        {
            return TimeSpan.FromTicks(ReadLong());
        }

        public Guid ReadGuid()
        {
            return new Guid(ReadBytes(16));
        }

        public Vector2D ReadVector2D()
        {
            return new Vector2D
            {
                x = ReadFloat(),
                y = ReadFloat()
            };
        }

        public Vector3D ReadVector3D()
        {
            return new Vector3D
            {
                x = ReadFloat(),
                y = ReadFloat(),
                z = ReadFloat()
            };
        }

        public Vector4D ReadVector4D()
        {
            return new Vector4D
            {
                x = ReadFloat(),
                y = ReadFloat(),
                z = ReadFloat(),
                w = ReadFloat()
            };
        }

        public ArraySegment<byte> ReadSegmentPacked()
        {
            var count = ReadUShort();

            if (_position + count > _buffer.Count)
            {
                throw new EndOfStreamException($"ReadSegment: {_position}, {count}, {_buffer.Count}");
            }

            var result = new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _position, count);

            _position += count;

            return result;
        }

        public ArraySegment<byte> ReadSegment(int count)
        {
            if (_position + count > _buffer.Count)
            {
                throw new EndOfStreamException($"ReadSegment: {_position}, {count}, {_buffer.Count}");
            }

            var result = new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _position, count);

            _position += count;

            return result;
        }

        public ArraySegment<byte> ReadToEnd()
        {
            var count = _buffer.Count - _position;

            return ReadSegment(count);
        }

        public byte[] ReadBytes(int count)
        {
            var buffer = new byte[count];
            var data = ReadSegment(count);

            Array.Copy(data.Array, data.Offset, buffer, 0, count);

            return buffer;
        }

        public T[] ReadArray<T>()
        {
            var length = ReadUShort();
            var value = new T[length];

            for (int i = 0; i < length; i++)
            {
                value[i] = SerializationRegistry.Deserialize<T>(this);
            }

            return value;
        }

        public List<T> ReadList<T>()
        {
            var count = ReadUShort();
            var value = new List<T>();

            for (int i = 0; i < count; i++)
            {
                value.Add(SerializationRegistry.Deserialize<T>(this));
            }

            return value;
        }

        public Dictionary<TKey, TValue> ReadDictionary<TKey, TValue>()
        {
            var count = ReadUShort();
            var value = new Dictionary<TKey, TValue>();

            for (int i = 0; i < count; i++)
            {
                value.Add(SerializationRegistry.Deserialize<TKey>(this), SerializationRegistry.Deserialize<TValue>(this));
            }

            return value;
        }

        public ValueTuple ReadTuple()
        {
            return default(ValueTuple);
        }

        public ValueTuple<T1> ReadTuple1<T1>()
        {
            return new ValueTuple<T1>
            {
                Item1 = SerializationRegistry.Deserialize<T1>(this)
            };
        }

        public ValueTuple<T1, T2> ReadTuple2<T1, T2>()
        {
            return new ValueTuple<T1, T2>
            {
                Item1 = SerializationRegistry.Deserialize<T1>(this),
                Item2 = SerializationRegistry.Deserialize<T2>(this)
            };
        }

        public ValueTuple<T1, T2, T3> ReadTuple3<T1, T2, T3>()
        {
            return new ValueTuple<T1, T2, T3>
            {
                Item1 = SerializationRegistry.Deserialize<T1>(this),
                Item2 = SerializationRegistry.Deserialize<T2>(this),
                Item3 = SerializationRegistry.Deserialize<T3>(this)
            };
        }

        public ValueTuple<T1, T2, T3, T4> ReadTuple4<T1, T2, T3, T4>()
        {
            return new ValueTuple<T1, T2, T3, T4>
            {
                Item1 = SerializationRegistry.Deserialize<T1>(this),
                Item2 = SerializationRegistry.Deserialize<T2>(this),
                Item3 = SerializationRegistry.Deserialize<T3>(this),
                Item4 = SerializationRegistry.Deserialize<T4>(this)
            };
        }

        public T Read<T>()
        {
            return SerializationRegistry.Deserialize<T>(this);
        }
    }
}