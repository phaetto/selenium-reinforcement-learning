namespace Selenium.Algorithms.ReinforcementLearning
{
    using System.Collections.Generic;

    public readonly struct WalkResult<TData>
    {
        public WalkResultState State { get; }
        public List<StateAndActionPair<TData>> Steps { get; }

        public WalkResult(
            WalkResultState state,
            List<StateAndActionPair<TData>> steps
        )
        {
            State = state;
            Steps = steps;
        }

        public WalkResult(
            WalkResultState state
        ) :  this(state, new List<StateAndActionPair<TData>>())
        {
        }
    }
}
