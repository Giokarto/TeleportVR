using ServerConnection;

namespace InputDevices
{
    /// <summary>
    /// This class should abstract away the input devices.
    ///
    /// Input device controllers should call the methods if a corresponding button combination is pressed.
    /// They can also subscribe with a delegate to send haptic feedback.
    ///
    /// TODO: maybe extract another class - Roboy model which holds the current state of emotions, joint positions, etc?
    /// also all the computations in <see cref="AnimusClient.UnityAnimusClient"/> in motor_get, Animus shouldn't depend on BioIK
    /// </summary>
    public class UserInteractionManager
    {
        private IServerData serverData;
        public void SendEmotion(string emotion)
        {
            serverData.SetEmotion(emotion);
            //    // Display the current Emotion on the widget
            //    EmotionManager.Instance.SetFaceByKey(currentEmotion);
        }

        public void ChangeGrip(float left, float right)
        {
            serverData.ChangeGrip(left, right);
            
            /*if (InputManager.Instance.GetLeftController())
                InputManager.Instance.controllerLeft[0]
                    .TryGetFeatureValue(UnityEngine.XR.CommonUsages.grip, out left);*/
        }
    }
}