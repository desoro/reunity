using System;

namespace Phuntasia.Networking.Transport
{
    public struct ClientEvent
    {
        public EventType type;
        public ArraySegment<byte> data;
    }
}