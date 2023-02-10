using System;
using Widgets;

namespace Widgets.ActiveWidgets
{
    /// <summary>
    /// Shows a clock in the top right corner.
    /// </summary>
    public class ClockWidget : Singleton<ClockWidget>
    {
        public TextWidget timeWidget;
        public void Start()
        {
            timeWidget = WidgetFactory.Instance.CreateTextWidget(timeMessage, WidgetPosition.TopRight, "clockWidget");
        }

        private string timeMessage = "What's the time?";
        public void Update()
        {
            timeMessage = $"{DateTime.Now.Hour:D2}:{DateTime.Now.Minute:D2}:{DateTime.Now.Second:D2}";
            timeWidget.SetMessage(timeMessage);
        }
    }
}