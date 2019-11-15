namespace Selenium.Algorithms.ReinforcementLearning
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

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
        public void Run(in int epochs = 1000)
        {
            for (int epoch = 0; epoch < epochs; ++epoch)
            {
                var currentState = environment.GetInitialState();
                while (true)
                {
                    var nextAction = policy.GetNextAction(environment, currentState);

                    currentState = Step(currentState, nextAction);
                    if (environment.HasReachedAGoalState(currentState))
                    {
                        break;
                    }
                }
            }
        }

        public State<TData> Step(State<TData> currentState, AgentAction<TData> nextAction)
        {
            var nextState = nextAction.ExecuteAction(environment, currentState);

            var nextNextActions = environment.GetPossibleActions(nextState);
            var maxQ = nextNextActions.Max(x => {
                var pair = new StateAndActionPair<TData>(nextState, x);
                return QualityMatrix.ContainsKey(pair)
                ? QualityMatrix[pair]
                : 0D;
            });

            // TODO: If max is 0 then get to exploration mode

            var selectedPair = new StateAndActionPair<TData>(currentState, nextAction);
            if (!QualityMatrix.ContainsKey(selectedPair))
            {
                QualityMatrix.Add(selectedPair, 0D);
            }

            // Q = [(1-a) * Q]  +  [a * (R + (g * maxQ))]
            QualityMatrix[selectedPair] =
                ((1 - learningRate) * QualityMatrix[selectedPair])
                + (learningRate * (environment.RewardFunction(currentState, nextAction) + (discountRate * maxQ)));

            return nextState;
        }

        /// <summary>
        /// Gets the best route from the starting state to the goal state making decisions using the maximum reward path.
        /// </summary>
        /// <param name="start">The starting state</param>
        /// <param name="target">The target state</param>
        /// <param name="maxSteps">Maximum steps that should be taken</param>
        /// <returns></returns>
        public List<StateAndActionPair<TData>> Walk(in State<TData> start, in Func<State<TData>, bool> goalCondition, in int maxSteps = 10)
        {
            // TODO: loop sense
            var resultStates = new List<StateAndActionPair<TData>>();

            var currentState = start;
            var iterationNumber = maxSteps;
            while (iterationNumber-- > 0)
            {
                var actions = environment.GetPossibleActions(currentState);
                var stateAndActionPairs = actions.Select(x => {
                        var pair = new StateAndActionPair<TData>(currentState, x);
                        return QualityMatrix.ContainsKey(pair)
                            ? (x, QualityMatrix[pair])
                            : (x, 0D);
                    })
                    .ToList();

                if (stateAndActionPairs.Count < 1)
                {
                    // Unreachable
                    return null;
                }

                var maximumValue = 0D;
                AgentAction<TData> maximumReturnAction = stateAndActionPairs.First().x;
                foreach (var pair in stateAndActionPairs)
                {
                    if (pair.Item2 > maximumValue)
                    {
                        maximumReturnAction = pair.x;
                        maximumValue = pair.Item2;
                    }
                }

                resultStates.Add(new StateAndActionPair<TData>(currentState, maximumReturnAction));

                currentState = maximumReturnAction.ExecuteAction(environment, currentState);

                if (goalCondition(currentState))
                {
                    break;
                }
            }

            return resultStates;
        }
    }
}
