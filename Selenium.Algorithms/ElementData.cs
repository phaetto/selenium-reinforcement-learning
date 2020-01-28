using OpenQA.Selenium;
using System.Collections.Generic;

namespace Selenium.Algorithms
{
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
        public IWebElement WebElementReference { get; }

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
            IWebElement webElementReference)
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
        }
    }
}
