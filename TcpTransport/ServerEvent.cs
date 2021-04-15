using System;

namespace Phuntasia.Networking.Transport
{
    public struct ServerEvent
    {
        public int connId;
        public EventType type;
        public ArraySegment<byte> data;
    }
}