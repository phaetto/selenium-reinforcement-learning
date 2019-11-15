namespace Selenium.Algorithms.ReinforcementLearning
{
    public abstract class Policy<TData>
    {
        public abstract AgentAction<TData> GetNextAction(in Environment<TData> environment, in State<TData> state);
    }
}
