using System;
using UnityEngine;

namespace Training
{
    public abstract class Step
    {
        public AudioManager audioManager;
        public AudioClips.Misc miscAudio;
        public AudioClips.Controller controllerAudio;
        
        /// <summary>
        /// Called when the step is started.
        /// </summary>
        public abstract void Enter();
        
        /// <summary>
        /// Called when the step is finished.
        /// </summary>
        public abstract void Exit();
        
        /// <summary>
        /// Called when the tutorial is paused or cancelled.
        /// </summary>
        public abstract void Break();
        
        /// <summary>
        /// Called when the tutorial is resumed.
        /// </summary>
        public abstract void Continue();

        /// <summary>
        /// Transition to the next state. Gets called by the steps themselves. Needs to be assigned from Tutorial.
        /// </summary>
        public event Action Next;
        protected void NextStep() { Next.Invoke(); }

        protected bool correctedAtThisStep = false;
        public void CorrectUser(string correctButton)
        {
            AudioClip audio;
            switch (correctButton)
            {
                case "trigger":
                    audio = miscAudio.wrongTrigger;
                    break;
                case "grip":
                    audio = miscAudio.wrongGrip;
                    break;
                default:
                    audio = miscAudio.wrongButton;
                    break;
            }

            if (!correctedAtThisStep)
            {
                Debug.Log("Correcting User");
                audioManager.ScheduleAudioClip(audio);
                correctedAtThisStep = true;
            }
        }
    }
}