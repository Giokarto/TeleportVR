using System;
using System.Collections.Generic;
using ServerConnection;
using UnityEngine;
using Widgets;
using OperatorUI;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

namespace OperatorUserInterface
{
    public enum Scene
    {
        MAIN, // contains only Roboy and necessary control elements, otherwise empty
        LOBBY, // training scene with mirror to get used to the simulation; tutorial
        REAL // real world scene, streaming camera from Roboy
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
            { Scene.LOBBY, "Training" },
            { Scene.REAL, "HUD" }
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
                DontDestroyOnLoad(GO);
            }

            currentScene = Scene.MAIN;
            LoadScene(Scene.LOBBY);
        }

        public void SwitchScene()
        {
            switch (currentScene)
            {
                case Scene.LOBBY:
                    LoadScene(Scene.REAL);
                    break;
                case Scene.REAL:
                    LoadScene(Scene.LOBBY);
                    break;
                default:
                    Debug.LogError($"Trying to switch from the scene {currentScene}. This is not allowed.");
                    break;
            }
        }

        private void LoadScene(Scene scene)
        {
            lastSwitch = Time.time;

            switch (scene)
            {
                case Scene.MAIN:
                    Debug.LogError("Not allowed to return back to empty main scene");
                    return;
                case Scene.LOBBY:
                    serverConnection.SetMotorOn(false);
                    serverConnection.SetPresenceIndicatorOn(false);
                    break;
                case Scene.REAL:
                    if (!serverConnection.ConnectedToServer)
                    {
                        Debug.Log("Couldn't switch to real world because the server connection is not established.");
                        WidgetFactory.Instance.CreateToastrWidget("Could not connect to server!", 3, "noConnection");
                        return;
                    }

                    realAlreadyVisited = true;
                    ResetRobody();
                    serverConnection.SetMotorOn(true);
                    serverConnection.SetPresenceIndicatorOn(true);
                    break;
            }

            if (currentScene != Scene.MAIN)
            {
                transitionAudioPlayer.Play();
            }

            Debug.Log($"Loading scene {sceneNames[scene]}");
            UnitySceneManager.LoadScene(sceneNames[scene]);

            currentScene = scene;
        }

        /// <summary>
        /// Reset all joints to 0 before going to Real. See <see cref="StateManager"/>.
        /// TODO: Instead of setting the joints here, create two Robody objects, one in Real and one in Lobby? Or at least move this peace of code somewhere else.
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