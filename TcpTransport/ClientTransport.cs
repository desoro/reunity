using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Phuntasia.Networking.Transport
{
    public class ClientTransport
    {
        static readonly LogChannel Log = LogManager.GetLog<ClientTransport>();

        public bool NoDelay { get; set; } = false;
        public int SendBufferSize { get; set; } = 16;
        public int ReceiveBufferSize { get; set; } = 16;
        public bool IsActive => _isConnected;  

        MessageBuffer _sendBuffer;
        MessageBuffer _receiveBuffer;
        TcpClient _client;
        NetworkStream _stream;
        CancellationTokenSource _tokenSource;
        CancellationToken _token;
        ConcurrentQueue<ClientEvent> _eventQueue;
        bool _isPending;
        bool _isConnected;

        public ClientTransport()
        {
            _eventQueue = new ConcurrentQueue<ClientEvent>();
        }

        public void Start(string host, int port)
        {
            if (_isPending) throw new InvalidOperationException("Client connection is pending.");
            if (_isConnected) throw new InvalidOperationException("Client is already connected.");

            if (_sendBuffer == null)
            {
                _sendBuffer = new MessageBuffer(SendBufferSize);
                _receiveBuffer = new MessageBuffer(ReceiveBufferSize);
            }

            _isPending = true;

            _client = new TcpClient();
            _client.NoDelay = NoDelay;

            _tokenSource = new CancellationTokenSource();
            _token = _tokenSource.Token;
            _eventQueue = new ConcurrentQueue<ClientEvent>();

            _ = Task.Run(() => ConnectInternal(host, port), _token);
        }

        public void Stop()
        {
            if (_isPending || _isConnected)
            {
                DisconnectInternal();
            }            
        }

        public void Send(ArraySegment<byte> data)
        {
            if (!_isConnected) throw new InvalidOperationException("Client is not connected.");

            SendInternal(data);
        }

        public bool TryNextEvent(out ClientEvent evnt)
        {
            return _eventQueue.TryDequeue(out evnt);
        }

        async void ConnectInternal(string host, int port)
        {
            try
            {
                await _client.ConnectAsync(host, port).ConfigureAwait(false);
            }
            catch
            {
                _isPending = false;
                Log.Warn?.Invoke("failed to connect to the server.");
                return;
            }

            _isConnected = true;
            _isPending = false;

            _stream = _client.GetStream();

            Log.Info?.Invoke($"connected.");

            _eventQueue.Enqueue(new ClientEvent
            {
                type = EventType.Connected
            });

            _ = Task.Run(() => ReceiveLoop(), _token);
        }

        async void ReceiveLoop()
        {
            while (true)
            {
                try
                {
                    var msg = _receiveBuffer.Next();
                    var success = await msg.ReadFromStream(_stream, _token).ConfigureAwait(false);

                    if (!success)
                    {
                        break;
                    }

                    Log.Verbose?.Invoke($"received {msg.Size} bytes.");

                    _eventQueue.Enqueue(new ClientEvent
                    {
                        type = EventType.DataReceived,
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

            DisconnectInternal();
        }

        async void SendInternal(ArraySegment<byte> data)
        {
            try
            {
                // _sendBuffer.Next() doesnt need to be thread safe here because
                // async wont start until the first await in the line below it
                // and Send should only be called from the main thread

                var msg =_sendBuffer.Next();
                var success = await msg.WriteToStream(_stream, data, _token).ConfigureAwait(false);

                if (!success)
                {
                    DisconnectInternal();
                    return;
                }

                Log.Verbose?.Invoke($"sent {msg.Size} bytes.");
            }
            catch (ObjectDisposedException)
            {
                DisconnectInternal();
            }
            catch (TaskCanceledException)
            {
                DisconnectInternal();
            }
            catch (OperationCanceledException)
            {
                DisconnectInternal();
            }
            catch (Exception e)
            {
                Log.Exception(e);
                DisconnectInternal();
            }
        }

        void DisconnectInternal()
        {
            try
            {
                if (_tokenSource != null)
                {
                    if (!_tokenSource.IsCancellationRequested)
                    {
                        _tokenSource.Cancel();
                        _tokenSource.Dispose();
                    }
                }

                if (_stream != null)
                {
                    _stream.Close();
                }

                if (_client != null)
                {
                    _client.Close();
                    _client.Dispose();
                }
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }

            if (_isConnected)
            {
                Log.Info?.Invoke($"disconnected.");

                _eventQueue.Enqueue(new ClientEvent
                {
                    type = EventType.Disconnected
                });
            }

            _isPending = false;
            _isConnected = false;
        }
    }
}