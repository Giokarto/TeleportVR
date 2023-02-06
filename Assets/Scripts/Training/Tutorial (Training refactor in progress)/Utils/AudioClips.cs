using UnityEngine;

namespace Tutorial
{
    [System.Serializable]
    public struct ControllerAudio
    {
        public AudioClip leftArm, leftBall,
            rightArm, rightBall, leftHand, rightHand;
    }
    [System.Serializable]
    public struct SGTrainingAudio
    {
        public AudioClip leftArm, leftBall,
            leftHandStart, rightHandStart,
            rightArm, rightBall;
    }

    [System.Serializable]
    public struct SGHandAudio
    {
        public AudioClip handOpen, handClosed, fingersExt, fingersFlexed,
            thumbUp, thumbFlex, abdOut, noThumbAbd, test, tooLargeChange;
    }

    [System.Serializable]
    public struct RudderWheelchairAudio
    {
        public AudioClip start_intro, start, forward, backwards, turn_left, turn_right_intro, turn_right;
    }


    [System.Serializable]
    public struct JoystickWheelchairAudio
    {
        public AudioClip start_intro, howto, front, back, left, right;
    }

    [System.Serializable]
    public struct PauseMenuAudio
    {
        public AudioClip start, paused, unpause, teleport;
    }

    [System.Serializable]
    public struct ArmLengthAudio
    {
        public AudioClip start, scale_left, scale_right, touch_left, touch_right;
    }

    [System.Serializable]
    public struct MiscAudio
    {
        public AudioClip welcome, imAria, head, nod, wrongTrigger, portal, enterButton,
            emergency, wrongGrip, wrongButton, siren, ready;
    }
}
