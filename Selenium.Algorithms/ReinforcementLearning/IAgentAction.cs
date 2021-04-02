namespace Selenium.Algorithms.ReinforcementLearning
{
    using System.Threading.Tasks;

    public interface IAgentAction<TData>
    {
        public Task<IState<TData>> ExecuteAction(IEnvironment<TData> environment, IState<TData> state);
        public bool Equals(object obj);
        public int GetHashCode();
        public string ToString();
    }
}
