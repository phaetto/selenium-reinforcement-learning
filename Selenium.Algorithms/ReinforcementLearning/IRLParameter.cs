namespace Selenium.Algorithms.ReinforcementLearning
{
    using System.Collections.Generic;

    public interface IRLParameter<TData>
    {
        bool Test(IAgentAction<TData> agentAction);
        IAgentAction<TData> Select(IEnumerable<IAgentAction<TData>> agentActions);
    }
}
