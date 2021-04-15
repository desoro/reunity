using System;

namespace Phuntasia.Networking.Transport
{
    class MessageBuffer
    {
        readonly Message[] _buffer;
        int _index;

        public MessageBuffer(int size)
        {
            _buffer = new Message[size];
            
            for (int i = 0; i < size; i++)
            {
                _buffer[i] = new Message();
            }
        }

        public Message Next()
        {
            if (_index == _buffer.Length)
            {
                _index = 0;
            }

            var value = _buffer[_index++];

            return value;
        }
    }
}