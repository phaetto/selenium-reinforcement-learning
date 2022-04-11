namespace Selenium.Algorithms
{
    using OpenQA.Selenium;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Drawing;
    using System.Text.Json.Serialization;

    public readonly struct ElementData
    {
        public string Class { get; }
        public string Id { get; }
        public string DataAutomationId { get; }
        public IReadOnlyCollection<string> DataAutomationActions { get; }
        public string TagName { get; }
        public string Text { get; }
        public string Name { get; }
        public bool IsTypingElement { get; }
        public string ExtraState { get; }
        [JsonIgnore]
        public IWebElement WebElementReference { get; }
        public bool IsGoalElement { get; }

        public ElementData(
            string @class,
            string id,
            string dataAutomationId,
            IReadOnlyCollection<string> dataAutomationActions,
            string tagName,
            string text,
            string name,
            bool isTypingElement,
            string extraState,
            IWebElement webElementReference,
            bool isGoalElement = false)
        {
            Class = @class;
            Id = id;
            DataAutomationId = dataAutomationId;
            DataAutomationActions = dataAutomationActions;
            TagName = tagName;
            Text = text;
            Name = name;
            IsTypingElement = isTypingElement;
            ExtraState = extraState;
            WebElementReference = webElementReference;
            IsGoalElement = isGoalElement;
        }

        [JsonConstructor]
        public ElementData(
            string @class,
            string id,
            string dataAutomationId,
            IReadOnlyCollection<string> dataAutomationActions,
            string tagName,
            string text,
            string name,
            bool isTypingElement,
            string extraState,
            bool isGoalElement = false)
        {
            Class = @class;
            Id = id;
            DataAutomationId = dataAutomationId;
            DataAutomationActions = dataAutomationActions;
            TagName = tagName;
            Text = text;
            Name = name;
            IsTypingElement = isTypingElement;
            ExtraState = extraState;
            WebElementReference = new NonInteractibleElement();
            IsGoalElement = isGoalElement;
        }

        private sealed class NonInteractibleElement : IWebElement
        {
            public string TagName => throw new StaleElementReferenceException();

            public string Text => throw new StaleElementReferenceException();

            public bool Enabled => throw new StaleElementReferenceException();

            public bool Selected => throw new StaleElementReferenceException();

            public Point Location => throw new StaleElementReferenceException();

            public Size Size => throw new StaleElementReferenceException();

            public bool Displayed => throw new StaleElementReferenceException();

            public void Clear()
            {
                throw new StaleElementReferenceException();
            }

            public void Click()
            {
                throw new StaleElementReferenceException();
            }

            public IWebElement FindElement(By by)
            {
                throw new StaleElementReferenceException();
            }

            public ReadOnlyCollection<IWebElement> FindElements(By by)
            {
                throw new StaleElementReferenceException();
            }

            public string GetAttribute(string attributeName)
            {
                throw new StaleElementReferenceException();
            }

            public string GetCssValue(string propertyName)
            {
                throw new StaleElementReferenceException();
            }

            public string GetDomAttribute(string attributeName)
            {
                throw new StaleElementReferenceException();
            }

            public string GetDomProperty(string propertyName)
            {
                throw new StaleElementReferenceException();
            }

            public string GetProperty(string propertyName)
            {
                throw new StaleElementReferenceException();
            }

            public ISearchContext GetShadowRoot()
            {
                throw new StaleElementReferenceException();
            }

            public void SendKeys(string text)
            {
                throw new StaleElementReferenceException();
            }

            public void Submit()
            {
                throw new StaleElementReferenceException();
            }
        }
    }
}
