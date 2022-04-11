namespace Selenium.Algorithms.ReinforcementLearning
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Reinforcement learning trainer
    /// </summary>
    /// <typeparam name="TData">The prime data for state - exposed for convienience</typeparam>
    public sealed class RLTrainer<TData> : IRLTrainer<TData>
    {
        private readonly IRLTrainerOptions<TData> options;

        public RLTrainer(IRLTrainerOptions<TData> options)
        {
            this.options = options;
        }

        /// <summary>
        /// Runs a training session for the number of prespecified epochs
        /// </summary>
        /// <param name="epochs">The amount of iterations that the algorithm will make (each epoch starts from the initial state)</param>
        /// <param name="maximumActions">The max actions to apply in an epoch</param>
        public async Task<TrainerReport> Run(int epochs = 1000, int maximumActions = 1000)
        {
            var timesReachedGoal = 0;
            var totalActionsRun = 0;
            var stabilizationWaitCount = 0;
            var dependencies = options.Dependencies.ToList();
            for (int epoch = 0; epoch < epochs; ++epoch)
            {
                await ExecuteDependentTrainedExperiments(dependencies);

                var currentState = await options.Environment.GetInitialState();
                var currentActionCounter = 0;

                while (await options.Environment.IsIntermediateState(currentState) && currentActionCounter < maximumActions)
                {
                    await options.Environment.WaitForPostActionIntermediateStabilization();
                    currentState = await options.Environment.GetCurrentState();
                    ++currentActionCounter;
                    ++totalActionsRun;
                    ++stabilizationWaitCount;
                }

                if (currentActionCounter >= maximumActions)
                {
                    continue;
                }

                do
                {
                    var nextAction = await options.Policy.GetNextAction(options.Environment, currentState, options.ExperimentState);
                    if (nextAction is NoAction<TData> && await options.TrainGoal.HasReachedAGoalCondition(currentState))
                    {
                        ++timesReachedGoal;
                        break;
                    }

                    var (nextState, currentStabilizationCounter) = await Step(currentState, nextAction, maximumActions - currentActionCounter);
                    currentActionCounter += currentStabilizationCounter;
                    totalActionsRun += currentStabilizationCounter + 1;
                    stabilizationWaitCount += currentStabilizationCounter;

                    if (await options.TrainGoal.HasReachedAGoalCondition(currentState))
                    {
                        ++timesReachedGoal;
                        break;
                    }

                    if (currentActionCounter >= maximumActions)
                    {
                        break;
                    }

                    currentState = nextState;
                }
                while (++currentActionCounter < maximumActions);
            }

            return new TrainerReport(timesReachedGoal, totalActionsRun, stabilizationWaitCount, epochs);
        }

        /// <summary>
        /// Executes one step of the algorithm with a predetermined state and action.
        /// </summary>
        /// <param name="currentState">The state the algorithm will start from</param>
        /// <param name="nextAction">The action that will try to apply</param>
        /// <returns>The new state after the action has been resolved</returns>
        public async Task<(IState<TData>, int)> Step(IState<TData> currentState, IAgentAction<TData> nextAction, int maximumWaitForStabilization = 1000)
        {
            var nextState = await nextAction.ExecuteAction(options.Environment, currentState);

            var currentStabilizationCounter = 0;
            while (await options.Environment.IsIntermediateState(nextState) && currentStabilizationCounter < maximumWaitForStabilization)
            {
                if (await options.TrainGoal.HasReachedAGoalCondition(nextState))
                {
                    break;
                }

                await options.Environment.WaitForPostActionIntermediateStabilization();
                nextState = await options.Environment.GetCurrentState();
                ++currentStabilizationCounter;
            }

            if (currentStabilizationCounter >= maximumWaitForStabilization)
            {
                return (nextState, currentStabilizationCounter);
            }

            await ApplyQMatrixLogic(currentState, nextAction, nextState);

            return (nextState, currentStabilizationCounter);
        }

        private static async Task ExecuteDependentTrainedExperiments(List<ExperimentDependency<TData>> dependencies)
        {
            for (var i = 0; i < dependencies.Count; ++i)
            {
                var dependency = dependencies[0];
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

        private async Task ApplyQMatrixLogic(IState<TData> currentState, IAgentAction<TData> nextAction, IState<TData> nextState)
        {
            var nextNextActions = await options.Environment.GetPossibleActions(nextState);
            var maxQ = nextNextActions.Any()
                ? nextNextActions.Max(x =>
                {
                    var pair = new StateAndActionPair<TData>(nextState, x);
                    return options.ExperimentState.QualityMatrix.ContainsKey(pair)
                    ? options.ExperimentState.QualityMatrix[pair]
                    : 0D;
                })
                : 0D;

            var selectedPair = new StateAndActionPairWithResultState<TData>(currentState, nextAction, nextState);
            if (!options.ExperimentState.QualityMatrix.ContainsKey(selectedPair))
            {
                options.ExperimentState.QualityMatrix.Add(selectedPair, 0D);
            }

            // Q = [(1-a) * Q]  +  [a * (R + (g * maxQ))]
            options.ExperimentState.QualityMatrix[selectedPair] =
                ((1 - options.LearningRate) * options.ExperimentState.QualityMatrix[selectedPair])
                + (options.LearningRate * (await options.TrainGoal.RewardFunction(currentState, nextAction) + (options.DiscountRate * maxQ)));
        }
    }
}
