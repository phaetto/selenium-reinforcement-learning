namespace Selenium.Algorithms.ReinforcementLearning
{
    using System.Collections.Generic;

    public readonly struct WalkResult<TData>
    {
        public PathFindResultState State { get; }
        public List<StateAndActionPair<TData>> Steps { get; }

        public WalkResult(
            PathFindResultState state,
            List<StateAndActionPair<TData>> steps
        )
        {
            State = state;
            Steps = steps;
        }

        public WalkResult(
            PathFindResultState state
        ) :  this(state, new List<StateAndActionPair<TData>>())
        {
        }
    }
}
