namespace Selenium.Algorithms.ReinforcementLearning
{
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
        public async Task Run(int epochs = 1000, int maximumActions = 1000)
        {
            for (int epoch = 0; epoch < epochs; ++epoch)
            {
                var currentState = await options.Environment.GetInitialState();
                var currentActionCounter = 0;

                do
                {
                    var nextAction = await options.Policy.GetNextAction(options.Environment, currentState, options.ExperimentState);
                    var (nextState, currentStabilizationCounter) = await Step(currentState, nextAction, maximumActions - currentActionCounter);
                    currentActionCounter += currentStabilizationCounter;

                    if (currentActionCounter >= maximumActions)
                    {
                        break;
                    }

                    currentState = nextState;

                    if (await options.TrainGoal.HasReachedAGoalCondition(currentState, nextAction))
                    {
                        ++options.TrainGoal.TimesReachedGoal;
                        break;
                    }
                }
                while (++currentActionCounter < maximumActions);
            }
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

        private async Task ApplyQMatrixLogic(IState<TData> currentState, IAgentAction<TData> nextAction, IState<TData> nextState)
        {
            var nextNextActions = await options.Environment.GetPossibleActions(nextState);
            var maxQ = nextNextActions.Max(x =>
            {
                var pair = new StateAndActionPair<TData>(nextState, x);
                return options.ExperimentState.QualityMatrix.ContainsKey(pair)
                ? options.ExperimentState.QualityMatrix[pair]
                : 0D;
            });

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
