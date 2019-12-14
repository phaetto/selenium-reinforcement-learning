namespace Selenium.Algorithms.ReinforcementLearning
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public abstract class Policy<TData>
    {
        public readonly IDictionary<StateAndActionPairWithResultState<TData>, double> QualityMatrix = new Dictionary<StateAndActionPairWithResultState<TData>, double>();
        public abstract Task<AgentAction<TData>> GetNextAction(Environment<TData> environment, State<TData> state);
    }
}
