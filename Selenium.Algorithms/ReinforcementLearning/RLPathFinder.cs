namespace Selenium.Algorithms.ReinforcementLearning
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Uses the policy's data to find routes to goals
    /// </summary>
    /// <typeparam name="TData">The prime data for state - exposed for convienience</typeparam>
    public sealed class RLPathFinder<TData> : IRLPathFinder<TData>
    {
        private readonly IEnvironment<TData> environment;
        private readonly IExperimentState<TData> experimentState;

        public RLPathFinder(
            in IEnvironment<TData> environment,
            in IExperimentState<TData> experimentState)
        {
            this.environment = environment;
            this.experimentState = experimentState;
        }

        /// <summary>
        /// Gets the best route from the starting state to the goal state making decisions using the maximum reward path.
        /// Mutates the environment.
        /// </summary>
        /// <param name="start">The starting state</param>
        /// <param name="target">The target state</param>
        /// <param name="maxSteps">Maximum steps that should be taken</param>
        /// <returns>A report data structure that describes what happened while attempting</returns>
        public async Task<WalkResult<TData>> FindRoute(IState<TData> start, ITrainGoal<TData> trainGoal, int maxSteps = 10, IRLParameter<TData>[]? parameters = null)
        {
            var resultStates = new List<StateAndActionPair<TData>>();

            var currentState = start;
            var currentStep = 0;
            while (currentStep < maxSteps)
            {
                var actions = await environment.GetPossibleActions(currentState);
                var stateAndActionPairs = actions
                    .Select(x =>
                    {
                        var pair = new StateAndActionPair<TData>(currentState, x);
                        return experimentState.QualityMatrix.ContainsKey(pair)
                            ? (action: x, score: experimentState.QualityMatrix[pair])
                            : (action: x, score: 0D);
                    })
                    .ToList();

                if (stateAndActionPairs.Count < 1)
                {
                    return new WalkResult<TData>(PathFindResultState.Unreachable);
                }

                var maximumValue = double.MinValue;
                var maximumReturnAction = stateAndActionPairs.First().action;
                foreach (var pair in stateAndActionPairs)
                {
                    if (pair.score > maximumValue)
                    {
                        maximumReturnAction = pair.action;
                        maximumValue = pair.score;
                    }
                }

                IAgentAction<TData> selectedAction = maximumReturnAction;
                if (parameters != null && parameters.Any(x => x.Test(maximumReturnAction)))
                {
                    selectedAction = parameters.First(x => x.Test(maximumReturnAction)).Select(actions);
                }

                var newState = await selectedAction.ExecuteAction(environment, currentState);

                while (await environment.IsIntermediateState(newState) && currentStep < maxSteps)
                {
                    await environment.WaitForPostActionIntermediateStabilization();
                    newState = await environment.GetCurrentState();
                    ++currentStep;

                    if (await trainGoal.HasReachedAGoalCondition(newState))
                    {
                        break;
                    }
                }

                if (currentStep >= maxSteps)
                {
                    return new WalkResult<TData>(PathFindResultState.StepsExhausted, resultStates);
                }

                var newPair = new StateAndActionPairWithResultState<TData>(currentState, maximumReturnAction, newState);

                if (resultStates.Contains(newPair))
                {
                    return new WalkResult<TData>(PathFindResultState.LoopDetected, resultStates);
                }

                resultStates.Add(newPair);
                currentState = newState;

                if (await trainGoal.HasReachedAGoalCondition(currentState))
                {
                    return new WalkResult<TData>(PathFindResultState.GoalReached, resultStates);
                }

                ++currentStep;
            }

            return new WalkResult<TData>(PathFindResultState.StepsExhausted, resultStates);
        }

        /// <summary>
        /// Uses the matrix  to try and find the optimal route without executing any action.
        /// This method does not mutate the environment.
        /// </summary>
        /// <param name="start">The starting state</param>
        /// <param name="target">The target state</param>
        /// <param name="maxSteps">Maximum steps that should be taken</param>
        /// <returns>A report data structure that describes what happened while attempting</returns>
        public async Task<WalkResult<TData>> FindRouteWithoutApplyingActions(IState<TData> start, ITrainGoal<TData> trainGoal, int maxSteps = 10)
        {
            var resultStates = new List<StateAndActionPair<TData>>();

            var currentState = start;
            var currentStep = 0;
            while (currentStep < maxSteps)
            {
                var actions = await environment.GetPossibleActions(currentState);
                var stateAndActionPairs = actions
                   .Select(x =>
                   {
                       var pair = new StateAndActionPair<TData>(currentState, x);
                       return experimentState.QualityMatrix.ContainsKey(pair)
                           ? (pair: experimentState.QualityMatrix.Keys.First(y => y.Equals(pair)), score: experimentState.QualityMatrix[pair])
                           : (pair, score: 0D);
                   })
                   .ToList();

                if (stateAndActionPairs.Count < 1)
                {
                    return new WalkResult<TData>(PathFindResultState.Unreachable);
                }

                var maximumValue = double.MinValue;
                var maximumReturnPair = stateAndActionPairs.First().pair;
                foreach (var pairAndScore in stateAndActionPairs)
                {
                    if (pairAndScore.score > maximumValue)
                    {
                        maximumReturnPair = pairAndScore.pair;
                        maximumValue = pairAndScore.score;
                    }
                }

                if (maximumReturnPair is StateAndActionPairWithResultState<TData> stateAndActionPairWithResultState)
                {
                    var newState = stateAndActionPairWithResultState.ResultState;

                    if (resultStates.Contains(stateAndActionPairWithResultState))
                    {
                        return new WalkResult<TData>(PathFindResultState.LoopDetected, resultStates);
                    }

                    resultStates.Add(stateAndActionPairWithResultState);
                    currentState = newState;

                    if (await trainGoal.HasReachedAGoalCondition(currentState))
                    {
                        return new WalkResult<TData>(PathFindResultState.GoalReached, resultStates);
                    }
                }
                else
                {
                    return new WalkResult<TData>(PathFindResultState.DataNotIncluded);
                }

                ++currentStep;
            }

            return new WalkResult<TData>(PathFindResultState.StepsExhausted, resultStates);
        }

        /// <summary>
        /// Executes experiments in a specific order.
        /// </summary>
        /// <param name="experiments">The experiments list</param>
        public async Task ExecuteTrainedExperiments(IEnumerable<ExperimentDependency<TData>> experiments)
        {
            var experimentsList = experiments.ToList();
            for (var i = 0; i < experimentsList.Count; ++i)
            {
                var dependency = experimentsList[i];
                var rlPathFinder = new RLPathFinder<TData>(dependency.Environment, dependency.ExperimentState);
                var state = i == 0
                    ? await dependency.Environment.GetInitialState()
                    : await dependency.Environment.GetCurrentState();
                var walkResult = await rlPathFinder.FindRoute(state, dependency.TrainGoal, dependency.MaxSteps);
                if (walkResult.State != PathFindResultState.GoalReached)
                {
                    throw new InvalidOperationException("Dependency could not be verified reaching the goal");
                }
            }
        }
    }
}
