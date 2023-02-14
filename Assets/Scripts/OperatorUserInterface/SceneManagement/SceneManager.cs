using System;
using System.Collections.Generic;
using InputDevices;
using ServerConnection;
using UnityEngine;
using UnityEngine.SceneManagement;
using Widgets;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

namespace OperatorUserInterface
{
    public enum Scene
    {
        MAIN, // contains only Roboy and necessary control elements
        TRAINING, // training scene with mirror to get used to the simulation; tutorial
    }

    /// <summary>
    /// This class loads the required scenes and takes care of the transition.
    ///
    /// SceneManager is supposed to be inside the Main scene. On init, it marks all objects from the Main scene
    /// as persistent and immediately loads the Lobby scene (previously Training).
    ///
    /// After init, it switches between Lobby and Real when the user asks.
    /// </summary>
    public class SceneManager : Singleton<SceneManager>
    {
        public Scene currentScene { get; private set; }

        private Dictionary<Scene, String> sceneNames = new Dictionary<Scene, string>()
        {
            { Scene.MAIN, "Main" },
            { Scene.TRAINING, "Training" }
        };

        public float lastSwitch { get; private set; } = float.NegativeInfinity;

        private bool realAlreadyVisited = false;

        [SerializeField] private ServerData serverConnection;

        private AudioSource transitionAudioPlayer;

        private BioIK.BioIK[] bioIks;

        private void Start()
        {
            serverConnection = ServerData.Instance;
            transitionAudioPlayer = GetComponent<AudioSource>();

            bioIks = FindObjectsOfType<BioIK.BioIK>();

            foreach (var GO in UnitySceneManager.GetActiveScene().GetRootGameObjects())
            {
                // Not required if we load the scene additively, but no harm to keep it.
                DontDestroyOnLoad(GO);
            }

            currentScene = Scene.MAIN;
            LoadScene(Scene.TRAINING, false);
        }

        private void OnEnable()
        {
            InputSystem.OnLeftMenuButton += SwitchScene;
        }

        private void OnDisable()
        {
            InputSystem.OnLeftMenuButton -= SwitchScene;
        }

        public void SwitchScene()
        {
            switch (currentScene)
            {
                case Scene.TRAINING:
                    LoadScene(Scene.MAIN);
                    break;
                case Scene.MAIN:
                    LoadScene(Scene.TRAINING);
                    break;
                default:
                    Debug.LogError($"Trying to switch from an unknown scene: {currentScene}.");
                    break;
            }
        }

        private void LoadScene(Scene scene, bool playAudio = true)
        {
            if (Time.time - lastSwitch < 1) // don't load scene if the last change was less than 1 second ago
            {
                return;
            }
            
            lastSwitch = Time.time;

            Debug.Log($"Loading scene {sceneNames[scene]}");
            switch (scene)
            {
                case Scene.TRAINING:
                    serverConnection.EmbodyRoboy(false);
                    UnitySceneManager.LoadSceneAsync(sceneNames[scene], LoadSceneMode.Additive);
                    break;
                case Scene.MAIN:
                    if (!serverConnection.ConnectedToServer)
                    {
                        Debug.Log("Couldn't switch to real world because the server connection is not established.");
                        WidgetFactory.Instance.CreateToastrWidget("Could not connect to server!", 3, "noConnection");
                        return;
                    }

                    realAlreadyVisited = true;
                    ResetRobody();
                    serverConnection.EmbodyRoboy(true);

                    UnitySceneManager.UnloadSceneAsync(sceneNames[Scene.TRAINING]);
                    break;
            }

            if (playAudio)
            {
                transitionAudioPlayer.Play();
            }

            currentScene = scene;
        }

        /// <summary>
        /// Reset all joints to 0 before going to the real world.
        /// TODO: Move this peace of code somewhere else.
        /// </summary>
        private void ResetRobody()
        {
            if (!realAlreadyVisited)
            {
                Debug.Log("resetting robot body");
                foreach (var body in bioIks)
                {
                    // switch to instantaneous movement type for BioIK so that the transition to joint targets 0 is immediate 
                    body.MotionType = BioIK.MotionType.Instantaneous;

                    foreach (var segment in body.Segments)
                    {
                        body.ResetPosture(segment);
                    }
                }
            }
            else
            {
                // go to the latest joint targets before leaving HUD
                var jointValues = serverConnection.GetLatestJointValues();
                if (jointValues.Count > 0)
                {
                    int i = 0;
                    foreach (var body in bioIks)
                    {
                        if (body.name.Contains("shadow"))
                        {
                            continue;
                        }

                        foreach (var segment in body.Segments)
                        {
                            if (segment.Joint != null)
                            {
                                //Debug.Log($"{body.name}: {segment.Joint.name} {i}");
                                segment.Joint.X.SetTargetValue(jointValues[i]);

                                i++;
                            }
                        }
                    }
                }
            }
        }
    }
}