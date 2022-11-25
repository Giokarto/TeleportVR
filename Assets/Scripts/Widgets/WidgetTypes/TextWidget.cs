using UnityEngine;

namespace Widgets
{
    public class TextWidget : Widget
    {
        public Color color;
        public int fontSize;

        public string currentlyDisplayedMessage;
        public Color textColor;
        public int textFontSize;

        public void SetMessage(string message)
        {
            currentlyDisplayedMessage = message;
        }
    }
}