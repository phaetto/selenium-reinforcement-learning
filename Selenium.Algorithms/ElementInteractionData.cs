using OpenQA.Selenium;

namespace Selenium.Algorithms
{
    public class ElementInteractionData
    {
        public bool IsInViewPort { get; set; }
        public int Y { get; set; }
        public int X { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool IsEnabled { get; set; }
        public IWebElement WebElementReference { get; set; }
    }
}
