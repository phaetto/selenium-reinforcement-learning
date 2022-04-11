namespace Selenium.ReinforcementLearning.Framework.Examples.UnitTests
{
    using Selenium.Algorithms;

    public static class Goals
    {
        public static SeleniumTextEqualsGoal DoesCartHasOneItem()
        {
            return new SeleniumTextEqualsGoal("1", "shopping_cart_badge");
        }

        public static SeleniumClassContainsGoal IsInventoryVisible()
        {
            return new SeleniumClassContainsGoal("btn_inventory");
        }

        public static SeleniumTextEqualsGoal HasOrderBeenPosted()
        {
            return new SeleniumTextEqualsGoal("THANK YOU FOR YOUR ORDER");
        }
    }
}
