namespace Selenium.Algorithms
{
    using OpenQA.Selenium;
    using Selenium.Algorithms.ReinforcementLearning;
    using System.Collections.Generic;

    /// <summary>
    /// State is defined as the actionable elements state
    /// </summary>
    public class SeleniumState : State<IReadOnlyCollection<IWebElement>>
    {
        public readonly string CachedName;
        public readonly int CachedHash;

        public SeleniumState(
            in IReadOnlyCollection<IWebElement> actionableElements) : base(actionableElements)
        {
            // We have to cache those values because the elements will get out of the DOM eventually
            CachedHash = Data.ExtendedGetHashCode();
            CachedName = Data.ExtendedToString();
        }

        public override bool Equals(object obj)
        {
            var otherState = obj as SeleniumState;
            if (obj == null || Data.Count != otherState.Data.Count)
            {
                return false;
            }

            for (var i = 0; i < Data.Count; ++i)
            {
                if (CachedHash != otherState.CachedHash)
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            return CachedHash;
        }

        public override string ToString()
        {
            return CachedName;
        }
    }
}
