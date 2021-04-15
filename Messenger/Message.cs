using System;

namespace Phuntasia.Networking.Messaging
{
    public struct Message<T>
        where T : IMessageData, new()
    {
        public MessageHandler Handler { get; }
        public int Id { get; }
        public T Data { get; }

        public Message(MessageHandler handler, int id, T data)
        {
            Id = id;
            Handler = handler;
            Data = data;
        }

        public void Respond<TRsp>(TRsp msg)
            where TRsp : IMessageData, new()
        {
            Handler.Respond(new Response<TRsp>(Id, msg));
        }

        public void Respond<TRsp>(string error)
            where TRsp : IMessageData, new()
        {
            Handler.Respond(new Response<TRsp>(Id, error));
        }
    }
}