namespace Selenium.Algorithms.ReinforcementLearning
{
    using System.Diagnostics;

    [DebuggerDisplay("State: {State.ToString()} --- Action: {Action.ToString()}")]
    public class StateAndActionPair<TData>
    {
        public StateAndActionPair(
            in IState<TData> state,
            in IAgentAction<TData> action)
        {
            State = state;
            Action = action;
        }

        public IState<TData> State { get; }
        public IAgentAction<TData> Action { get; }

        public override bool Equals(object obj)
        {
            return obj is StateAndActionPair<TData> stateAndAction
                && Equals(stateAndAction.State, State)
                && Equals(stateAndAction.Action, Action);
        }

        public override int GetHashCode()
        {
            var hash = 13;
            hash = (hash * 7) + State.GetHashCode();
            hash = (hash * 7) + Action.GetHashCode();
            return hash;
        }
    }

    [DebuggerDisplay("State: {State.ToString()} --- Action: {Action.ToString()} --- Plus result")]
    public class StateAndActionPairWithResultState<TData> : StateAndActionPair<TData>
    {
        public StateAndActionPairWithResultState(
            in IState<TData> state,
            in IAgentAction<TData> action,
            in IState<TData> resultState)
            : base(state, action)
        {
            ResultState = resultState;
        }
        public IState<TData> ResultState { get; }
    }
}
