namespace Selenium.Algorithms
{
    using Selenium.Algorithms.ReinforcementLearning;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public sealed class SeleniumClassContainsGoal : ITrainGoal<IReadOnlyCollection<ElementData>>
    {
        public string ClassValue { get; set; }
        public bool UseOnlyGoals { get; set; }

        public SeleniumClassContainsGoal(string classValue, bool useOnlyGoals = true)
        {
            ClassValue = classValue;
            UseOnlyGoals = useOnlyGoals;
        }

        public Task<bool> HasReachedAGoalCondition(IState<IReadOnlyCollection<ElementData>> state)
        {
            return Task.FromResult(state.Data
                .Where(x => !UseOnlyGoals || x.IsGoalElement)
                .Any(x => x.Class.Contains(ClassValue))
            );
        }
    }
}
