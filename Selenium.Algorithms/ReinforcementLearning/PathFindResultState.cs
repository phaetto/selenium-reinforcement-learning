namespace Selenium.Algorithms.ReinforcementLearning
{
    /// <summary>
    /// Defines the status of path finding results
    /// </summary>
    public enum PathFindResultState
    {
        /// <summary>
        /// The goal condition was never reached by this policy and using the Q-matrix is not suggested
        /// </summary>
        GoalNeverReached,
        /// <summary>
        /// The goal condition has been reached and the result contains the successful steps
        /// </summary>
        GoalReached,
        /// <summary>
        /// The path lead to a state that there are no available actions to move forward
        /// </summary>
        Unreachable,
        /// <summary>
        /// The algorithm has already applied the maximum allowed number of actions
        /// </summary>
        StepsExhausted,
        /// <summary>
        /// Action loop has been detected while trying to find a path
        /// </summary>
        LoopDetected,
        /// <summary>
        /// Data that would help discover the next state without activating any action are not provided
        /// </summary>
        /// <remarks>
        /// Special state for FindRouteWithoutApplyingActions
        /// </remarks>
        DataNotIncluded,
    }
}
