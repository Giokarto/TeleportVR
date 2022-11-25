using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Widgets
{
    public class IconWidget : Widget
    {
        public Texture2D[] iconsArray;
   
        private RawImage image;

        public void SetIcon(Texture2D icon)
        {
            image.texture = icon;
        }

        public void SetIcon(string iconName)
        {
            image.texture = WidgetFactory.Instance.Icons[iconName];
        }

        public void Awake()
        {
            image = gameObject.GetComponentInChildren<RawImage>();
        }
    }
}