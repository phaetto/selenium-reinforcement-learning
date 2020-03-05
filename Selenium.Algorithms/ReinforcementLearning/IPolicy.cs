namespace Selenium.Algorithms.ReinforcementLearning
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IPolicy<TData>
    {
        public IDictionary<StateAndActionPair<TData>, double> QualityMatrix { get; }
        public abstract Task<IAgentAction<TData>> GetNextAction(IEnvironment<TData> environment, IState<TData> state);
    }
}
