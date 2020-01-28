namespace Selenium.Algorithms
{
    using OpenQA.Selenium;
    using Selenium.Algorithms.ReinforcementLearning;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class WaitAction : AgentAction<IReadOnlyCollection<ElementData>>
    {
        private readonly int milliseconds;

        public WaitAction(int milliseconds)
        {
            this.milliseconds = milliseconds;
        }

        public override bool Equals(object obj)
        {
            return obj is WaitAction otherAction
                && milliseconds == otherAction.milliseconds;
        }

        public override async Task<State<IReadOnlyCollection<ElementData>>> ExecuteAction(Environment<IReadOnlyCollection<ElementData>> environment, State<IReadOnlyCollection<ElementData>> state)
        {
            Console.Write($"\t- waiting for {milliseconds}ms");
            await Task.Delay(milliseconds);
            Console.WriteLine($" ... done!");
            return ((SeleniumEnvironment)environment).GetCurrentState();
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override string ToString()
        {
            return $"Waiting for {milliseconds}ms";
        }
    }
}
