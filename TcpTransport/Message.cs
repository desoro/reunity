using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Phuntasia.Networking.Transport
{
    class Message
    {
        public const int HeaderSize = 2;

        public int Size { get; private set; }
        public ArraySegment<byte> Payload { get; private set; }

        readonly int _maxPayloadSize;
        readonly byte[] _buffer;

        public Message()
        {
            _maxPayloadSize = NetworkSettings.MaxMessageSize - HeaderSize;
            _buffer = new byte[_maxPayloadSize];
        }

        public async Task<bool> WriteToStream(NetworkStream stream, ArraySegment<byte> data, CancellationToken token)
        {
            try
            {
                if (data.Count > _maxPayloadSize)
                {
                    throw new Exception($"Invalid payload size, {data.Count} > {_maxPayloadSize}.");
                }

                Size = 0;

                _buffer[Size++] = (byte)(data.Count);
                _buffer[Size++] = (byte)(data.Count >> 8);

                Array.Copy(data.Array, data.Offset, _buffer, Size, data.Count);

                Size += data.Count;
                Payload = new ArraySegment<byte>(_buffer, 0, data.Count);

                await stream.WriteAsync(_buffer, 0, Size, token).ConfigureAwait(false);

                return true;
            }
            catch (IOException)
            {
                return false;
            }
        }

        public async Task<bool> ReadFromStream(NetworkStream stream, CancellationToken token)
        {
            try
            {
                var headerSuccess = await ReadCount(stream, HeaderSize, token).ConfigureAwait(false);

                if (!headerSuccess)
                {
                    return false;
                }

                var payloadSize = 0;
                payloadSize |= _buffer[0];
                payloadSize |= _buffer[1] << 8;

                if (payloadSize > _maxPayloadSize)
                {
                    throw new Exception($"Invalid payload size, {payloadSize} > {_maxPayloadSize}.");
                }

                var payloadSuccess = await ReadCount(stream, payloadSize, token).ConfigureAwait(false);

                if (!payloadSuccess)
                {
                    return false;
                }

                Size = HeaderSize + payloadSize;
                Payload = new ArraySegment<byte>(_buffer, 0, payloadSize);

                return true;
            }
            catch (IOException)
            {
                return false;
            }
        }

        async Task<bool> ReadCount(NetworkStream stream, int count, CancellationToken token)
        {
            var bytesRead = 0;

            while (bytesRead < count)
            {
                var remaining = count - bytesRead;
                var result = await stream.ReadAsync(_buffer, bytesRead, remaining, token).ConfigureAwait(false);

                if (result == 0)
                {
                    return false;
                }

                bytesRead += result;
            }

            return true;
        }
    }
}