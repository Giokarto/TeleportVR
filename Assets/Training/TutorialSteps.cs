using System.Collections.Generic;
using UnityEngine;
using Widgets;
using System;

namespace Training
{
    public class TutorialSteps : MonoBehaviour//Singleton<TutorialSteps>
    {
        public enum TrainingStep
        {
            IDLE,
            HEAD,
            LEFT_ARM,
            LEFT_HAND,
            RIGHT_ARM,
            RIGHT_HAND,
            WHEELCHAIR,
            DONE
        }

        public static TutorialSteps Instance;

        public TrainingStep currentStep
        {
            get { return stateMachine.State; }
            set { stateMachine.State = value; }
        }
        private StateMachine<TrainingStep> stateMachine = new StateMachine<TrainingStep>();

        public AudioClips.SGTraining senseGloveAudio;
        public AudioClips.Controller controllerAudio;
        public AudioClips.DriveJoystick driveJoystickAudio;
        public AudioClips.Misc miscAudio;

        public List<AudioClip> praisePhrases = new List<AudioClip>();
        public AudioSource[] audioSourceArray;
        public AudioSource sirenAudioSource;
        public bool waitingForNod = false;
        public Calibration.HandCalibrator rightCalibrator, leftCalibrator;

        int toggle;
        double prevDuration = 0.0;
        double prevStart = 0.0;
        TrainingStep lastCorrectedAtStep = TrainingStep.IDLE;
        bool trainingStarted = false, startTraining = true;


        [SerializeField] private Transform handCollectables;

        [SerializeField] private GameObject designatedArea;


        void Start()
        {
            Debug.Log(StateManager.Instance.TimesStateVisited(StateManager.States.Training));
            // get a reference to this singleton, as scripts from other scenes are not able to do this
            _ = Instance;
            if (StateManager.Instance.TimesStateVisited(StateManager.States.Training) <= 1)
            {
                ScheduleAudioClip(miscAudio.welcome, queue: true, delay: 1.0);
                ScheduleAudioClip(miscAudio.imAria, queue: true);//, delay: 2.0);

                PublishNotification("Welcome to Teleport VR!"); //\n" +
                                                                //"Take a look around. " +
                                                                //"In the mirror you can see how you are controlling the Head of Roboy.\n" +
                                                                //"Look at the blue sphere to get started!");
                PublishNotification("I am Aria - your personal telepresence trainer.");

            }
            else
            {
                Debug.Log("Training routine skipped.");
                startTraining = false;
            }
            //currentStep = TrainingStep.RIGHT_HAND;
            //NextStep();
            //trainingStarted = false;

            #region StateDefinition

            stateMachine.onEnter[TrainingStep.HEAD] = (step) =>
            {
                ScheduleAudioClip(miscAudio.head);
                PublishNotification("Try moving your head around");
                ScheduleAudioClip(miscAudio.nod, delay: 0);
                waitingForNod = true;
            };
            //stateMachine.onExit[TrainingStep.HEAD] = () => { };
            stateMachine.onEnter[TrainingStep.LEFT_ARM] = (step) =>
            {
#if SENSEGLOVE
                ScheduleAudioClip(senseGloveAudio.leftArm, queue: true);
                ScheduleAudioClip(senseGloveAudio.leftBall, queue: true);
                PublishNotification("Move youre left arm and try to touch the blue ball");
#else
                ScheduleAudioClip(controllerAudio.leftArm, queue: true);
                ScheduleAudioClip(controllerAudio.leftBall, queue: true);
                PublishNotification("Press and hold the index trigger and try moving your left arm");
#endif
                var colTF = PlayerRig.Instance.transform.position;
                colTF.y -= 0.1f;
                colTF.z += 0.2f;
                handCollectables.transform.position = colTF;
                handCollectables.Find("HandCollectableLeft").gameObject.SetActive(true);
            };
            stateMachine.onExit[TrainingStep.LEFT_ARM] = (step) =>
            {
                handCollectables.Find("HandCollectableLeft").gameObject.SetActive(false);
            };

            stateMachine.onEnter[TrainingStep.LEFT_HAND] = (step) =>
            {
#if SENSEGLOVE
                PublishNotification("Move your left hand into the blue box");
                ScheduleAudioClip(senseGloveAudio.leftHandStart);
                leftCalibrator.OnDone(step => NextStep());
#else
                ScheduleAudioClip(controllerAudio.leftHand, queue: true, delay: 0);
                PublishNotification("Press the grip button on the side to close the hand.");
#endif
            };
            stateMachine.onExit[TrainingStep.LEFT_HAND] = (step) =>
            {
                // force stop the calibration, if not done so already
                leftCalibrator.PauseCalibration();
            };

            stateMachine.onEnter[TrainingStep.RIGHT_ARM] = (step) =>
            {
#if SENSEGLOVE
                ScheduleAudioClip(senseGloveAudio.rightArm);
                ScheduleAudioClip(senseGloveAudio.rightBall, queue: true);
                PublishNotification("Move your right arm and try to touch the blue ball");
#else
                ScheduleAudioClip(controllerAudio.rightArm);
                ScheduleAudioClip(controllerAudio.rightBall, queue: true);
                //PublishNotification("To move your arm, hold down the hand trigger on the controller with your middle finger.");
                PublishNotification("Press and hold the index trigger and try moving your right arm");
#endif
                handCollectables.Find("HandCollectableRight").gameObject.SetActive(true);
            };
            stateMachine.onExit[TrainingStep.RIGHT_ARM] = (step) =>
            {
                handCollectables.Find("HandCollectableRight").gameObject.SetActive(false);
            };

            stateMachine.onEnter[TrainingStep.RIGHT_HAND] = (step) =>
            {
#if SENSEGLOVE
                PublishNotification("Move your right hand into the blue box");
                ScheduleAudioClip(senseGloveAudio.rightHandStart);
                rightCalibrator.OnDone(step => NextStep());
#else
                ScheduleAudioClip(controllerAudio.rightHand, queue: true, delay: 0);
                PublishNotification("Press the grip button to close the hand.");
#endif
            };
            stateMachine.onExit[TrainingStep.RIGHT_HAND] = (step) =>
            {
                rightCalibrator.PauseCalibration();
            };

            stateMachine.onEnter[TrainingStep.WHEELCHAIR] = (step) =>
            {
                ScheduleAudioClip(driveJoystickAudio.drive, delay: 1);
                //ScheduleAudioClip(emergency, queue: true);

                //sirenAudioSource.PlayDelayed(25.0f);
                //sirenAudioSource.SetScheduledEndTime(AudioSettings.dspTime + 45.0f);
                //ScheduleAudioClip(portal);
                PublishNotification("Use left joystick to drive around");
            };
            //stateMachine.onExit[TrainingStep.WHEELCHAIR] = () => { };

            stateMachine.onEnter[TrainingStep.DONE] = (step) =>
            {
                ScheduleAudioClip(miscAudio.ready, delay: 3);
            };
            #endregion
        }

