namespace Selenium.Algorithms
{
    using OpenQA.Selenium;
    using Selenium.Algorithms.ReinforcementLearning;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class SeleniumQLearningStepPolicy : Policy<IReadOnlyCollection<IWebElement>>
    {
        private readonly Random random;

        public SeleniumQLearningStepPolicy(Random random)
        {
            this.random = random;
        }

        public override async Task<AgentAction<IReadOnlyCollection<IWebElement>>> GetNextAction(Environment<IReadOnlyCollection<IWebElement>> environment, State<IReadOnlyCollection<IWebElement>> state)
        {
            // TODO: typing: when element is an input, type instead of click (remember it has to be able to be traced from the goal/reward)

            // Intermediate states (e.g. loading): when you have a state with no elements, create a wait-for-time-action
            if (state.Data.Count == 0)
            {
                return new WaitAction(random.Next(100, 500));
            }

            // TODO: analyse the existing Q-matrix data and find the best action
            // TODO: exploration mode?
            var actions = await environment.GetPossibleActions(state);
            var stateAndActionPairs = actions
                .Select(x =>
                {
                    var pair = new StateAndActionPairWithResultState<IReadOnlyCollection<IWebElement>>(state, x);
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
