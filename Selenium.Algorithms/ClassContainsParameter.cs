namespace Selenium.Algorithms
{
    using Selenium.Algorithms.ReinforcementLearning;
    using System.Collections.Generic;
    using System.Linq;

    public sealed class ClassContainsParameter : IRLParameter<IReadOnlyCollection<ElementData>>
    {
        private readonly string className;
        private readonly int positionalElement;

        public ClassContainsParameter(string className, int positionalElement)
        {
            this.className = className;
            this.positionalElement = positionalElement;
        }

        public IAgentAction<IReadOnlyCollection<ElementData>> Select(IEnumerable<IAgentAction<IReadOnlyCollection<ElementData>>> agentActions)
        {
            var agentActionsForElement = agentActions
                .Cast<IAgentActionForElement>()
                .Where(x => x.ElementData.Class.Contains(className))
                .ToArray();
            return (IAgentAction<IReadOnlyCollection<ElementData>>)agentActionsForElement[positionalElement];
        }

        public bool Test(IAgentAction<IReadOnlyCollection<ElementData>> agentAction)
        {
            if (agentAction is IAgentActionForElement agentActionForElement)
            {
                if (agentActionForElement.ElementData.Class.Contains(className))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
