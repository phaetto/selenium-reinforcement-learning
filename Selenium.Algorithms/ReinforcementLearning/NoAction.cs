namespace Selenium.Algorithms.ReinforcementLearning
{
    using System.Threading.Tasks;

    public sealed class NoAction<TData> : IAgentAction<TData>
    {
        public Task<IState<TData>> ExecuteAction(IEnvironment<TData> environment, IState<TData> state)
        {
            throw new System.NotSupportedException();
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public override string ToString()
        {
            return nameof(NoAction<TData>);
        }
    }
}
