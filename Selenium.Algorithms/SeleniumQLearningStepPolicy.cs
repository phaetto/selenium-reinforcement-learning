namespace Selenium.Algorithms
{
    using Selenium.Algorithms.ReinforcementLearning;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    public class SeleniumQLearningStepPolicy : Policy<IReadOnlyCollection<ElementData>>
    {
        private readonly Random random;

        public SeleniumQLearningStepPolicy(Random random)
        {
            this.random = random;
        }

        public override async Task<AgentAction<IReadOnlyCollection<ElementData>>> GetNextAction(Environment<IReadOnlyCollection<ElementData>> environment, State<IReadOnlyCollection<ElementData>> state)
        {
            var actions = await environment.GetPossibleActions(state);

            Debug.Assert(state.Data.Count > 0, $"A state reached {nameof(SeleniumQLearningStepPolicy)} that has no data");

            // TODO: exploration mode?
            var stateAndActionPairs = actions
                .Select(x =>
                {
                    var pair = new StateAndActionPair<IReadOnlyCollection<ElementData>>(state, x);
                    return QualityMatrix.ContainsKey(pair)
                        ? (Action: x, Score: QualityMatrix[pair])
                        : (Action: x, Score: 0D);
                })
                .OrderByDescending(x => x.Score)
                .ToList();
            
            // Take all the items that compete for the maximum value
            var maxStateAndActionPairs = stateAndActionPairs
                .Where(x => x.Score == stateAndActionPairs[0].Score)
                .ToList();

            return maxStateAndActionPairs.ElementAt(random.Next(0, maxStateAndActionPairs.Count)).Action;
        }
    }
}
