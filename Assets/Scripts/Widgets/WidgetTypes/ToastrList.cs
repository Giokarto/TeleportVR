using System;
using System.Collections.Generic;
using UnityEngine;

namespace Widgets
{
    public class ToastrList : Singleton<ToastrList>
    {
        // These are the toastr that are shown in HUD scene already.
        public Queue<ToastrWidget> toastrQueue;

        private Timer topToastrTimer;
        
        public readonly int OFFSET = 110;

        /// <summary>
        /// Initialize queues first, to avoid null pointer exception in Update().
        /// </summary>
        private void Awake()
        {
            toastrQueue = new Queue<ToastrWidget>();
            topToastrTimer = new Timer();
        }

        private void Update()
        {
            if (toastrQueue.Count != 0)
            {
                topToastrTimer.LetTimePass(Time.deltaTime);
            }
        }

        /// <summary>
        /// Adds a toastr to the queue of shown toastrs. It will be shown until it gets to the top of the queue
        /// and then the timer starts for the toastr duration.
        /// </summary>
        /// <param name="toastr"></param>
        public void EnqueueToastr(ToastrWidget toastr)
        {
            toastrQueue.Enqueue(toastr);
            
            if (toastrQueue.Count == 1)
            {
                topToastrTimer.SetTimer(toastr.duration, DestroyTopToastr);
            }
        }

        /// <summary>
        /// Destroy top toastr and move all other toastr up.
        /// </summary>
        public void DestroyTopToastr()
        {
            Destroy(toastrQueue.Dequeue().gameObject);
            MoveToastrsUp();
            
            if (toastrQueue.Count >= 1)
            {
                topToastrTimer.SetTimer(toastrQueue.Peek().duration, DestroyTopToastr);
            }
        }

        /// <summary>
        /// Slerp all toastr upwards with delay according to their position.
        /// </summary>
        private void MoveToastrsUp()
        {
            float offsetTime = 0;

            foreach (ToastrWidget toastr in toastrQueue)
            {
                offsetTime += 0.1f;
                toastr.SlerpUp(OFFSET, offsetTime);
            }
        }
    }
}