using OpenQA.Selenium.Remote;
using Selenium.Algorithms.ReinforcementLearning;
using System;
using System.Threading.Tasks;

namespace Selenium.Algorithms
{
    public class SeleniumTrainGoal<TData> : ITrainGoal<TData>
    {
        private readonly Func<State<TData>, AgentAction<TData>, Task<bool>> hasReachedAGoalConditionPredicate;

        public int TimesReachedGoal { get; set; }

        public SeleniumTrainGoal(Func<State<TData>, AgentAction<TData>, Task<bool>> hasReachedAGoalConditionPredicate)
        {
            this.hasReachedAGoalConditionPredicate = hasReachedAGoalConditionPredicate;
        }

        public async Task<double> RewardFunction(State<TData> stateFrom, AgentAction<TData> action)
        {
            if (await HasReachedAGoalCondition(stateFrom, action))
            {
                return 100;
            }

            return -1;
        }

        public async Task<bool> HasReachedAGoalCondition(State<TData> state, AgentAction<TData> action)
        {
            return await hasReachedAGoalConditionPredicate.Invoke(state, action);
        }
    }
}
