namespace Selenium.Algorithms.ReinforcementLearning
{
    using System.Threading.Tasks;

    public interface IPolicy<TData>
    {
        public abstract Task<IAgentAction<TData>> GetNextAction(IEnvironment<TData> environment, IState<TData> state, IExperimentState<TData> experimentState);
    }
}
