using System;
using System.Text.RegularExpressions;

namespace Woof.Schedules {
    
    /// <summary>
    /// A timer which generates events at specified interval or daily at specified time
    /// </summary>
    public class Schedule : IDisposable {

        #region Private

        /// <summary>
        /// Daily time schedule
        /// </summary>
        private class Daily {

            /// <summary>
            /// ISO8601 date format
            /// </summary>
            const string IsoDate = "yyyy-MM-dd";

            /// <summary>
            /// Time of day set in seconds
            /// </summary>
            private readonly int TODSet;

            /// <summary>
            /// ISO date of the last positive check result
            /// </summary>
            private string LastPositiveDate;

            /// <summary>
            /// Daily schedule constructor
            /// </summary>
            /// <param name="time">start time in HH:mm format</param>
            public Daily(string time) {
                TODSet = (int)DateTime.Parse(time).TimeOfDay.TotalSeconds;
            }

            /// <summary>
            /// Checks if current time meets daily schedule
            /// - if called first BEFORE scheduled time (too early) : returns false
            /// - if called first AFTER scheduled time (too late) : returns true
            /// - if called subsequently AFTER scheduled time : returns false
            /// </summary>
            /// <returns></returns>
            public bool Check() {
                string currentDate = DateTime.Now.ToString(IsoDate);
                int currentTOD = (int)DateTime.Now.TimeOfDay.TotalSeconds;
                if (currentDate != LastPositiveDate && (currentTOD > TODSet)) {
                    LastPositiveDate = currentDate;
                    return true;
                }
                return false;
            }

            /// <summary>
            /// Disarm signalling for today
            /// </summary>
            public void Disarm() {
                string currentDate = DateTime.Now.ToString(IsoDate);
                LastPositiveDate = currentDate;
            }

            /// <summary>
            /// Enable Check() method to return true again within the same day
            /// </summary>
            public void Rearm() {
                LastPositiveDate = null;
            }

        }

        private const double DailyChecksEveryMs = 1000;

        private static Regex RxTimeInMilliseconds = new Regex(@"^\d+$", RegexOptions.Compiled);
        private static Regex RxTimeInSeconds = new Regex(@"^(\d+)(?:s|sec)\.?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static Regex RxTimeInMinutes = new Regex(@"^(\d+)(?:m|min)\.?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static Regex RxTimeInHours = new Regex(@"^(\d+)h\.?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static Regex RxTime = new Regex(@"^\d\d:\d\d$", RegexOptions.Compiled);
        private System.Timers.Timer Timer;
        private Daily DailySchedule;


        private Daily GetDailySchedule(string interval) {
            Match m = RxTime.Match(interval);
            if (m.Success) return new Daily(interval);
            return null;
        }

        private int GetMillisecondInterval(string interval) {
            int ms;
            Match m = RxTimeInMilliseconds.Match(interval);
            if (m.Success) ms = int.Parse(interval);
            else {
                m = RxTimeInSeconds.Match(interval);
                if (m.Success) ms = int.Parse(m.Groups[1].Value) * 1000;
                else {
                    m = RxTimeInMinutes.Match(interval);
                    if (m.Success) ms = int.Parse(m.Groups[1].Value) * 60000;
                    else {
                        m = RxTimeInHours.Match(interval);
                        if (m.Success) ms = int.Parse(m.Groups[1].Value) * 3600000;
                        else throw new InvalidOperationException("Invalid time format");
                    }
                }
            }
            return ms;
        }

        private void OnceADay(object sender, System.Timers.ElapsedEventArgs e) {
            if (DailySchedule.Check() && Tick != null) Tick.Invoke(this, EventArgs.Empty);
        }

        private void Recurring(object sender, System.Timers.ElapsedEventArgs e) {
            if (Tick != null) Tick.Invoke(this, EventArgs.Empty);
        }

        #endregion

        public event EventHandler Tick;
        public bool Enabled { get { return Timer.Enabled; } set { Timer.Enabled = value; } }
        public bool IsDaily { get { return DailySchedule != null; } }

        public Schedule(string interval) {
            interval = interval.Trim();
            DailySchedule = GetDailySchedule(interval);
            if (DailySchedule != null) {
                DailySchedule.Disarm();
                Timer = new System.Timers.Timer(DailyChecksEveryMs);
                Timer.Elapsed += OnceADay;
            } else {
                Timer = new System.Timers.Timer(GetMillisecondInterval(interval));
                Timer.Elapsed += Recurring;
            }
        }

        public void Start() {
            Timer.Start();
        }

        public void Stop() {
            Timer.Stop();
        }

        public void Dispose() {
            Timer.Dispose();
        }

    }

}