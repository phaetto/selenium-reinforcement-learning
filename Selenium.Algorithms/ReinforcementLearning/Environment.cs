namespace Selenium.Algorithms.ReinforcementLearning
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public abstract class Environment<TData>
    {
        public abstract Task<State<TData>> GetInitialState();
        public abstract Task<IEnumerable<AgentAction<TData>>> GetPossibleActions(State<TData> state);
        public abstract Task<bool> IsIntermediateState(State<TData> state);
        public abstract Task<State<TData>> GetCurrentState();
        public abstract Task WaitForPostActionIntermediateStabilization();
    }
}
