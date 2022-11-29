using System;
using Widgets;

namespace DisplayExtensions
{
    public class ClockWidget : Singleton<ClockWidget>
    {
        public TextWidget timeWidget;
        public void Start()
        {
            timeWidget = WidgetFactory.Instance.CreateTextWidget("what's the time?", WidgetPosition.Right, "clockWidget");
        }

        private int oldSeconds = 0;
        public void Update()
        {
            string time = $"It is {DateTime.Now.Hour:D2}:{DateTime.Now.Minute:D2}:{DateTime.Now.Second:D2}.{DateTime.Now.Millisecond:D3}!";
            timeWidget.SetMessage(time);

            if (DateTime.Now.Second != oldSeconds)
            {
                oldSeconds = DateTime.Now.Second;
                WidgetFactory.Instance.CreateToastrWidget($"Toastr says {DateTime.Now.Second:D2}", oldSeconds > 50? 2f : 0.5f);
            }
        }
    }
}