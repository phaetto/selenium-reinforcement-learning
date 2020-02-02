namespace Selenium.Algorithms.ReinforcementLearning
{
    using Selenium.Algorithms.ReinforcementLearning.Repetitions;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Reinforcement learning trainer
    /// </summary>
    /// <typeparam name="TData">The prime data for state - exposed for convienience</typeparam>
    public sealed class RLTrainer<TData>
    {
        private readonly Environment<TData> environment;
        private readonly Policy<TData> policy;
        private readonly ITrainGoal<TData> trainGoal;
        private readonly double learningRate;
        private readonly double discountRate;

        public RLTrainer(
            in Environment<TData> environment,
            in Policy<TData> policy,
            in ITrainGoal<TData> trainGoal,
            in double learningRate = 0.5D,
            in double discountRate = 0.5D)
        {
            this.environment = environment;
            this.policy = policy;
            this.trainGoal = trainGoal;
            this.learningRate = learningRate;
            this.discountRate = discountRate;
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
                var currentState = await environment.GetInitialState();
                var actionRepetitionContext = new RepetitionContext(maximumActions);

                do
                {
                    var nextAction = await policy.GetNextAction(environment, currentState);
                    var nextState = await nextAction.ExecuteAction(environment, currentState);

                    if (await environment.IsIntermediateState(nextState))
                    {
                        await environment.WaitForPostActionIntermediateStabilization(actionRepetitionContext);

                        if (!actionRepetitionContext.CanContinue())
                        {
                            break;
                        }

                        nextState = await environment.GetCurrentState();
                    }

                    await ApplyQMatrixLogic(currentState, nextAction, nextState);
                    currentState = nextState;

                    if (await trainGoal.HasReachedAGoalCondition(currentState, nextAction))
                    {
                        break;
                    }
                }
                while (actionRepetitionContext.Step());
            }
        }

        /// <summary>
        /// Executes one step of the algorithm with a predetermined state and action.
        /// </summary>
        /// <param name="currentState">The state the algorithm will start from</param>
        /// <param name="nextAction">The action that will try to apply</param>
        /// <returns>The new state after the action has been resolved</returns>
        public async Task<State<TData>> Step(State<TData> currentState, AgentAction<TData> nextAction, int maximumActions = 10)
        {
            var nextState = await nextAction.ExecuteAction(environment, currentState);

            var actionRepetitionContext = new RepetitionContext(maximumActions);
            if (await environment.IsIntermediateState(nextState))
            {
                await environment.WaitForPostActionIntermediateStabilization(actionRepetitionContext);

                if (!actionRepetitionContext.CanContinue())
                {
                    throw new TimeoutException($"Repetitions reached maximum value (maximumActions = {maximumActions}) when waiting for stabilization");
                }

                nextState = await environment.GetCurrentState();
            }

            await ApplyQMatrixLogic(currentState, nextAction, nextState);

            return nextState;
        }

        private async Task ApplyQMatrixLogic(State<TData> currentState, AgentAction<TData> nextAction, State<TData> nextState)
        {
            var nextNextActions = await environment.GetPossibleActions(nextState);
            var maxQ = nextNextActions.Max(x =>
            {
                var pair = new StateAndActionPair<TData>(nextState, x);
                return policy.QualityMatrix.ContainsKey(pair)
                ? policy.QualityMatrix[pair]
                : 0D;
            });

            var selectedPair = new StateAndActionPairWithResultState<TData>(currentState, nextAction, nextState);
            if (!policy.QualityMatrix.ContainsKey(selectedPair))
            {
                policy.QualityMatrix.Add(selectedPair, 0D);
            }

            // Q = [(1-a) * Q]  +  [a * (R + (g * maxQ))]
            policy.QualityMatrix[selectedPair] =
                ((1 - learningRate) * policy.QualityMatrix[selectedPair])
                + (learningRate * (await trainGoal.RewardFunction(currentState, nextAction) + (discountRate * maxQ)));
        }
    }
}
