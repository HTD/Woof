using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Woof.Schedules {
    
    /// <summary>
    /// Schedules data updates
    /// </summary>
    public class UpdateScheduler<T> : IDisposable where T : IScheduledUpdate, new() {

        /// <summary>
        /// Internal singleton instance
        /// </summary>
        private static UpdateScheduler<T> _Instance;

        /// <summary>
        /// Singleton instance
        /// </summary>
        public static UpdateScheduler<T> Instance { get { return _Instance ?? (_Instance = new UpdateScheduler<T>()); } }

        /// <summary>
        /// A list of scheduled updates
        /// </summary>
        public List<T> ScheduledUpdates = new List<T>();

        /// <summary>
        /// Adds new scheduled update module
        /// </summary>
        /// <param name="name"></param>
        /// <param name="interval"></param>
        /// <param name="module"></param>
        public void Add(string name, string interval, IScheduledModule module) {
            var update = new T();
            update.Define(name, interval, module);
            ScheduledUpdates.Add(update);
        }

        public T GetUpdateByName(string name) {
            return ScheduledUpdates.FirstOrDefault(i => i.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>
        /// Starts all or specified schedule
        /// </summary>
        /// <param name="name"></param>
        public void Start(string name = null) {
            new Task(new Action(() => {
                Update(name);
                if (name == null) ScheduledUpdates.ForEach(i => i.Schedule.Start());
                else GetUpdateByName(name).Schedule.Start();
            })).Start();
        }

        /// <summary>
        /// Stops all or specified schedule
        /// </summary>
        /// <param name="name"></param>
        public void Stop(string name = null) {
            if (name == null) ScheduledUpdates.ForEach(i => i.Schedule.Stop());
            else GetUpdateByName(name).Schedule.Stop();
        }

        /// <summary>
        /// Updates all or specified module
        /// </summary>
        /// <param name="name"></param>
        public void Update(string name = null) {
            if (name == null) ScheduledUpdates.ForEach(i => i.Module.Update());
            else GetUpdateByName(name).Module.Update();
        }

        /// <summary>
        /// Disposes disposable objects
        /// </summary>
        public void Dispose() {
            ScheduledUpdates.ForEach(i => {
                if (i.Schedule != null) {
                    i.Schedule.Stop();
                    i.Schedule.Dispose();
                    i.Schedule = null;
                    if (i.Module is IDisposable) (i.Module as IDisposable).Dispose();
                    i.Module = null;
                }
            });
            ScheduledUpdates.Clear();
        }

    }

}