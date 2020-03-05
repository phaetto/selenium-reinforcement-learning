using System.Threading.Tasks;

namespace Selenium.Algorithms.ReinforcementLearning
{
    public interface IRLPathFinder<TData>
    {
        Task<WalkResult<TData>> FindRoute(State<TData> start, ITrainGoal<TData> trainGoal, int maxSteps = 10);
        Task<WalkResult<TData>> FindRouteWithoutApplyingActions(State<TData> start, ITrainGoal<TData> trainGoal, int maxSteps = 10);
    }
}