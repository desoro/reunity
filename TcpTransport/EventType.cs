using System;

namespace Phuntasia.Networking.Transport
{
    public enum EventType : byte
    {
        Connected = 1,
        DataReceived,
        Disconnected
    }
}