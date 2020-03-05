namespace Selenium.Algorithms.ReinforcementLearning
{
    public interface IState<TData>
    {
        public TData Data { get; }

        public bool Equals(object obj);
        public int GetHashCode();
        public string ToString();
    }
}
