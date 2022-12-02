using System.Collections.Generic;
using UnityEngine;
using CurvedUI;

namespace Widgets
{
    /// <summary>
    /// This class instantiates widgets from the prefabs of different widget types.
    /// 
    /// It positions the widgets on the canvas to one of predefined positions.
    /// </summary>
    public class WidgetFactory : Singleton<WidgetFactory>
    {
        // Stored prefabs to instantiate the widgets.
        // Could also be instantiated fully from the script without prefab,
        // then there would be no need for a WidgetFactory.
        public GameObject toastrWidgetPrefab;
        public GameObject textWidgetPrefab;
        public GameObject iconWidgetPrefab;
        public GameObject graphWidgetPrefab;

        public IconWidget CreateIconWidget(string iconName, WidgetPosition position = WidgetPosition.Top, string name = "new IconWidget")
        {
            var widget = GameObject.Instantiate(iconWidgetPrefab);
            widget.name = name;
            var iconWidget = widget.GetComponent<IconWidget>();
            
            iconWidget.SetIcon(iconName);
            SetWidgetPosition(iconWidget, position);

            return iconWidget;
        }
        
        public ToastrWidget CreateToastrWidget(string message, float duration, string name = "new ToastrWidget")
        {
            var widget = GameObject.Instantiate(toastrWidgetPrefab);
            widget.name = name;
            var toastrWidget = widget.GetComponent<ToastrWidget>();
            
            toastrWidget.SetMessage(message);
            toastrWidget.SetDuration(duration);

            ToastrList toastrList = ToastrList.Instance;
            toastrWidget.transform.SetParent(toastrList.transform, false);
            toastrWidget.transform.localPosition -= toastrList.toastrQueue.Count * toastrList.OFFSET * Vector3.up;

            toastrList.EnqueueToastr(toastrWidget);

            return toastrWidget;
        }
        
        public TextWidget CreateTextWidget(string message, WidgetPosition position = WidgetPosition.Center, string name = "new TextWidget")
        {
            var widget = GameObject.Instantiate(textWidgetPrefab);
            widget.name = name;
            var textWidget = widget.GetComponent<TextWidget>();
            
            textWidget.SetMessage(message);
            SetWidgetPosition(textWidget, position);

            print("created textWidget");
            return textWidget;
        }

        private void SetWidgetPosition(Widget widget, WidgetPosition position)
        {
            GameObject parent = GameObject.Find("Widgets" + position);
            widget.transform.SetParent(parent.transform, false);
        }
    }
    
    public enum WidgetPosition { Top, Left, Right, Center, Bottom, BottomLeft, BottomRight };
}