namespace Selenium.Algorithms
{
    using Selenium.Algorithms.ReinforcementLearning;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    public sealed class SeleniumQLearningStepPolicy : IPolicy<IReadOnlyCollection<ElementData>>
    {
        private readonly Random random;

        public SeleniumQLearningStepPolicy(Random random)
        {
            this.random = random;
        }
       
        public async Task<IAgentAction<IReadOnlyCollection<ElementData>>> GetNextAction(
            IEnvironment<IReadOnlyCollection<ElementData>> environment,
            IState<IReadOnlyCollection<ElementData>> state,
            IExperimentState<IReadOnlyCollection<ElementData>> experimentState)
        {
            var actions = await environment.GetPossibleActions(state);

            Debug.Assert(state.Data.Count > 0, $"A state reached {nameof(SeleniumQLearningStepPolicy)} that has no data");

            // TODO: exploration mode for some iterations could be beneficial (or another class)
            var stateAndActionPairs = actions
                .Select(x =>
                {
                    var pair = new StateAndActionPair<IReadOnlyCollection<ElementData>>(state, x);
                    return experimentState.QualityMatrix.ContainsKey(pair)
                        ? (Action: x, Score: experimentState.QualityMatrix[pair])
                        : (Action: x, Score: 0D);
                })
                .OrderByDescending(x => x.Score)
                .ToList();
            
            // Take all the items that compete for the maximum value
            var maxStateAndActionPairs = stateAndActionPairs
                .Where(x => x.Score == stateAndActionPairs[0].Score)
                .ToList();

            return maxStateAndActionPairs.Count > 1
                ? maxStateAndActionPairs.ElementAt(random.Next(0, maxStateAndActionPairs.Count)).Action
                : maxStateAndActionPairs[0].Action;
        }
    }
}
