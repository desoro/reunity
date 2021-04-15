using System.Net.Sockets;

namespace Phuntasia.Networking.Transport
{
    class ConnectionPool
    {
        readonly Connection[] _buffer;
        readonly IdGenerator _idGenerator;
        int _index;

        public ConnectionPool(ServerTransport server)
        {
            _buffer = new Connection[server.MaxConnections];
            _idGenerator = new IdGenerator(int.MaxValue);

            for (int i = 0; i < _buffer.Length; i++)
            {
                _buffer[i] = new Connection(server);
            }
        }

        public void Reset()
        {
            _index = 0;
            _idGenerator.Reset(); 
        }

        public Connection Next(TcpClient client)
        {
            if (_index == _buffer.Length)
            {
                _index = 0;
            }

            var value = _buffer[_index++];
            var id = _idGenerator.NextInt();

            value.Initialize(id, client);

            return value;
        }
    }
}