using UnityEngine;

namespace Training.AudioClips
{
    [System.Serializable]
    public struct Controller
    {
        public AudioClip leftArm, leftBall,
            rightArm, rightBall, leftHand, rightHand;
    }
    [System.Serializable]
    public struct SGTraining
    {
        public AudioClip leftArm, leftBall,
            leftHandStart, rightHandStart,
            rightArm, rightBall;
    }

    [System.Serializable]
    public struct SGHand
    {
        public AudioClip handOpen, handClosed, fingersExt, fingersFlexed,
            thumbUp, thumbFlex, abdOut, noThumbAbd, test, tooLargeChange;
    }

    [System.Serializable]
    public struct DriveWheelcair
    {
        public AudioClip start, forward, backwards, turn_left, turn_right;
    }

    [System.Serializable]
    public struct PauseMenu
    {
        public AudioClip start, paused, unpause, teleport;
    }

    [System.Serializable]
    public struct Misc
    {
        public AudioClip welcome, imAria, head, nod, wrongTrigger, portal, enterButton,
            emergency, wrongGrip, wrongButton, siren, ready;
    }

}
