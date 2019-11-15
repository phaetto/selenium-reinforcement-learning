namespace Selenium.Algorithms.ReinforcementLearning
{
    using System.Collections.Generic;

    public abstract class Environment<TData>
    {
        public abstract State<TData> GetInitialState();
        public abstract double RewardFunction(in State<TData> stateFrom, in AgentAction<TData> action);
        public abstract IEnumerable<AgentAction<TData>> GetPossibleActions(in State<TData> state);
        public abstract bool HasReachedAGoalState(in State<TData> state);
    }
}
