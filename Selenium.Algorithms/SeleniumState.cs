namespace Selenium.Algorithms
{
    using Selenium.Algorithms.ReinforcementLearning;
    using System.Collections.Generic;

    /// <summary>
    /// State is defined as the actionable elements state
    /// </summary>
    public class SeleniumState : IState<IReadOnlyCollection<ElementData>>
    {
        public readonly string CachedName;
        public readonly int CachedHash;

        public SeleniumState(
            in IReadOnlyCollection<ElementData> actionableElementsData)
        {
            Data = actionableElementsData;

            // We have to cache those values because the elements will get out of the DOM eventually
            CachedHash = actionableElementsData.ExtendedGetHashCode();
            CachedName = actionableElementsData.ExtendedToString();
        }

        public IReadOnlyCollection<ElementData> Data { get; }

        public override bool Equals(object obj)
        {
            var otherState = obj as SeleniumState;
            if (obj == null || otherState == null || Data.Count != otherState.Data.Count)
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
