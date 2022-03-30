namespace Selenium.ReinforcementLearning.Framework.Examples.UnitTests
{
    using OpenQA.Selenium;
    using Selenium.Algorithms;
    using Selenium.Algorithms.ReinforcementLearning;
    using System;
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
                        ActionableElementsCssSelectors = new[] {
                            "#login-button",
                            ".login-box .form_input",
                            ".btn_inventory"
                        },
                        InputTextData = inputTextData,
                    });
        }

        public static SeleniumEnvironment Cart_CheckOut(WebDriver driver, SeleniumExperimentState seleniumExperimentState)
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
                    new SeleniumEnvironmentOptions // TODO: Add dependency for other experiments (maybe)
                    {
                        Url = Data.CartPage.AbsoluteUri,
                        ActionableElementsCssSelectors = new[] {
                            ".checkout_button",
                            ".checkout_info .form_input",
                            "input[data-test='continue']",
                            "button[data-test='finish']",
                        },
                        InputTextData = inputTextData,
                        SetupInitialState = async (webDriver, options) =>
                        {
                            // Dependency on Environment, ExperimentState, Goal
                            var Index_AddAnyItemToCartEnvironment = Index_LoginAndAddItemToCart(driver);
                            var initialState = await Index_AddAnyItemToCartEnvironment.GetInitialState();
                            var pathFinder = new RLPathFinder<IReadOnlyCollection<ElementData>>(Index_AddAnyItemToCartEnvironment, seleniumExperimentState);
                            var pathList = await pathFinder.FindRoute(initialState, Goals.IsInInventory(driver));
                            if (pathList.State != PathFindResultState.GoalReached)
                            {
                                throw new InvalidOperationException();
                            }

                            webDriver.Navigate().GoToUrl(Data.CartPage);
                        }
                    });
        }
    }
}
