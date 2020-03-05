using System.Threading.Tasks;

namespace Selenium.Algorithms.ReinforcementLearning
{
    public interface IRLTrainer<TData>
    {
        Task Run(int epochs = 1000, int maximumActions = 1000);
        Task<(IState<TData>, int)> Step(IState<TData> currentState, IAgentAction<TData> nextAction, int maximumWaitForStabilization = 1000);
    }
}