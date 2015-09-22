using System;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceProcess;
using System.Timers;

namespace Woof.ServiceModel {

    /// <summary>
    /// Service scheduler module
    /// </summary>
    public class WinService : ServiceBase {

        protected WebServiceHost Host;

        #region Private fields

        /// <summary>
        /// Timer used to keep service instance alive
        /// </summary>
        Timer ZapTimer;

        #endregion

        #region Public fields

        public static WinService Instance;

        #endregion

        /// <summary>
        /// Service class constructor
        /// </summary>
        public WinService() {
            Log.EventLog = EventLog;
            EventLog.Log = Config.Company;
            EventLog.Source = Config.DisplayName;
            ServiceName = Config.ServiceName;
            CanPauseAndContinue = false;
            CanShutdown = true;
            ZapTimer = new Timer(179000);
            ZapTimer.Elapsed += ZapTimer_Elapsed;
            Instance = this;
        }

        /// <summary>
        /// Zaps service instance (hopefully)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ZapTimer_Elapsed(object sender, ElapsedEventArgs e) {
            GC.KeepAlive(Host.SingletonInstance);
        }

        /// <summary>
        /// Service START
        /// </summary>
        /// <param name="args">dodatkowe parametry</param>
        protected override void OnStart(string[] args) {
            if (Config.StartAction != null) {
                try {
                    Config.StartAction.Invoke();
                    if (Host != null) Host.Close();
                } catch (Exception x) { Log.Exception(x); }
            }
            Host = new WebServiceHost(Config.Type, new Uri(Config.Uri));
            ZapTimer.Start();
            try {
                Host.Open();
            } catch (AddressAccessDeniedException) {
                throw new AddressAccessDeniedException("Insuficient privileges to open WebServiceHost.");
            }
        }

        /// <summary>
        /// Service STOP
        /// </summary>
        protected override void OnStop() {
            if (Config.StopAction != null) Config.StopAction.Invoke();
            if (Host != null) {
                GC.KeepAlive(Host.SingletonInstance);
                GC.KeepAlive(Host);
                ZapTimer.Stop();
                Host.Close();
                Host = null;
            }
        }

        /// <summary>
        /// Service SHUTDOWN
        /// </summary>
        protected override void OnShutdown() {
            OnStop();
            base.OnShutdown();
        }

        /// <summary>
        /// Public access to service START
        /// </summary>
        public void Start() { OnStart(null); }

        /// <summary>
        /// Public access to service SHUTDOWN
        /// </summary>
        public void Shutdown() { OnShutdown(); }

    }

    /// <summary>
    /// Static service EventLog - accessible for both WCF and WindowsService instance
    /// </summary>
    public static class Log {

        /// <summary>
        /// System event definition
        /// </summary>
        public class EventDefinition {
            /// <summary>
            /// Event type
            /// </summary>
            public EventLogEntryType Type { get; set; }
            /// <summary>
            /// Event identifier
            /// </summary>
            public int Id { get; set; }
            /// <summary>
            /// Event message
            /// </summary>
            public string Message { get; set; }
        }

        /// <summary>
        /// Target log - must be set to EventLog object before use
        /// </summary>
        public static EventLog EventLog;

        /// <summary>
        /// Last system event message text
        /// </summary>
        private static string LastLogMessage;

        /// <summary>
        /// Lock to allow console messages synchronization
        /// </summary>
        private static object ConsoleLock = new Object();

        /// <summary>
        /// Lock to prevent from multiple access to system log
        /// </summary>
        private static object EventLogLock = new Object();

        /// <summary>
        /// Writes event log with spam protection
        /// </summary>
        /// <param name="ev">event object</param>
        /// <param name="data">optional data to format message text</param>
        public static void Event(EventDefinition ev, object data = null) {
            lock (EventLogLock) {
                if (EventLog == null) EventLog = new EventLog(Config.Company);
                var msg = data == null ? ev.Message : String.Format(ev.Message, data);
#if NOSPAM
                if (msg != LastLogMessage) {
#endif
                LastLogMessage = msg;
                if (Config.TestMode) {
                    lock (ConsoleLock) {
                        switch (ev.Type) {
                            case EventLogEntryType.Information: Console.Write("II: "); break;
                            case EventLogEntryType.Warning: Console.Write("WW: "); break;
                            case EventLogEntryType.Error: Console.Write("EE: "); break;
                        }
                        Console.WriteLine(String.Format("({0}) {1}", ev.Id, msg));
                    }
                } else EventLog.WriteEntry(msg, ev.Type, ev.Id);
#if NOSPAM
                }
#endif
            }
        }

        public static void Exception(Exception x, int id = 3666) {
            Event(new Log.EventDefinition {
                Id = id,
                Type = EventLogEntryType.Error,
                Message = String.Format(EventMessages.Exception, "0x" + x.HResult.ToString("X"), x.Message, x.StackTrace)
            });
        }

    }

}