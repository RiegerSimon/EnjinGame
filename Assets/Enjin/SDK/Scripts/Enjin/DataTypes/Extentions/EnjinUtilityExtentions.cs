namespace EnjinSDK
{
    using UnityEngine;

    public enum LoggerType { DEBUG_INFO, INFO, GRAPHQL_REQUEST, GRAPHQL_RESPONSE, ERROR, WARNING, EVENT }
    public enum LoggerLevel { NORMAL, ERROR_ONLY, FULL, REQUEST_ONLY, NO_WARNING}

    public static class EnjinLogger
    {
        public static LoggerType LogType;

        private static LoggerLevel _logLevel = LoggerLevel.FULL;    // Should be set to normal after testing etc.
        private static bool _isDebugOn;

        public static void SetDebugActive(bool active) { _isDebugOn = active; }
        public static void SetLogLevel(LoggerLevel level) { _logLevel = level; }
        public static void SetToDefault()
        {
            _isDebugOn = false;
            _logLevel = LoggerLevel.NORMAL;
        }

        public static void ConsoleReporter(LoggerType type, string message)
        {
            if (message.Contains("password") && !_isDebugOn)
                return;

            switch (type)
            {
                case LoggerType.DEBUG_INFO:
                    if (_logLevel != LoggerLevel.NORMAL || _logLevel != LoggerLevel.ERROR_ONLY)
                        Debug.Log("<color=lime>[DEBUG_INFO]</color> " + message);
                    break;

                case LoggerType.ERROR:
                    Debug.Log("<color=red>[ERROR]</color> " + message);
                    break;

                case LoggerType.GRAPHQL_REQUEST:
                case LoggerType.GRAPHQL_RESPONSE:
                    if (_logLevel != LoggerLevel.NORMAL || _logLevel != LoggerLevel.ERROR_ONLY)
                        Debug.Log("<color=orange>[" + type.ToString() + "]</color> " + message);
                    break;
                case LoggerType.INFO:
                    if (_logLevel != LoggerLevel.ERROR_ONLY || _logLevel != LoggerLevel.REQUEST_ONLY)
                        Debug.Log("<color=grey>[" + type.ToString() + "]</color> " + message);
                    break;

                case LoggerType.WARNING:
                    if (_logLevel == LoggerLevel.NORMAL || _logLevel == LoggerLevel.FULL)
                        Debug.Log("<color=yellow>[WARNING]</color> " + message);
                    break;

                case LoggerType.EVENT:
                    if (_logLevel == LoggerLevel.FULL)
                    Debug.Log("<color=aqua>[PUSHER]</color> " + message);
                    break;
            }
        }
    }
}