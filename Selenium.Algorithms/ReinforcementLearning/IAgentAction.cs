using System.Threading.Tasks;

namespace Selenium.Algorithms.ReinforcementLearning
{
    public interface IAgentAction<TData>
    {
        public Task<IState<TData>> ExecuteAction(IEnvironment<TData> environment, IState<TData> state);
        public bool Equals(object obj);
        public int GetHashCode();
        public string ToString();
    }
}
