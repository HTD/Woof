using System;

namespace Woof.Schedules {
    
    public interface IScheduledUpdate {

        string Name { get; set; }

        Schedule Schedule { get; set; }

        IScheduledModule Module { get; set; }

        event EventHandler Exception;

        void Define(string name, string interval, IScheduledModule module);

    }

}