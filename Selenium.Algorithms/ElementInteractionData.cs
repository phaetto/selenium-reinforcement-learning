using OpenQA.Selenium;

namespace Selenium.Algorithms
{
    public readonly struct ElementInteractionData
    {
        public bool IsInViewPort { get; }
        public int Y { get; }
        public int X { get; }
        public int Width { get; }
        public int Height { get; }
        public bool IsEnabled { get; }
        public IWebElement WebElementReference { get; }

        public ElementInteractionData(
            bool isInViewPort,
            int y,
            int x,
            int width,
            int height,
            bool isEnabled,
            IWebElement webElementReference
            )
        {
            IsInViewPort = isInViewPort;
            Y = y;
            X = x;
            Width = width;
            Height = height;
            IsEnabled = isEnabled;
            WebElementReference = webElementReference;
        }
    }
}
