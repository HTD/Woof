using System;

namespace Woof.Schedules {
    
    /// <summary>
    /// Scheduled update object
    /// </summary>
    public class ScheduledUpdateBase : IScheduledUpdate {

        /// <summary>
        /// Scheduled update name
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Bound schedule instance
        /// </summary>
        public Schedule Schedule { get; set; }
        
        /// <summary>
        /// Bound module instance
        /// </summary>
        public IScheduledModule Module { get; set; }
        
        /// <summary>
        /// Allows custom exception handling - Exception object sent as sender
        /// </summary>
        public event EventHandler Exception;

        /// <summary>
        /// Creates scheduled update instance
        /// </summary>
        /// <param name="name"></param>
        /// <param name="interval"></param>
        /// <param name="module"></param>
        public void Define(string name, string interval, IScheduledModule module) {
            Name = name;
            Schedule = new Schedule(interval);
            Schedule.Tick += Schedule_Tick;
            Module = module;
        }

        /// <summary>
        /// Executed each interval
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Schedule_Tick(object sender, EventArgs e) {
            try {
                var recurrent = !Schedule.IsDaily;
                if (recurrent) Schedule.Stop();
                Module.Update();
                if (recurrent) Schedule.Start();
            } catch (Exception x) { this.Exception.Invoke(x, EventArgs.Empty); }
        }

    }

}