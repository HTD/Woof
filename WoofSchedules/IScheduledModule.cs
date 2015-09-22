namespace Woof.Schedules {
    
    /// <summary>
    /// An interface of an update module which can be scheduled
    /// </summary>
    public interface IScheduledModule {
        
        /// <summary>
        /// Module update
        /// </summary>
        void Update();
    
    }

}