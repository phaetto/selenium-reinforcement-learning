namespace Selenium.ReinforcementLearning.Framework.Examples.UnitTests
{
    using Selenium.Algorithms;

    public static class Goals
    {
        public static SeleniumTextEqualsGoal DoesCartHasOneItem()
        {
            return new SeleniumTextEqualsGoal("1");
        }

        public static SeleniumClassContainsGoal IsInventoryVisible()
        {
            return new SeleniumClassContainsGoal("btn_inventory", false);
        }

        public static SeleniumTextEqualsGoal HasOrderBeenPosted()
        {
            return new SeleniumTextEqualsGoal("THANK YOU FOR YOUR ORDER");
        }
    }
}
