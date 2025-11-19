using System.Collections.Generic;
using UnityEngine;

namespace TreasurePlay.SDK.Core
{
    /// <summary>
    /// Centralized logger for the Treasure Play SDK.
    /// </summary>
    internal static class TreasurePlayLogger
    {
        private const string Tag = "[TreasurePlaySDK]";
        private static LogType _minimumLogType = LogType.Log;
        private static readonly Dictionary<LogType, int> Severity = new()
        {
            { LogType.Error, 0 },
            { LogType.Assert, 1 },
            { LogType.Exception, 1 },
            { LogType.Warning, 2 },
            { LogType.Log, 3 },
        };

        internal static void Configure(LogType minimumLogType)
        {
            _minimumLogType = minimumLogType;
        }

        internal static void Log(string message)
        {
            if (!ShouldLog(LogType.Log))
            {
                return;
            }

            Debug.Log($"{Tag} {message}");
        }

        internal static void Warning(string message)
        {
            if (!ShouldLog(LogType.Warning))
            {
                return;
            }

            Debug.LogWarning($"{Tag} {message}");
        }

        internal static void Error(string message)
        {
            if (!ShouldLog(LogType.Error))
            {
                return;
            }

            Debug.LogError($"{Tag} {message}");
        }

        internal static void Exception(System.Exception exception)
        {
            if (!ShouldLog(LogType.Exception))
            {
                return;
            }

            Debug.LogException(exception);
        }
        
        internal static void SdkLog(string message)
        {
            LogWithColor(message, "#9C27B0"); // Purple
        }

        internal static void SessionLog(string message)
        {
            LogWithColor(message, "#2196F3"); // Blue
        }

        internal static void SessionError(string message)
        {
            ErrorWithColor(message, "#2196F3"); // Blue
        }

        internal static void WebViewLog(string message)
        {
            LogWithColor(message, "#4CAF50"); // Green
        }

        internal static void WebViewWarning(string message)
        {
            WarningWithColor(message, "#4CAF50"); // Green
        }

        internal static void WebViewError(string message)
        {
            ErrorWithColor(message, "#4CAF50"); // Green
        }

        internal static void NetworkLog(string message)
        {
            LogWithColor(message, "#FF9800"); // Orange
        }

        internal static void NetworkError(string message)
        {
            ErrorWithColor(message, "#FF9800"); // Orange
        }
        
        private static void LogWithColor(string message, string color)
        {
            if (!ShouldLog(LogType.Log))
            {
                return;
            }

            Debug.Log($"<color={color}>{Tag}</color> {message}");
        }

        private static void WarningWithColor(string message, string color)
        {
            if (!ShouldLog(LogType.Warning))
            {
                return;
            }

            Debug.LogWarning($"<color={color}>{Tag}</color> {message}");
        }

        private static void ErrorWithColor(string message, string color)
        {
            if (!ShouldLog(LogType.Error))
            {
                return;
            }

            Debug.LogError($"<color={color}>{Tag}</color> {message}");
        }

        private static bool ShouldLog(LogType logType)
        {
            var logSeverity = Severity.TryGetValue(logType, out var value) ? value : 3;
            var minimumSeverity = Severity.TryGetValue(_minimumLogType, out var minimum) ? minimum : 3;
            return logSeverity <= minimumSeverity;
        }
    }
}
