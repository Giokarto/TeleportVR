using System;
using System.Collections.Generic;

namespace Widgets2
{
    /// <summary>
    /// This class watches over all widgets. It takes care of the Canvas and positions the widgets on it.
    /// 
    /// It also creates the most essential widgets at startup.
    /// </summary>
    public class WidgetManager : Singleton<WidgetManager>
    {
        public List<Widget> AllWidgets;
        
        private IconWidget wifiWidget;
        private IconWidget micWidget;
        private IconWidget speakerWidget;
        private IconWidget motorsWidget;
        
        public void RegisterWidget(Widget widget, WidgetPosition position)
        {
            
        }

        public void Start()
        {
            wifiWidget = IconWidget.
        }
    }
    
    public enum WidgetPosition { Top, Left, Right, Center, Bottom, Child, BottomLeft, BottomRight, Incorrect };
}