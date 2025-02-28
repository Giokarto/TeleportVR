using System.Collections.Generic;
using UnityEngine;

namespace Tutorial
{
    public class Automaton : MonoBehaviour
    {
        protected List<Step> Steps;

        protected int currentStepIndex;
        protected bool running;

        public AudioManager audioManager;
        public MiscAudio miscAudio;
        public ControllerAudio controllerAudio;

        protected void Start()
        {
            foreach (var step in Steps)
            {
                step.audioManager = audioManager;
                step.miscAudio = miscAudio;
                step.controllerAudio = controllerAudio;

                step.Next += Next;
            }
        }

        public void StartAutomaton()
        {
            if (!running)
            {
                currentStepIndex = 0;
                Steps[currentStepIndex].Enter();
                running = true;
            }
        }

        public void CancelAutomaton()
        {
            Steps[currentStepIndex].Break();
        }

        public void ResumeAutomaton()
        {
            Steps[currentStepIndex].Continue();
        }

        private void Next()
        {
            Steps[currentStepIndex].Exit();

            if (Steps.Count > currentStepIndex + 1)
            {
                Debug.Log($"Transition {Steps[currentStepIndex]} -> {Steps[currentStepIndex + 1]}");
                currentStepIndex += 1;
                Steps[currentStepIndex].Enter();
            }

            else
            {
                running = false;
            }
        }
    }
}