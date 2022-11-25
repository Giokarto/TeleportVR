using System.Collections.Generic;
using UnityEngine;

namespace Widgets
{
    /// <summary>
    /// This class instantiates widgets from the prefabs of different widget types.
    /// 
    /// It positions the widgets on the canvas to one of predefined positions.
    /// </summary>
    public class WidgetFactory : Singleton<WidgetFactory>
    {
        public Dictionary<string, Texture2D> Icons;
        
        public GameObject toastrWidgetPrefab;
        public GameObject textWidgetPrefab;
        public GameObject iconWidgetPrefab;
        public GameObject graphWidgetPrefab;

        public IconWidget CreateIconWidget(string iconName, WidgetPosition position = WidgetPosition.Top, string name = "new IconWidget")
        {
            var widget = GameObject.Instantiate(iconWidgetPrefab);
            widget.name = name;
            var iconWidget = widget.GetComponent<IconWidget>();
            
            iconWidget.SetIcon(Icons[iconName]);
            SetWidgetPosition(iconWidget, position);

            return iconWidget;
        }
        
        public ToastrWidget CreateToastrWidget(string message, float duration, WidgetPosition position = WidgetPosition.Center, string name = "new ToastrWidget")
        {
            var widget = GameObject.Instantiate(toastrWidgetPrefab);
            widget.name = name;
            var toastrWidget = widget.GetComponent<ToastrWidget>();
            
            toastrWidget.SetMessage(message);
            toastrWidget.SetDuration(duration);
            SetWidgetPosition(toastrWidget, position);

            return toastrWidget;
        }
        
        public TextWidget CreateTextWidget(string message, WidgetPosition position = WidgetPosition.Center, string name = "new TextWidget")
        {
            var widget = GameObject.Instantiate(toastrWidgetPrefab);
            widget.name = name;
            var textWidget = widget.GetComponent<TextWidget>();
            
            textWidget.SetMessage(message);
            SetWidgetPosition(textWidget, position);

            return textWidget;
        }

        private void SetWidgetPosition(Widget widget, WidgetPosition position)
        {
            GameObject parent = GameObject.Find("Widgets" + position);
            widget.transform.SetParent(parent.transform, false);
        }

        public void Awake()
        {
            Icons = FetchAllIcons();
        }


        /// <summary>
        /// Opens all registered icons from the resources folder.
        /// </summary>
        /// <returns>Returns a dictionary of the icons as Texture2D with their names as keys.</returns>
        private Dictionary<string, Texture2D> FetchAllIcons()
        {
            Texture2D[] iconsArray = Resources.LoadAll<Texture2D>("Icons");
            Dictionary<string, Texture2D> iconsDictionary = new Dictionary<string, Texture2D>();

            foreach (Texture2D icon in iconsArray)
            {
                iconsDictionary.Add(icon.name, icon);
            }

            return iconsDictionary;
        }
    }
    
    public enum WidgetPosition { Top, Left, Right, Center, Bottom, BottomLeft, BottomRight };
}