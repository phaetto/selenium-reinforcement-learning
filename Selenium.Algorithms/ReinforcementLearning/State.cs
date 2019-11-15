namespace Selenium.Algorithms.ReinforcementLearning
{
    public abstract class State<TData>
    {
        public State(in TData data)
        {
            Data = data;
        }

        public TData Data { get; }
        public abstract override bool Equals(object obj);
        public abstract override int GetHashCode();
        public abstract override string ToString();
    }
}
