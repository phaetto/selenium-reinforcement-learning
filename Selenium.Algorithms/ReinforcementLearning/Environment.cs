namespace Selenium.Algorithms.ReinforcementLearning
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public abstract class Environment<TData>
    {
        public abstract Task<State<TData>> GetInitialState();
        public abstract Task<double> RewardFunction(State<TData> stateFrom, AgentAction<TData> action);
        public abstract Task<IEnumerable<AgentAction<TData>>> GetPossibleActions(State<TData> state);
        public abstract Task<bool> HasReachedAGoalCondition(State<TData> state, AgentAction<TData> action);
    }
}
