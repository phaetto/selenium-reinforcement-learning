using System.Threading.Tasks;

namespace Selenium.Algorithms.ReinforcementLearning
{
    public abstract class AgentAction<TData>
    {
        public abstract Task<State<TData>> ExecuteAction(Environment<TData> environment, State<TData> state);
        public abstract override bool Equals(object obj);
        public abstract override int GetHashCode();
        public abstract override string ToString();
    }
}
