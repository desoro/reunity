using Phuntasia.Networking.Serialization;

namespace Phuntasia.Networking.Messaging
{
    public interface IMessageData
    {
    }

    public interface ISystemMessageData : IMessageData, ICustomSerialized
    {
    }

    public interface IUserMessageData : IMessageData
    {
    }
}