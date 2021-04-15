using Phuntasia.Networking.Serialization;
using System;
using System.Collections.Generic;

namespace Phuntasia.Networking.Messaging
{
    public class MessageRegistry
    {
        static readonly LogChannel Log = LogManager.GetLog<MessageRegistry>();

        static readonly Dictionary<ushort, MessageConfig> _configByHash;
        static readonly Dictionary<Type, MessageConfig> _configByType;

        public class MessageConfig
        {
            public Type type;
            public ushort hash;
            public bool isUserMessage;
        }

        static MessageRegistry()
        {
            _configByType = new Dictionary<Type, MessageConfig>();
            _configByHash = new Dictionary<ushort, MessageConfig>();

            RegisterSystemMessages();
        }

        static void RegisterSystemMessages()
        {
            RegisterSystem<AuthMessage>();
            RegisterSystem<PingMessage>();
            RegisterSystem<PongMessage>();
            RegisterSystem<KickMessage>();
        }

        public static void RegisterUser<T>()
            where T : IUserMessageData
        {
            RegisterInternal<T>(true);
        }

        public static void RegisterSystem<T>(bool forceAsUserMessage = false)
            where T : ISystemMessageData
        {
            SerializationRegistry.Register(
            (w, v) =>
            {
                v.Serialize(w);
            },
            (r) =>
            {
                var value = default(T);
                value.Deserialize(r);
                return value;
            });

            RegisterInternal<T>(forceAsUserMessage);
        }

        static void RegisterInternal<T>(bool isUserMessage)
            where T : IMessageData
        {
            var type = typeof(T);

            var config = new MessageConfig
            {
                type = type,
                hash = GetMessageHash(type),
                isUserMessage = isUserMessage
            };

            _configByHash.Add(config.hash, config);
            _configByType.Add(config.type, config);

            Log.Verbose?.Invoke($"registered {typeof(T)}");
        }

        public static MessageConfig GetConfig(Type type)
        {
            if (_configByType.TryGetValue(type, out var config))
            {
                return config;
            }
            else
            {
                throw new Exception($"Message is not registered, {type}");
            }
        }

        public static MessageConfig GetConfig(ushort hash)
        {
            if (_configByHash.TryGetValue(hash, out var config))
            {
                return config;
            }
            else
            {
                throw new Exception($"Message is not registered, {hash}");
            }
        }

        static ushort GetMessageHash(Type type)
        {
            unchecked
            {
                var hash = 23;

                foreach (var c in type.FullName)
                {
                    hash = hash * 31 + c;
                }

                return (ushort)(hash & 0xFFFF);
            }
        }
    }
}