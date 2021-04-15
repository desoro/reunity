using Phuntasia.Networking.Serialization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Phuntasia.Networking.Messaging
{
    public delegate void MessageCallback<T>(Message<T> msg) where T : IMessageData, new();
    delegate void MessageCallback(int id, ByteReader reader);
    delegate void ResponseCallback(ByteReader reader);

    public abstract class MessageHandler
    {
        static readonly LogChannel Log = LogManager.GetLog<MessageHandler>();

        public abstract bool IsActive { get; }
        public double TimeLastUserMessage { get; internal set; }

        readonly Dictionary<ushort, MessageCallback> _messageListeners;
        readonly Dictionary<int, ResponseCallback> _responseListeners;
        readonly IdGenerator _idGen;

        public MessageHandler()
        {
            _messageListeners = new Dictionary<ushort, MessageCallback>();
            _responseListeners = new Dictionary<int, ResponseCallback>();
            _idGen = new IdGenerator(ushort.MaxValue);
        }

        public void SetListener<T>(MessageCallback<T> callback)
            where T : IMessageData, new()
        {
            var config = MessageRegistry.GetConfig(typeof(T));

            _messageListeners[config.hash] = (id, reader) =>
            {
                var data = reader.Read<T>();
                var msg = new Message<T>(this, id, data);

                callback.Invoke(msg);
            };
        }

        public void ClearListener<T>()
            where T : IMessageData, new()
        {
            var config = MessageRegistry.GetConfig(typeof(T));

            _messageListeners.Remove(config.hash);
        }

        public MessageReceipt Send<T>(T msg)
           where T : IMessageData, new()
        {
            if (!IsActive)
            {
                Log.Warn?.Invoke("cant send while not active.");
                return default;
            }

            var config = MessageRegistry.GetConfig(typeof(T));
            var writer = ByteWriter.Get(); 
            var messageId = _idGen.NextInt();

            writer.WriteByte((byte)MessageType.Simple);
            writer.WriteUShort(config.hash);
            writer.WriteUShort((ushort)messageId);
            writer.Write(msg);

            SendBytes(writer.ToSegment());

            writer.Dispose();

            return new MessageReceipt(this, messageId);
        }

        protected abstract void SendBytes(ArraySegment<byte> data);

        internal async Task<Response<T>> WaitForResponse<T>(int requestId, int timeout = 5000)
            where T : IMessageData, new()
        {

            var response = default(Response<T>);

            if (!IsActive)
            {
                response = new Response<T>(requestId, "Network is not active.");
                Log.Warn?.Invoke($"{typeof(T).Name} sent when network is not active.");
            }
            else
            {
                var hasResponse = false;

                _responseListeners[requestId] = onResponse;

                void onResponse(ByteReader reader)
                {
                    var hasError = reader.ReadBool();

                    if (hasError)
                    {
                        response = new Response<T>(requestId, reader.ReadString());
                    }
                    else
                    {
                        response = new Response<T>(requestId, reader.Read<T>());
                    }

                    hasResponse = true;
                }

                var success = await Tasks.WaitUntil(() => hasResponse, timeout);

                if (!success)
                {
                    response = new Response<T>(requestId, "Request timed out.");
                    Log.Warn?.Invoke($"{typeof(T).Name} timed out.");
                }

                _responseListeners.Remove(requestId);
            }            

            return response;
        }

        internal void Respond<T>(Response<T> rsp)
           where T : IMessageData, new()
        {
            if (!IsActive)
            {
                Log.Warn?.Invoke($"{typeof(T).Name} sent when network is not active.");
                return;
            }

            var config = MessageRegistry.GetConfig(typeof(T));
            var writer = ByteWriter.Get();

            writer.WriteByte((byte)MessageType.Response);
            writer.WriteUShort(config.hash);
            writer.WriteUShort((ushort)rsp.RequestId);
            writer.WriteBool(rsp.HasError);

            if (rsp.HasError)
            {
                writer.WriteString(rsp.Error);
            }
            else
            {
                writer.Write(rsp.Data);
            }            

            SendBytes(writer.ToSegment());

            writer.Dispose();
        }

        internal void Unpack(ArraySegment<byte> data)
        {
            var reader = ByteReader.Get(data);

            try
            {
                var msgType = (MessageType)reader.ReadByte();
                var hash = reader.ReadUShort();
                var messageId = reader.ReadUShort();
                var config = MessageRegistry.GetConfig(hash);

                if (msgType == MessageType.Simple)
                {
                    if (_messageListeners.TryGetValue(hash, out var listener))
                    {
                        listener.Invoke(messageId, reader);
                    }
                    else
                    {
                        Log.Warn?.Invoke($"discarded simple message, {config.type}.");
                    }
                }
                else if (msgType == MessageType.Response)
                {
                    if (_responseListeners.TryGetValue(messageId, out var listener))
                    {
                        listener.Invoke(reader);
                    }
                    else
                    {
                        Log.Warn?.Invoke($"discarded response message, {config.type}({messageId}).");
                    }
                }
                else
                {
                    throw new Exception($"{msgType} is not valid.");
                }

                if (config.isUserMessage)
                {
                    TimeLastUserMessage = Timer.Time;
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }

            reader.Dispose();
        }

        internal virtual void Reset()
        {
            _messageListeners.Clear();
            _responseListeners.Clear();
            _idGen.Reset();
        }
    }
}