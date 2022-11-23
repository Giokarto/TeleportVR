using System;
using UnityEngine;
using Widgets;

namespace DisplayExtensions
{
    public class ClockWidget : Singleton<ClockWidget>
    {
        public TextWidget widget;
        public void Start()
        {
            widget = (TextWidget)Manager.Instance.FindWidgetWithID(42);
        }

        public void Update()
        {
            if (widget == null)
            {
                widget = (TextWidget)Manager.Instance.FindWidgetWithID(42);
            }
            string time = $"It is {DateTime.Now.Hour:D2}:{DateTime.Now.Minute:D2}:{DateTime.Now.Second:D2}.{DateTime.Now.Millisecond:D3}!";
            RosJsonMessage msg =
                RosJsonMessage.CreateTextMessage(42, time, 20, new byte[] { 0xff, 0xff, 0xff, 0xff });
            widget.ProcessRosMessage(msg);
        }
    }
}