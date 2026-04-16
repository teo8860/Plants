using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Plants;

public static class CrashLogger
{
    private static readonly object _lock = new();
    private static string _logDirectory = "";
    
    public static void Init(string baseDirectory)
    {
        // Use same path as save files: %APPDATA%/Plants/crashes
        string appDataBase = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        if (string.IsNullOrEmpty(appDataBase))
            appDataBase = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        
        _logDirectory = Path.Combine(appDataBase, "Plants", "crashes");
        
        try
        {
            if (!Directory.Exists(_logDirectory))
                Directory.CreateDirectory(_logDirectory);
            
            LogInfo("CrashLogger initialized", $"Log directory: {_logDirectory}");
        }
        catch (Exception ex)
        {
            _logDirectory = Path.GetTempPath();
            LogInfo("CrashLogger fallback", $"Using temp path: {_logDirectory}, Init error: {ex.Message}");
        }
    }
    
    public static void SetupGlobalHandlers()
    {
        // AppDomain handlers for unhandled exceptions
        AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
        {
            var ex = args.ExceptionObject as Exception;
            LogFatal("Unhandled AppDomain Exception", ex, args.IsTerminating);
            if (args.IsTerminating)
            {
                // Give logger time to flush before termination
                Thread.Sleep(500);
            }
        };
        
        // TaskScheduler for unobserved task exceptions
        TaskScheduler.UnobservedTaskException += (sender, args) =>
        {
            LogError("Unobserved Task Exception", args.Exception);
            args.SetObserved();
        };
        
        // Set up Windows structured exception handling for native crashes
        try
        {
            AppDomain.CurrentDomain.DomainUnload += (s, e) => LogInfo("DomainUnload", "Application domain unloading");
            AppDomain.CurrentDomain.ProcessExit += (s, e) => LogInfo("ProcessExit", "Process exiting with code: " + Environment.ExitCode);
        }
        catch { }
    }
    
    public static void LogInfo(string context, string message)
    {
        WriteLog("INFO", context, message, null);
    }
    
    public static void LogWarning(string context, string message)
    {
        WriteLog("WARN", context, message, null);
    }
    
    public static void LogError(string context, Exception? ex)
    {
        WriteLog("ERROR", context, ex?.Message ?? "null", ex);
    }
    
    public static void LogFatal(string context, Exception? ex, bool isTerminating)
    {
        WriteLog(isTerminating ? "FATAL" : "CRASH", context, ex?.Message ?? "null", ex);
    }
    
    public static void LogGameAction(string action, string details)
    {
        WriteLog("ACTION", action, details, null);
    }
    
    public static void LogMemory(string context)
    {
        try
        {
            var proc = Process.GetCurrentProcess();
            var memMB = proc.WorkingSet64 / (1024.0 * 1024.0);
            var threadCount = proc.Threads.Count;
            WriteLog("MEMORY", context, $"Memory: {memMB:F1}MB, Threads: {threadCount}", null);
        }
        catch
        {
            // Ignore - this is best-effort logging
        }
    }
    
    private static void WriteLog(string level, string context, string message, Exception? ex)
    {
        if (string.IsNullOrEmpty(_logDirectory))
            return;
            
        lock (_lock)
        {
            try
            {
                var timestamp = DateTime.Now;
                var sb = new StringBuilder();
                sb.AppendLine($"[{timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{level}] [{context}]");
                sb.AppendLine($"  Message: {message}");
                
                if (ex != null)
                {
                    sb.AppendLine($"  Exception Type: {ex.GetType().FullName}");
                    sb.AppendLine($"  Stack Trace:");
                    foreach (var line in (ex.StackTrace ?? "No stack trace").Split('\n'))
                    {
                        sb.AppendLine($"    {line.Trim()}");
                    }
                    
                    if (ex.InnerException != null)
                    {
                        sb.AppendLine($"  Inner Exception: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}");
                    }
                }
                
                // Add environment info for crashes
                if (level == "FATAL" || level == "CRASH")
                {
                    sb.AppendLine();
                    sb.AppendLine("Environment:");
                    sb.AppendLine($"  Machine: {Environment.MachineName}");
                    sb.AppendLine($"  OS: {Environment.OSVersion}");
                    sb.AppendLine($"  CLR: {Environment.Version}");
                    sb.AppendLine($"  64-bit: {Environment.Is64BitProcess}");
                    sb.AppendLine($"  TickCount: {Environment.TickCount64}");
                    
                    try
                    {
                        var proc = Process.GetCurrentProcess();
                        sb.AppendLine($"  Process Memory: {proc.WorkingSet64 / (1024.0 * 1024.0):F1}MB");
                        sb.AppendLine($"  Thread Count: {proc.Threads.Count}");
                        sb.AppendLine($"  Start Time: {proc.StartTime}");
                    }
                    catch { }
                    
                    sb.AppendLine($"  GameElements: {GameElement.elementList?.Count ?? -1}");
                }
                
                sb.AppendLine();
                sb.AppendLine(new string('-', 60));
                sb.AppendLine();
                
                var filename = level == "FATAL" || level == "CRASH" 
                    ? $"crash_{timestamp:yyyyMMdd_HHmmss}.log"
                    : $"plants_{timestamp:yyyyMMdd}.log";
                
                var filepath = Path.Combine(_logDirectory, filename);
                File.AppendAllText(filepath, sb.ToString());
                
                // Also write to latest.log for easy access
                var latestPath = Path.Combine(_logDirectory, "latest.log");
                File.AppendAllText(latestPath, sb.ToString());
            }
            catch
            {
                // Last resort - avoid infinite recursion
            }
        }
    }
    
    public static void DumpGameState(string reason)
    {
        try
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Game State Dump - {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"Reason: {reason}");
            sb.AppendLine();
            
            sb.AppendLine("Active Room:");
            try { sb.AppendLine($"  ID: {Room.GetActiveId()}"); } catch { sb.AppendLine("  (error)"); }
            
            sb.AppendLine("GameElements:");
            var elements = GameElement.GetList();
            sb.AppendLine($"  Total: {elements.Count}");
            
            var activeCount = 0;
            foreach (var e in elements)
            {
                if (e.active) activeCount++;
            }
            sb.AppendLine($"  Active: {activeCount}");
            
            var filepath = Path.Combine(_logDirectory, $"state_{DateTime.Now:yyyyMMdd_HHmmss}.dump");
            File.WriteAllText(filepath, sb.ToString());
            
            LogInfo("StateDump", $"Dumped to {filepath}");
        }
        catch
        {
            // Ignore - best effort
        }
    }
}