        public void ScheduleAudioClip(AudioClip clip, bool queue = false, double delay = 0)
        {
            var timeLeft = 0.0;
            //queue = false;
            if (IsAudioPlaying() && queue)
            {
                timeLeft = prevDuration - (AudioSettings.dspTime - prevStart);
                if (timeLeft > 0) delay = timeLeft;
            }


            if (queue) toggle = 1 - toggle;
            audioSourceArray[toggle].clip = clip;
            //if (queue)
            //    prevStart = AudioSettings.dspTime + prevDuration + delay;
            //else
            prevStart = AudioSettings.dspTime + delay;
            audioSourceArray[toggle].PlayScheduled(prevStart);

            //if (queue)
            //    audioSourceArray[toggle].PlayScheduled(AudioSettings.dspTime + prevDuration + delay);
            //else
            //    audioSourceArray[toggle].PlayScheduled(AudioSettings.dspTime + delay);
            prevDuration = (double)clip.samples / clip.frequency;

        }

        public void StopAudioClips()
        {
            foreach (var source in audioSourceArray)
            {
                source.Stop();
            }
        }

        public bool IsAudioPlaying()
        {
            foreach (var source in audioSourceArray)
            {
                if (source.isPlaying) return true;
            }
            return false;
        }


        /// <summary>
        /// Shows a message on the notification widget
        /// </summary>
        /// <param name="message">Text to display</param>
        /// <param name="duration">time in seconds to display for</param>
        /// <returns>if the given message was published, i.e. not already existing</returns>
        public static bool PublishNotification(string message, float duration = 5f)
        {
            byte[] color = new byte[] { 0x17, 0x17, 0x17, 0xff };
            ToastrWidget widget = (ToastrWidget)Manager.Instance.FindWidgetWithID(10);
            RosJsonMessage msg = RosJsonMessage.CreateToastrMessage(10, message, duration, color);

            bool isOld = false;
            foreach (var template in widget.toastrActiveQueue)
            {
                if (template.toastrMessage == message && template.toastrDuration == duration)
                {
                    isOld = true;
                    break;
                }
            }
            if (!isOld)
            {
                widget.ProcessRosMessage(msg);
            }
            // published?
            return !isOld;
        }

        public void PraiseUser()
        {
            Debug.Log("Praise");
            ScheduleAudioClip(praisePhrases[UnityEngine.Random.Range(0, praisePhrases.Count)]);
        }

        public void CorrectUser(string correctButton)
        {
            AudioClip audio;
            switch (correctButton)
            {
                case "tigger":
                    audio = miscAudio.wrongTrigger;
                    break;
                case "grip":
                    audio = miscAudio.wrongGrip;
                    break;
                default:
                    audio = miscAudio.wrongButton;
                    break;
            }
            Debug.Log("Correcting User");
            if (lastCorrectedAtStep != currentStep && (currentStep == TrainingStep.LEFT_ARM || currentStep == TrainingStep.RIGHT_ARM))
            {
                ScheduleAudioClip(miscAudio.wrongTrigger);
                lastCorrectedAtStep = currentStep;
            }
        }

        /// <summary>
        /// Continues to the next step in the Tutorial
        /// </summary>
        /// <param name="praise"></param>
        public void NextStep(bool praise = false)
        {
            //if (praise)
            //    PraiseUser();
            currentStep++;
            Debug.Log("current tutorial step: " + currentStep);
        }


        void Update()
        {
            if (startTraining && !IsAudioPlaying() && currentStep == TrainingStep.IDLE)
            {
                currentStep = TrainingStep.IDLE;
                Debug.Log("Started Training");
                NextStep();
                startTraining = false;
                //trainingStarted = true;
            }
            if (currentStep == TrainingStep.DONE && !IsAudioPlaying())
                StateManager.Instance.GoToState(StateManager.States.HUD);
            //if (currentStep == TrainingStep.HEAD && !isAudioPlaying())
            //    waitingForNod = true;

            // allows to continue to the next step when pressing 'n'
            if (Input.GetKeyDown(KeyCode.N))
            {
                NextStep();
            }
        }
    }


}
