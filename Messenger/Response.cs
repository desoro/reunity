using System;

namespace Phuntasia.Networking.Messaging
{
    public struct Response<T>
        where T : IMessageData, new()
    {
        public int RequestId { get; }
        public bool HasError { get; }
        public string Error { get; }
        public T Data { get; }

        public Response(int requestId, string error)
        {
            RequestId = requestId;
            HasError = true;
            Error = error;
            Data = default;
        }

        public Response(int requestId, T data)
        {
            RequestId = requestId;
            HasError = false;
            Error = null;
            Data = data;
        }
    }
}