namespace Selenium.Algorithms.ReinforcementLearning
{
    using System.Threading.Tasks;

    public interface ITrainGoal<TData>
    {
        public int TimesReachedGoal { get; set; }
        Task<double> RewardFunction(IState<TData> stateFrom, IAgentAction<TData> action);
        Task<bool> HasReachedAGoalCondition(IState<TData> state, IAgentAction<TData> action);
    }
}
