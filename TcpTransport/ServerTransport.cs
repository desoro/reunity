using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Phuntasia.Networking.Transport
{
    public class ServerTransport
    {
        static readonly LogChannel Log = LogManager.GetLog<ServerTransport>();

        public bool NoDelay { get; set; } = false;
        public int MaxConnections { get; set; } = 100;
        public int AcceptInterval { get; set; } = 100;
        public int SendBufferSize { get; set; } = 16;
        public int ReceiveBufferSize { get; set; } = 16;
        public bool IsActive => _isRunning;
        public int ConnectionCount => _connections.Count;

        ConnectionPool _connectionPool;
        readonly ConcurrentDictionary<int, Connection> _connections;
        TcpListener _listener;
        CancellationTokenSource _tokenSource;
        CancellationToken _token;
        ConcurrentQueue<ServerEvent> _eventQueue;
        bool _isRunning;
        bool _isListening;

        public ServerTransport()
        {
            _connections = new ConcurrentDictionary<int, Connection>();
            _eventQueue = new ConcurrentQueue<ServerEvent>();
        }

        public void Start(int port)
        {
            if (_isRunning) throw new InvalidOperationException("Server is already running.");

            if (_connectionPool == null)
            {
                _connectionPool = new ConnectionPool(this);
            }

            _isRunning = true;

            _listener = new TcpListener(IPAddress.Any, port);
            _listener.Server.NoDelay = NoDelay;

            _tokenSource = new CancellationTokenSource();
            _token = _tokenSource.Token;
            _eventQueue = new ConcurrentQueue<ServerEvent>();

            _ = Task.Run(() => AcceptLoop(), _token);

            Log.Info?.Invoke($"listening on port {port}.");
        }

        public void Stop()
        {
            if (!_isRunning) return;

            _isRunning = false;

            try
            {
                foreach (var conn in _connections.Values.ToArray())
                {
                    DisconnectInternal(conn);
                }

                if (_isListening)
                {
                    _isListening = false;
                    _listener.Stop();
                }

                _tokenSource.Cancel();

                _connectionPool.Reset();
                _connections.Clear();
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }

        public void Send(int connId, ArraySegment<byte> data)
        {
            if (!_isRunning) throw new InvalidOperationException("Server is not running.");

            if (_connections.TryGetValue(connId, out var conn))
            {
                SendInternal(conn, data);
            }
            else
            {
                Log.Warn?.Invoke($"send failed, could not find connection {connId}.");
            }
        }

        public void Broadcast(ArraySegment<byte> data)
        {
            if (!_isRunning) throw new InvalidOperationException("Server is not running.");

            foreach (var conn in _connections.Values)
            {
                SendInternal(conn, data);
            }
        }

        public bool TryNextEvent(out ServerEvent evnt)
        {
            return _eventQueue.TryDequeue(out evnt);
        }

        public void Disconnect(int connId)
        {
            if (!_isRunning) throw new InvalidOperationException("Server is not running.");

            if (_connections.TryGetValue(connId, out var conn))
            {
                DisconnectInternal(conn);
            }
            else
            {
                Log.Warn?.Invoke($"disconnect failed, could not find connection {connId}.");
            }
        }

        async void AcceptLoop()
        {
            _listener.Start();
            _isListening = true;

            while (true)
            {
                try
                {
                    if (_connections.Count >= MaxConnections)
                    {
                        if (_isListening)
                        {
                            _listener.Stop();
                            _isListening = false;
                        }

                        await Task.Delay(AcceptInterval, _token).ConfigureAwait(false);

                        continue;
                    }
                    else if (!_isListening)
                    {
                        _listener.Start();
                        _isListening = true;
                    }

                    var client = await _listener.AcceptTcpClientAsync().ConfigureAwait(false);
                    var conn = _connectionPool.Next(client);

                    _connections.TryAdd(conn.Id, conn);

                    Log.Info?.Invoke($"added connection {conn.Id}:{conn.IpPort}.");

                    _eventQueue.Enqueue(new ServerEvent
                    {
                        type = EventType.Connected,
                        connId = conn.Id
                    });

                    var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_token, conn.Token);

                    _ = Task.Run(() => ReceiveLoop(conn), linkedCts.Token);

                    await Task.Delay(AcceptInterval, _token).ConfigureAwait(false);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (Exception e)
                {
                    Log.Exception(e);
                    break;
                }
            }
        }

        async void ReceiveLoop(Connection conn)
        {
            while (true)
            {
                try
                {
                    var msg = conn.ReceiveBuffer.Next();
                    var success = await msg.ReadFromStream(conn.NetworkStream, conn.Token).ConfigureAwait(false);

                    if (!success)
                    {
                        break;
                    }

                    Log.Verbose?.Invoke($"received {msg.Size} bytes from connection {conn.Id}.");

                    _eventQueue.Enqueue(new ServerEvent
                    {
                        type = EventType.DataReceived,
                        connId = conn.Id,
                        data = msg.Payload
                    });
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception e)
                {
                    Log.Exception(e);
                    break;
                }
            }

            DisconnectInternal(conn);
        }

        async void SendInternal(Connection conn, ArraySegment<byte> data)
        {
            try
            {
                // conn.SendBuffer.Next() doesnt need to be thread safe here because
                // async wont start until the first await in the line below it
                // and Send/Broadcast should only be called from the main thread

                var msg = conn.SendBuffer.Next();
                var success = await msg.WriteToStream(conn.NetworkStream, data, conn.Token).ConfigureAwait(false);

                if (!success)
                {
                    DisconnectInternal(conn);
                    return;
                }

                Log.Verbose?.Invoke($"sent {msg.Size} bytes to connection {conn.Id}.");
            }
            catch (ObjectDisposedException)
            {
                DisconnectInternal(conn);
            }
            catch (TaskCanceledException)
            {
                DisconnectInternal(conn);
            }
            catch (OperationCanceledException)
            {
                DisconnectInternal(conn);
            }
            catch (Exception e)
            {
                Log.Exception(e); 
                DisconnectInternal(conn);
            }
        }

        void DisconnectInternal(Connection conn)
        {
            if (!conn.IsActive) return;

            _connections.TryRemove(conn.Id, out _);

            Log.Info?.Invoke($"removed connection {conn.Id}:{conn.IpPort}.");

            _eventQueue.Enqueue(new ServerEvent
            {
                type = EventType.Disconnected,
                connId = conn.Id
            });

            try
            {
                conn.Reset();
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }
    }
}