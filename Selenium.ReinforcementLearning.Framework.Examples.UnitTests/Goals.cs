namespace Selenium.ReinforcementLearning.Framework.Examples.UnitTests
{
    using Selenium.Algorithms;

    public static class Goals
    {
        public static SeleniumTextEqualsGoal DoesCartHasOneItem()
        {
            return new SeleniumTextEqualsGoal("1");
        }

        public static SeleniumTextEqualsGoal DoesCartHasTwoItems()
        {
            return new SeleniumTextEqualsGoal("2");
        }

        public static SeleniumTextEqualsGoal HasOrderBeenPosted()
        {
            return new SeleniumTextEqualsGoal("THANK YOU FOR YOUR ORDER");
        }
    }
}
