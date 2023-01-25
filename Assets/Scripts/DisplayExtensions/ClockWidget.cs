using System;
using Widgets;

namespace DisplayExtensions
{
    public class ClockWidget : Singleton<ClockWidget>
    {
        public TextWidget timeWidget;
        public void Start()
        {
            timeWidget = WidgetFactory.Instance.CreateTextWidget("what's the time?", WidgetPosition.TopRight, "clockWidget");
        }

        private int oldSeconds = 0;
        public void Update()
        {
            string time = $"{DateTime.Now.Hour:D2}:{DateTime.Now.Minute:D2}:{DateTime.Now.Second:D2}";
            timeWidget.SetMessage(time);

            if (DateTime.Now.Second != oldSeconds)
            {
                oldSeconds = DateTime.Now.Second;
            }
        }
    }
}