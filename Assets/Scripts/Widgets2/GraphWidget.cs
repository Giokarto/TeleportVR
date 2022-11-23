using System;
using System.Collections.Generic;
using UnityEngine;

namespace Widgets2
{
    public class GraphWidget : Widget
    {
        public readonly int SIZE = 100;

        public Color color = Color.white;

        public int numXLabels = 2;
        public int numYLabels = 3;

        public bool showCompleteHistory;

        public List<Datapoint> datapoints = new List<Datapoint>();

        public static GraphWidget CreateGraphWidget(string name)
        {
            GameObject gameObject = new GameObject(name, typeof(GraphWidget));
            var gw = gameObject.GetComponent<GraphWidget>();
            gw.color = Color.black;
            WidgetManager.Instance.RegisterWidget(gw);
            return gw;
        }

        /// <summary>
        /// the necessary data needed for a point on the graph
        /// </summary>
        public struct Datapoint
        {
            public DateTime time;
            public float data;

            public Datapoint(DateTime time, float newDatapoint)
            {
                this.time = time;
                this.data = newDatapoint;
            }
        }
    }
}