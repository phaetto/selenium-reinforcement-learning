namespace Selenium.Algorithms
{
    using Selenium.Algorithms.ReinforcementLearning;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public sealed class SeleniumTextEqualsGoal : ITrainGoal<IReadOnlyCollection<ElementData>>
    {
        public string TextValue { get; }
        public StringComparison StringComparison { get; }
        public bool UseOnlyGoals { get; }

        public SeleniumTextEqualsGoal(string textValue, StringComparison stringComparison = StringComparison.Ordinal, bool useOnlyGoals = true)
        {
            TextValue = textValue;
            StringComparison = stringComparison;
            UseOnlyGoals = useOnlyGoals;
        }

        public Task<bool> HasReachedAGoalCondition(IState<IReadOnlyCollection<ElementData>> state)
        {
            return Task.FromResult(state.Data
                .Where(x => !UseOnlyGoals || x.IsGoalElement)
                .Any(x => string.Equals(x.Text, TextValue, StringComparison))
            );
        }
    }
}
