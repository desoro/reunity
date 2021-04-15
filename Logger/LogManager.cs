using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Phuntasia
{
    public static class LogManager
    {
        public static Action<object> Verbose = (x) => DefaultVerbose(x);
        public static Action<object> Info = (x) => DefaultInfo(x);
        public static Action<object> Warn = (x) => DefaultWarn(x);
        public static Action<object> Error = (x) => DefaultError(x);
        public static Action<Exception> Exception = (x) => DefaultException(x);


        static readonly Dictionary<Type, LogChannel> _logs;
        static LogLevel _globalLevel;


        static LogManager()
        {
            _logs = new Dictionary<Type, LogChannel>();

#if DEBUG
            _globalLevel = LogLevel.Warn;
#else
            _globalLevel = LogLevel.Error;
#endif
        }


        public static LogChannel GetLog<T>(LogLevel level = 0)
        {
            var type = typeof(T);

#if !DEBUG
            level = 0;
#endif

            if (!_logs.TryGetValue(type, out var log))
            {
                log = new LogChannel(type, level == 0 ? _globalLevel : level);

                _logs[type] = log;
            }

            return log;
        }

        public static IReadOnlyDictionary<Type, LogChannel> GetLogs()
        {
            return _logs;
        }

        [Conditional("DEBUG")]
        public static void SetLevel<T>(LogLevel level)
        {
            if (_logs.TryGetValue(typeof(T), out var log))
            {
                log.Level = level;
            }
        }

        [Conditional("DEBUG")]
        public static void SetLevel(Type type, LogLevel level)
        {
            if (_logs.TryGetValue(type, out var log))
            {
                log.Level = level;
            }
        }

        [Conditional("DEBUG")]
        public static void SetAllLevels(LogLevel level)
        {
            _globalLevel = level;

            foreach (var c in _logs)
            {
                c.Value.Level = level;
            }
        }


        static void DefaultVerbose(object msg)
        {
            lock (Console.Out)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine(msg);
            }
        }

        static void DefaultInfo(object msg)
        {
            lock (Console.Out)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine(msg);
                Console.ResetColor();
            }
        }

        static void DefaultWarn(object msg)
        {
            lock (Console.Out)
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine(msg);
                Console.ResetColor();
            }
        }

        static void DefaultError(object msg)
        {
            lock (Console.Out)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine(msg);
                Console.ResetColor();
            }
        }

        static void DefaultException(Exception e)
        {
            if (e is ExpectedException ee)
            {
                Warn?.Invoke(e.Message);
            }
            else
            {
                lock (Console.Out)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(e);
                    Console.ResetColor();
                }
            }
        }
    }
}