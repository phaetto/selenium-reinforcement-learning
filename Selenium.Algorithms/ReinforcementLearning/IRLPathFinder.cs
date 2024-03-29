﻿namespace Selenium.Algorithms.ReinforcementLearning
{
    using System.Threading.Tasks;

    public interface IRLPathFinder<TData>
    {
        Task<WalkResult<TData>> FindRoute(IState<TData> start, ITrainGoal<TData> trainGoal, int maxSteps = 10, IRLParameter<TData>[]? parameters = null);
        Task<WalkResult<TData>> FindRouteWithoutApplyingActions(IState<TData> start, ITrainGoal<TData> trainGoal, int maxSteps = 10);
    }
}