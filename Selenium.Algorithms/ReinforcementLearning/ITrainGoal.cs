﻿namespace Selenium.Algorithms.ReinforcementLearning
{
    using Selenium.Algorithms.ReinforcementLearning.Serialization;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;

    [JsonConverter(typeof(TrainGoalConverterFactory))]
    public interface ITrainGoal<TData>
    {
        Task<bool> HasReachedAGoalCondition(IState<TData> state);

        public async Task<double> RewardFunction(IState<TData> stateFrom, IAgentAction<TData> action, IState<TData> stateTo)
        {
            if (await HasReachedAGoalCondition(stateTo))
            {
                return 100;
            }

            return -1;
        }
    }
}
