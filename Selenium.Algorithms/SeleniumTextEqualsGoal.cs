namespace Selenium.Algorithms
{
    using Selenium.Algorithms.ReinforcementLearning;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public sealed class SeleniumTextEqualsGoal : ITrainGoal<IReadOnlyCollection<ElementData>>
    {
        public SeleniumTextEqualsGoal(string textValue, string classToLimit = "")
        {
            TextValue = textValue;
            ClassToLimit = classToLimit;
        }
        public string TextValue { get; set; }
        public string ClassToLimit { get; set; }

        public Task<bool> HasReachedAGoalCondition(IState<IReadOnlyCollection<ElementData>> state, IAgentAction<IReadOnlyCollection<ElementData>> action)
        {
            return Task.FromResult(state.Data
                .Where(x => string.IsNullOrWhiteSpace(ClassToLimit) || x.Class.Contains(ClassToLimit))
                .Any(x => x.Text == TextValue));
        }
    }
}
