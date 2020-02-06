namespace Selenium.Algorithms.ReinforcementLearning
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public abstract class Environment<TData>
    {
        /// <summary>
        /// Resets the environment and retrieves the initial state
        /// </summary>
        /// <returns>The initial state</returns>
        public abstract Task<State<TData>> GetInitialState();
        /// <summary>
        /// Retrieves all the possible actions for that state
        /// </summary>
        /// <param name="state">The state in question</param>
        /// <returns>All the actions that can be applied</returns>
        public abstract Task<IEnumerable<AgentAction<TData>>> GetPossibleActions(State<TData> state);
        /// <summary>
        /// Checks if the state is an indermediate state
        /// </summary>
        /// <remarks>
        /// This is an important method for systems that might have delayed response to actions.
        /// </remarks>
        /// <param name="state">The state checked</param>
        /// <returns>True if intermediate, false otherwise</returns>
        public abstract Task<bool> IsIntermediateState(State<TData> state);
        /// <summary>
        /// Retrieves the current state of the environment
        /// </summary>
        /// <returns>The environment state</returns>
        public abstract Task<State<TData>> GetCurrentState();
        /// <summary>
        /// When the state is considered intermediate (exactly after an action has been applied) the algorithm
        /// is going to call this method to wait for the environment stabilization
        /// </summary>
        /// <returns>Returns when the task has finished waiting</returns>
        public abstract Task WaitForPostActionIntermediateStabilization();
    }
}
