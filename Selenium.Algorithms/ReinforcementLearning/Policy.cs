namespace Selenium.Algorithms.ReinforcementLearning
{
    using System;
    using System.Threading.Tasks;

    public abstract class Policy<TData>
    {
        public abstract Task<AgentAction<TData>> GetNextAction(Environment<TData> environment, State<TData> state);
    }
}
