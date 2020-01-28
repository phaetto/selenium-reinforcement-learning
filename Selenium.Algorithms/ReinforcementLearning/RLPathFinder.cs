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
    public sealed class RLPathFinder<TData>
    {
        private readonly Environment<TData> environment;
        private readonly Policy<TData> policy;

        public RLPathFinder(
            in Environment<TData> environment,
            in Policy<TData> policy)
        {
            this.environment = environment;
            this.policy = policy;
        }

        /// <summary>
        /// Gets the best route from the starting state to the goal state making decisions using the maximum reward path.
        /// Mutates the environment.
        /// </summary>
        /// <param name="start">The starting state</param>
        /// <param name="target">The target state</param>
        /// <param name="maxSteps">Maximum steps that should be taken</param>
        /// <returns>A report data structure that describes what happened while attempting</returns>
        public async Task<WalkResult<TData>> Walk(State<TData> start, Func<State<TData>, AgentAction<TData>, Task<bool>> goalCondition, int maxSteps = 10)
        {
            var resultStates = new List<StateAndActionPair<TData>>();

            var currentState = start;
            var iterationNumber = maxSteps;
            while (iterationNumber-- > 0)
            {
                var actions = await environment.GetPossibleActions(currentState);
                var stateAndActionPairs = actions
                    .Select(x =>
                    {
                        var pair = new StateAndActionPair<TData>(currentState, x);
                        return policy.QualityMatrix.ContainsKey(pair)
                            ? (x, policy.QualityMatrix[pair])
                            : (x, 0D);
                    })
                    .ToList();

                if (stateAndActionPairs.Count < 1)
                {
                    return new WalkResult<TData>(WalkResultState.Unreachable);
                }

                var maximumValue = 0D;
                var maximumReturnAction = stateAndActionPairs.First().x;
                foreach (var pair in stateAndActionPairs)
                {
                    if (pair.Item2 > maximumValue)
                    {
                        maximumReturnAction = pair.x;
                        maximumValue = pair.Item2;
                    }
                }

                var newState = await maximumReturnAction.ExecuteAction(environment, currentState);
                var newPair = new StateAndActionPairWithResultState<TData>(currentState, maximumReturnAction, newState);

                if (resultStates.Contains(newPair))
                {
                    return new WalkResult<TData>(WalkResultState.LoopDetected, resultStates);
                }

                resultStates.Add(newPair);
                currentState = newState;

                if (await goalCondition(currentState, maximumReturnAction))
                {
                    return new WalkResult<TData>(WalkResultState.GoalReached, resultStates);
                }
            }

            return new WalkResult<TData>(WalkResultState.StepsExhausted, resultStates);
        }

        public async Task<WalkResult<TData>> FindRoute(State<TData> start, Func<State<TData>, AgentAction<TData>, Task<bool>> goalCondition, int maxSteps = 10)
        {
            var resultStates = new List<StateAndActionPair<TData>>();

            var currentState = start;
            var iterationNumber = maxSteps;
            while (iterationNumber-- > 0)
            {
                var actions = await environment.GetPossibleActions(currentState);
                var stateAndActionPairs = actions
                    .Select(x =>
                    {
                        var pair = new StateAndActionPair<TData>(currentState, x);
                        return policy.QualityMatrix.ContainsKey(pair)
                            ? (x, policy.QualityMatrix[pair])
                            : (x, 0D);
                    })
                    .ToList();

                if (stateAndActionPairs.Count < 1)
                {
                    return new WalkResult<TData>(WalkResultState.Unreachable);
                }

                var maximumValue = 0D;
                var maximumReturnAction = stateAndActionPairs.First().x;
                foreach (var pair in stateAndActionPairs)
                {
                    if (pair.Item2 > maximumValue)
                    {
                        maximumReturnAction = pair.x;
                        maximumValue = pair.Item2;
                    }
                }

                var newState = await maximumReturnAction.ExecuteAction(environment, currentState);
                var newPair = new StateAndActionPairWithResultState<TData>(currentState, maximumReturnAction, newState);

                if (resultStates.Contains(newPair))
                {
                    return new WalkResult<TData>(WalkResultState.LoopDetected, resultStates);
                }

                resultStates.Add(newPair);
                currentState = newState;

                if (await goalCondition(currentState, maximumReturnAction))
                {
                    return new WalkResult<TData>(WalkResultState.GoalReached, resultStates);
                }
            }

            return new WalkResult<TData>(WalkResultState.StepsExhausted, resultStates);
        }
    }
}
