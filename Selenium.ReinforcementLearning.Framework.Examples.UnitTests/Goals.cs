namespace Selenium.ReinforcementLearning.Framework.Examples.UnitTests
{
    using OpenQA.Selenium;
    using Selenium.Algorithms;
    using System.Collections.Generic;

    public static class Goals
    {
        public static SeleniumTrainGoal<IReadOnlyCollection<ElementData>> DoesCartHasOneItem(WebDriver driver)
        {
            return new SeleniumTrainGoal<IReadOnlyCollection<ElementData>>(async (_1, _2) => {
                try
                {
                    var target = driver.FindElement(By.CssSelector(".shopping_cart_link .shopping_cart_badge"));

                    return target.Text == "1";
                }
                catch (NoSuchElementException)
                {
                    return false;
                }
            });
        }

        public static SeleniumTrainGoal<IReadOnlyCollection<ElementData>> IsInInventory(WebDriver driver)
        {
            return new SeleniumTrainGoal<IReadOnlyCollection<ElementData>>(async (_1, _2) => {
                try
                {
                    var target = driver.FindElement(By.CssSelector(".btn_inventory"));

                    return target.CanBeInteracted();
                }
                catch (NoSuchElementException)
                {
                    return false;
                }
            });
        }

        public static SeleniumTrainGoal<IReadOnlyCollection<ElementData>> HasOrderBeenPosted(WebDriver driver)
        {
            return new SeleniumTrainGoal<IReadOnlyCollection<ElementData>>(async (_1, _2) => {
                try
                {
                    var target = driver.FindElement(By.CssSelector("h2.complete-header"));

                    return target.Text == "THANK YOU FOR YOUR ORDER";
                }
                catch (NoSuchElementException)
                {
                    return false;
                }
            });
        }
    }
}
