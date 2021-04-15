using System.Threading.Tasks;

namespace Phuntasia.Networking.Messaging
{
    public struct MessageReceipt
    {
        public MessageHandler Handler { get; }
        public int Id { get; }

        public MessageReceipt(MessageHandler handler, int id)
        {
            Id = id;
            Handler = handler;
        }
        public Task<Response<T>> WaitForResponse<T>(int timeout = 5000)
            where T : IMessageData, new()
        {
            return Handler.WaitForResponse<T>(Id, timeout);
        }
    }
}