namespace Selenium.Algorithms.ReinforcementLearning
{
    public abstract class State<TData>
    {
        public TData Data { get; }

        public State(in TData data)
        {
            Data = data;
        }

        private State()
        {
            Data = default;
        }

        public abstract override bool Equals(object obj);
        public abstract override int GetHashCode();
        public abstract override string ToString();
    }
}
