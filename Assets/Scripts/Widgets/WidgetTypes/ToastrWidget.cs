using System.Collections.Generic;
using OperatorUI;
using UnityEngine;

namespace Widgets
{
    public class ToastrWidget : Widget
    {
        // While in construct, incoming toastr are queued and wait to be shown by view in HUD scene.
        public Queue<ToastrTemplate> toastrToInstantiateQueue;

        // These are the toastr that are shown in HUD scene already.
        public Queue<ToastrTemplate> toastrActiveQueue;

        public float duration;
        public Color color;
        public int fontSize;

        private Timer timer;

        private string message;
        public void SetMessage(string message)
        {
            this.message = message;
        }
        
        public void SetDuration(float duration)
        {
            this.duration = duration;
        }

        /// <summary>
        /// Shows the message on the screen for <see cref="duration"/> time.
        /// </summary>
        public void Publish()
        {
            // TODO
        }

        /// <summary>
        /// Initialize queues first, to avoid null pointer exception in Update().
        /// </summary>
        private void Awake()
        {
            toastrToInstantiateQueue = new Queue<ToastrTemplate>();
            toastrActiveQueue = new Queue<ToastrTemplate>();
            timer = new Timer();
        }

        /// <summary>
        /// Enqueue new toastr. Use default values, if not specified.
        /// </summary>
        /// <param name="toastrMessage"></param>
        /// <param name="toastrDuration"></param>
        /// <param name="toastrColor"></param>
        /// <param name="toastrFontSize"></param>
        private void EnqueueNewMessage(string toastrMessage, float toastrDuration, Color toastrColor, int toastrFontSize)
        {
            if (toastrMessage.Equals(""))
            {
                return;
            }

            if (toastrColor == null)
            {
                toastrColor = color;
            }

            if (toastrDuration == 0.0f)
            {
                toastrDuration = duration;
            }

            if (toastrFontSize == 0)
            {
                toastrFontSize = fontSize;
            }

            toastrToInstantiateQueue.Enqueue(new ToastrTemplate(toastrMessage, toastrDuration, toastrColor, toastrFontSize));
        }
        
        /// <summary>
        /// Struct to initialize toastr by view.
        /// </summary>
        public class ToastrTemplate
        {
            public string toastrMessage;
            public Color toastrColor;
            public int toastrFontSize;
            public float toastrDuration;

            public ToastrTemplate(string toastrMessage, float toastrDuration, Color toastrColor, int toastrFontSize)
            {
                this.toastrMessage = toastrMessage;
                this.toastrColor = toastrColor;
                this.toastrFontSize = toastrFontSize;
                this.toastrDuration = toastrDuration;
            }
        }
    }
}
