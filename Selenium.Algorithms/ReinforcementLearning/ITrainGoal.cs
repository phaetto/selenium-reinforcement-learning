namespace Selenium.Algorithms.ReinforcementLearning
{
    using System.Threading.Tasks;

    public interface ITrainGoal<TData>
    {
        public int TimesReachedGoal { get; set; }
        Task<double> RewardFunction(State<TData> stateFrom, AgentAction<TData> action);
        Task<bool> HasReachedAGoalCondition(State<TData> state, AgentAction<TData> action);
    }
}
