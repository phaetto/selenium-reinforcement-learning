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
    public sealed class RLTrainer<TData>
    {
        private readonly Environment<TData> environment;
        private readonly Policy<TData> policy;
        private readonly double learningRate;
        private readonly double discountRate;

        public readonly IDictionary<StateAndActionPair<TData>, double> QualityMatrix = new Dictionary<StateAndActionPair<TData>, double>();

        public RLTrainer(
            in Environment<TData> environment,
            in Policy<TData> policy,
            in double learningRate = 0.5D,
            in double discountRate = 0.5D)
        {
            this.environment = environment;
            this.policy = policy;
            this.learningRate = learningRate;
            this.discountRate = discountRate;
        }

        /// <summary>
        /// Runs a training session for the number of prespecified epochs
        /// </summary>
        /// <param name="epochs">The amount of iterations that the algorithm will make</param>
        /// <param name="maximumActions">The max actions to apply in an epoch</param>
        public async Task Run(int epochs = 1000, int maximumActions = 1000)
        {
            for (int epoch = 0; epoch < epochs; ++epoch)
            {
                var currentState = await environment.GetInitialState();
                for (int actionNumber = 0; actionNumber < maximumActions; ++actionNumber)
                {
                    var nextAction = await policy.GetNextAction(environment, currentState);

                    currentState = await Step(currentState, nextAction);
                    if (await environment.HasReachedAGoalCondition(currentState, nextAction))
                    {
                        break;
                    }
                }
            }
        }

        public async Task<State<TData>> Step(State<TData> currentState, AgentAction<TData> nextAction)
        {
            var nextState = await nextAction.ExecuteAction(environment, currentState);

            var nextNextActions = await environment.GetPossibleActions(nextState);
            var maxQ = nextNextActions.Max(x => {
                var pair = new StateAndActionPair<TData>(nextState, x);
                return QualityMatrix.ContainsKey(pair)
                ? QualityMatrix[pair]
                : 0D;
            });

            var selectedPair = new StateAndActionPair<TData>(currentState, nextAction);
            if (!QualityMatrix.ContainsKey(selectedPair))
            {
                QualityMatrix.Add(selectedPair, 0D);
            }

            // Q = [(1-a) * Q]  +  [a * (R + (g * maxQ))]
            QualityMatrix[selectedPair] =
                ((1 - learningRate) * QualityMatrix[selectedPair])
                + (learningRate * (await environment.RewardFunction(currentState, nextAction) + (discountRate * maxQ)));

            return nextState;
        }

        /// <summary>
        /// Gets the best route from the starting state to the goal state making decisions using the maximum reward path.
        /// </summary>
        /// <param name="start">The starting state</param>
        /// <param name="target">The target state</param>
        /// <param name="maxSteps">Maximum steps that should be taken</param>
        /// <returns></returns>
        public async Task<WalkResult<TData>> Walk(State<TData> start, Func<State<TData>, AgentAction<TData>, Task<bool>> goalCondition, int maxSteps = 10)
        {
            // TODO: loop sense
            var resultStates = new List<StateAndActionPair<TData>>();

            var currentState = start;
            var iterationNumber = maxSteps;
            while (iterationNumber-- > 0)
            {
                var actions = await environment.GetPossibleActions(currentState);
                var stateAndActionPairs = actions.Select(x => {
                        var pair = new StateAndActionPair<TData>(currentState, x);
                        return QualityMatrix.ContainsKey(pair)
                            ? (x, QualityMatrix[pair])
                            : (x, 0D);
                    })
                    .ToList();

                if (stateAndActionPairs.Count < 1)
                {   
                    return new WalkResult<TData>
                    {
                        State = WalkResultState.Unreachable
                    };
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

                resultStates.Add(new StateAndActionPair<TData>(currentState, maximumReturnAction));

                currentState = await maximumReturnAction.ExecuteAction(environment, currentState);

                if (await goalCondition(currentState, maximumReturnAction))
                {
                    return new WalkResult<TData>
                    {
                        State = WalkResultState.GoalReached,
                        Steps = resultStates,
                    };
                }
            }

            return new WalkResult<TData>
            {
                State = WalkResultState.StepsExhausted,
                Steps = resultStates,
            };
        }
    }
}
