namespace Selenium.ReinforcementLearning.Framework.Examples.UnitTests
{
    using OpenQA.Selenium;
    using Selenium.Algorithms;
    using System.Collections.Generic;

    public static class Environments
    {
        public static SeleniumEnvironment Index_LoginAndAddItemToCart(WebDriver driver)
        {
            var inputTextData = new Dictionary<string, string>
            {
                { "user-name", "standard_user" },
                { "password", "secret_sauce" },
            };

            return new SeleniumEnvironment(
                    driver,
                    driver,
                    new SeleniumEnvironmentOptions
                    {
                        Url = Data.HomePage.AbsoluteUri,
                        ActionableElementsCssSelectors = new[]
                        {
                            "#login-button",
                            ".login-box .form_input",
                            ".btn_inventory"
                        },
                        GoalElementSelectors = new[]
                        {
                            ".shopping_cart_link .shopping_cart_badge",
                        },
                        InputTextData = inputTextData,
                    });
        }

        public static SeleniumEnvironment Cart_CheckOut(WebDriver driver)
        {
            var inputTextData = new Dictionary<string, string>
            {
                { "firstName", "Alexander" },
                { "lastName", "Mantzoukas" },
                { "postalCode", "2020" },
            };

            return new SeleniumEnvironment(
                    driver,
                    driver,
                    new SeleniumEnvironmentOptions
                    {
                        Url = Data.CartPage.AbsoluteUri,
                        ActionableElementsCssSelectors = new[] {
                            ".checkout_button",
                            ".checkout_info .form_input",
                            "input[data-test='continue']",
                            "button[data-test='finish']",
                        },
                        GoalElementSelectors = new[]
                        {
                            "h2.complete-header",
                        },
                        InputTextData = inputTextData,
                    });
        }
    }
}
