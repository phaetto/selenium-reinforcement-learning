namespace Selenium.Algorithms
{
    using Selenium.Algorithms.ReinforcementLearning;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public sealed class SeleniumTextEqualsGoal : ITrainGoal<IReadOnlyCollection<ElementData>>
    {
        public string TextValue { get; set; }
        public bool UseOnlyGoals { get; set; }

        public SeleniumTextEqualsGoal(string textValue, bool useOnlyGoals = true)
        {
            TextValue = textValue;
            UseOnlyGoals = useOnlyGoals;
        }

        public Task<bool> HasReachedAGoalCondition(IState<IReadOnlyCollection<ElementData>> state)
        {
            return Task.FromResult(state.Data
                .Where(x => !UseOnlyGoals || x.IsGoalElement)
                .Any(x => x.Text == TextValue)
            );
        }
    }
}
