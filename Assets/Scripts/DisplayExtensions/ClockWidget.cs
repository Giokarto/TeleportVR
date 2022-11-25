using System;
using Widgets;

namespace DisplayExtensions
{
    public class ClockWidget : Singleton<ClockWidget>
    {
        public IconWidget widget;
        public void Start()
        {
            widget = WidgetFactory.Instance.CreateIconWidget("InfoActive", WidgetPosition.Center, "testinfo");
        }

        public void Update()
        {
            string time = $"It is {DateTime.Now.Hour:D2}:{DateTime.Now.Minute:D2}:{DateTime.Now.Second:D2}.{DateTime.Now.Millisecond:D3}!";
            widget.SetIcon(DateTime.Now.Second % 2 == 0? "InfoActive" : "InfoInactive");
        }
    }
}