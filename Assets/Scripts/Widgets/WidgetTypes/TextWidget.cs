using System;
using TMPro;
using UnityEngine;

namespace Widgets
{
    public class TextWidget : Widget
    {
        public Color textColor;
        public int textFontSize;
        
        TextMeshProUGUI textMeshPro;

        public void SetMessage(string message)
        {
            textMeshPro.text = message;
        }

        public void Awake()
        {
            textMeshPro = gameObject.GetComponentInChildren<TextMeshProUGUI>();
        }
    }
}