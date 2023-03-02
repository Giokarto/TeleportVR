using System.Collections;
using Animus.RobotProto;
using AnimusManager;
using UnityEngine;

namespace AnimusClient
{
    /// <summary>
    /// <remarks>
    /// This class needs to stay in the Example folder of the AnimusClient package.
    /// See explanation in <see cref="UnityAnimusClient"/>.
    /// </remarks>
    ///
    /// This class interacts with AnimusClientManager to establish the connection to the robot.
    /// </summary>
    public class ClientLogic : Singleton<ClientLogic>
    {
        public AnimusClientManager AnimusManager;
        public UnityAnimusClient unityClient;

        public string robotName;
        public string AccountEmail;
        public string AccountPassword;

        public string[] requiredModalities = new string[] { "vision" };

        public Robot ChosenRobot;
        private int _count;

        void Awake()
        {
        }

        // Start is called before the first frame update
        void Start()
        {
            _count = 0;
            StartCoroutine(ClientManagerLogic());
        }

        IEnumerator ClientManagerLogic()
        {

            // Yield for 10 frames to allow other scripts to initialise first
            if (_count < 10)
            {
                _count++;
                yield return null;
            }

            // Step 1 - Login user
            AnimusManager.LoginUser(AccountEmail, AccountPassword);
            while (!AnimusManager.loginResultAvailable)
            {
                yield return null;
            }

            if (!AnimusManager.loginSuccess)
            {
                yield break;
            }

            Debug.Log("Login successful.");

            // Step 2 - Search for connectable robots
            while (true)
            {
                AnimusManager.SearchRobots();
                while (!AnimusManager.searchResultsAvailable)
                {
                    yield return null;
                }

                if (AnimusManager.searchReturn == "Cannot search more than once per second")
                {
                    yield return new WaitForSeconds(2);
                    continue;
                }
                else if (AnimusManager.searchReturn.Contains("Error"))
                {
                    yield return new WaitForSeconds(1);
                    continue;
                }

                if (!AnimusManager.searchSuccess) yield break;
                break;
            }

            // Step 3 - Choose Robot
            foreach (var robot in AnimusManager.robotDetailsList)
            {
                if (robot.Name == robotName)
                {
                    ChosenRobot = robot;
                }
            }

            if (ChosenRobot == null)
            {
                Debug.Log($"Robot {robotName} not found");

                //WidgetInteraction.SetAnimusStatus("WifiRed", $"Robot {robotName} not found");
                yield break;
            }

            Debug.Log($"Found robot {robotName}");

            //Step 4 - Send AnimusManager the interface it should use for this connection
            AnimusManager.SetClientClass(unityClient);

            // Step 5 - Connect to the robot
            AnimusManager.StartRobotConnection(ChosenRobot);
            while (!AnimusManager.connectToRobotFinished)
            {
                yield return null;
            }

            if (!AnimusManager.connectedToRobotSuccess) yield break;



            // Step 5 - Starting all modalities
            AnimusManager.OpenModalities(requiredModalities);
        }
    }
}