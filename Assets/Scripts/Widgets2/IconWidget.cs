using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Widgets2
{
    public class IconWidget : Widget
    {
        public Dictionary<string, Texture2D> icons;
        public Texture2D[] iconsArray;
        
        private string currentIconName;
        public Texture2D currentIcon;

        public static IconWidget CreateIconWidget(string name)
        {
            GameObject gameObject = new GameObject(name, typeof(IconWidget));
            var widget = gameObject.GetComponent<IconWidget>();
            widget.currentIcon = Texture2D.redTexture;
            WidgetManager.Instance.RegisterWidget(widget, WidgetPosition.Top);
            return widget;
        }
    }
}