using System;

namespace Phuntasia
{
    public class LogChannel
    {
        readonly Type _context;
        LogLevel _level;

        public Action<object> Verbose { get; private set; }
        public Action<object> Info { get; private set; }
        public Action<object> Warn { get; private set; }
        public Action<object> Error { get; private set; }
        public Action<Exception> Exception { get; private set; }

        public LogLevel Level
        {
            get => _level;
            set => SetLevel(value);
        }

        public LogChannel(Type context, LogLevel level)
        {
            _context = context;

            Level = level;

            Error = (x) => LogManager.Error($"{_context.Name}: {x}");
            Exception = (x) => LogManager.Exception(x);
        }

        void SetLevel(LogLevel level)
        {
            _level = level;

            if (_level <= LogLevel.Verbose)
                Verbose = (x) => LogManager.Verbose($"{_context.Name}: {x}");
            else
                Verbose = null;

            if (_level <= LogLevel.Info)
                Info = (x) => LogManager.Info($"{_context.Name}: {x}");
            else
                Info = null;

            if (_level <= LogLevel.Warn)
                Warn = (x) => LogManager.Warn($"{_context.Name}: {x}");
            else
                Warn = null;
        }
    }
}