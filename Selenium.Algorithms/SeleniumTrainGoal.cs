namespace Selenium.Algorithms
{
    using Selenium.Algorithms.ReinforcementLearning;
    using System;
    using System.Threading.Tasks;

    public sealed class SeleniumTrainGoal<TData> : ITrainGoal<TData>
    {
        private readonly Func<IState<TData>, IAgentAction<TData>, Task<bool>> hasReachedAGoalConditionPredicate;

        public SeleniumTrainGoal(Func<IState<TData>, IAgentAction<TData>, Task<bool>> hasReachedAGoalConditionPredicate)
        {
            this.hasReachedAGoalConditionPredicate = hasReachedAGoalConditionPredicate;
        }

        public async Task<double> RewardFunction(IState<TData> stateFrom, IAgentAction<TData> action)
        {
            if (await HasReachedAGoalCondition(stateFrom, action))
            {
                return 100;
            }

            return -1;
        }

        public async Task<bool> HasReachedAGoalCondition(IState<TData> state, IAgentAction<TData> action)
        {
            return await hasReachedAGoalConditionPredicate.Invoke(state, action);
        }
    }
}
