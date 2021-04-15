using System.Net.Sockets;
using System.Threading;

namespace Phuntasia.Networking.Transport
{
    class Connection
    {
        public int Id { get; private set; }
        public bool IsActive { get; private set; }
        public TcpClient TcpClient { get; private set; }
        public NetworkStream NetworkStream { get; private set; }
        public CancellationTokenSource TokenSource { get; private set; }
        public CancellationToken Token { get; private set; }
        public string IpPort { get; private set; }
        public MessageBuffer SendBuffer { get; private set; }
        public MessageBuffer ReceiveBuffer { get; private set; }

        public Connection(ServerTransport server)
        {
            SendBuffer = new MessageBuffer(server.SendBufferSize);
            ReceiveBuffer = new MessageBuffer(server.ReceiveBufferSize);
        }

        public void Initialize(int id, TcpClient client)
        {
            Id = id;
            IsActive = true;
            TcpClient = client;
            NetworkStream = client.GetStream();
            TokenSource = new CancellationTokenSource();
            Token = TokenSource.Token;
            IpPort = TcpClient.Client.RemoteEndPoint.ToString();
        }

        public void Reset()
        {
            IsActive = false;

            if (TokenSource != null)
            {
                if (!TokenSource.IsCancellationRequested)
                {
                    TokenSource.Cancel();
                    TokenSource.Dispose();
                }
            }

            if (NetworkStream != null)
            {
                NetworkStream.Close();
            }

            if (TcpClient != null)
            {
                TcpClient.Close();
                TcpClient.Dispose();
            }
        }
    }
}