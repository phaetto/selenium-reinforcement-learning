namespace Selenium.ReinforcementLearning.Framework.Examples.UnitTests
{
    using OpenQA.Selenium;
    using Selenium.Algorithms;
    using System.Collections.Generic;
    using System.Linq;

    public static class Goals
    {
        public static SeleniumTrainGoal<IReadOnlyCollection<ElementData>> DoesCartHasOneItem()
        {
            return new SeleniumTrainGoal<IReadOnlyCollection<ElementData>>(async (state, _2) => {
                return state.Data
                    .Where(x => x.Class.Contains("shopping_cart_badge"))
                    .Any(x => x.Text == "1");
            });
        }

        public static SeleniumTrainGoal<IReadOnlyCollection<ElementData>> IsInInventory()
        {
            return new SeleniumTrainGoal<IReadOnlyCollection<ElementData>>(async (state, _2) => {
                return state.Data.Any(x => x.Class.Contains("btn_inventory"));
            });
        }

        public static SeleniumTrainGoal<IReadOnlyCollection<ElementData>> HasOrderBeenPosted()
        {
            return new SeleniumTrainGoal<IReadOnlyCollection<ElementData>>(async (state, _2) => {
                return state.Data.Any(x => x.Text == "THANK YOU FOR YOUR ORDER");
            });
        }
    }
}
