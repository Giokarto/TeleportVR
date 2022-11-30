using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Widgets
{
    public class IconWidget : Widget
    {
        private RawImage image;
        
        public static Dictionary<string, Texture2D> Icons;

        public void SetIcon(Texture2D icon)
        {
            image.texture = icon;
        }

        public void SetIcon(string iconName)
        {
            if (!Icons.Keys.Contains(iconName))
            {
                Debug.LogError($"Icon {iconName} not found!");
            }
            else
            {
                image.texture = Icons[iconName];
            }
        }

        public void Awake()
        {
            if (Icons == null)
            {
                Icons = FetchAllIcons();
            }
            
            image = gameObject.GetComponentInChildren<RawImage>();
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
}