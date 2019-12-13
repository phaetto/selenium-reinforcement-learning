namespace Selenium.Algorithms.ReinforcementLearning
{
    using System.Collections.Generic;

    public sealed class WalkResult<TData>
    {
        public WalkResultState State { get; set; }
        public List<StateAndActionPair<TData>> Steps { get; set; }
    }
}
