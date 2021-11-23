using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TelemedicineTraining : MonoBehaviour
{
    public Training.AudioManager audioManager;
    public List<AudioClip> audioClips;
    private bool startTraining;
    private TelemedicineTrainingStep currentStep = TelemedicineTrainingStep.IDLE;
    private static  Dictionary<UserActionType, bool> userActionComplete;
    [SerializeField] GameObject VitalsMonitor;
    [SerializeField] GameObject NormalHeartSphere, AbnormalHeartSphere, NormalLungSphere;
    public enum TelemedicineTrainingStep
    {
        IDLE = -1,
        BEATRICEHOME,
        HOWTOUSE,
        VITALSIGNS,
        HR,
        BP,
        SP02,
        T,
        STETHOSCOPE,
        HEARTSOUND,
        LIKETHAT,
        NORMALHEART,
        NICEJOB,
        ABNORMALHEART,
        LUNGSOUND,
        THATSRIGHT,
        NORMALBREATHING,
        GREAT,
        COMPELETE
    }

    public enum UserActionType
    {
        DRIVETOBED,
        TOUCHHEART,
        TOUCHHEART2,
        TOUCHLUNG
    }

    private IEnumerator WaitForSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);

    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(WaitForSeconds(3));
        if (StateManager.Instance.TimesStateVisited(StateManager.States.AmbulantCare) <= 1)
        {
            startTraining = true;
            //currentStep = TelemedicineTrainingStep.VITALSIGNS;
            NextStep();
            //currentStep++;
        }
        else
        {
            startTraining = false;
        }
        userActionComplete = new Dictionary<UserActionType, bool>();
        userActionComplete[UserActionType.DRIVETOBED] = true;
        userActionComplete[UserActionType.TOUCHHEART] = true;
        userActionComplete[UserActionType.TOUCHLUNG] = true;

        VitalsMonitor.SetActive(false);
    }

    void Highlight(string tag, bool on=true)
    {
        var ms = VitalsMonitor.GetComponentsInChildren<Image>().Where(r => r.CompareTag(tag)).ToArray();// (r => r.tag == tag).ToList<Image>(); //;
        foreach (var m in ms)
        {
            var c = m.color;
            if (on)
            {
                c.g = 160; //color.g = 160;
            }
            else
            {
                c.g = 0;
            }

            //c.g = 160;
            m.color = c;
            //yield return (new WaitForSeconds(2f));
            //m.color = Color.black;
        }


    }

        void NextStep()
    {
        currentStep++;
        Debug.Log("Current training step: " + currentStep.ToString());
        switch (currentStep)
        {
            case TelemedicineTrainingStep.HOWTOUSE:
                userActionComplete[UserActionType.DRIVETOBED] = false;
                // wait to arrive in designated area
                break;
            case TelemedicineTrainingStep.VITALSIGNS:
                VitalsMonitor.SetActive(true);
                // pop monitor
                break;
            case TelemedicineTrainingStep.HR:
                Highlight("HR");
                Highlight("ECG");
                // highlight HR
                //audioManager.ScheduleAudioClip(audioClips[(int)currentStep], queue: true);
                break;
            case TelemedicineTrainingStep.BP:
                //highlight BP
                Highlight("ECG", false);
                Highlight("HR", false);
                Highlight("BP");
                //audioManager.ScheduleAudioClip(audioClips[(int)currentStep], queue: true);
                break;
            case TelemedicineTrainingStep.SP02:
                // highlight SPO2
                Highlight("BP", false);
                Highlight("SPO2");
                //audioManager.ScheduleAudioClip(audioClips[(int)currentStep], queue: true);
                break;
            case TelemedicineTrainingStep.T:
                Highlight("SPO2", false);
                Highlight("T");
                // highlight T
                break;
            case TelemedicineTrainingStep.STETHOSCOPE:
                Highlight("T", false);
                
                break;
            case TelemedicineTrainingStep.HEARTSOUND:
                // pop 1st heart sphere
                NormalHeartSphere.SetActive(true);
                userActionComplete[UserActionType.TOUCHHEART] = false;
                InputManager.Instance.GetController(false).SendHapticImpulse(0, 0.3f);
                // vibrate right
                break;
            case TelemedicineTrainingStep.ABNORMALHEART:
                // remove 1st, pop 2nd heart sphere
                NormalHeartSphere.SetActive(false);
                AbnormalHeartSphere.SetActive(true);
                break;
            case TelemedicineTrainingStep.LUNGSOUND:
                // remove 2nd heart, pop up lungs sphere
                AbnormalHeartSphere.SetActive(false);
                NormalLungSphere.SetActive(true);
                userActionComplete[UserActionType.TOUCHLUNG] = false;
                break;
            case TelemedicineTrainingStep.THATSRIGHT:
                // waiti for occlusion  with lung sphere
                break;
            case TelemedicineTrainingStep.LIKETHAT:
            // wait for occlusion  with 1st heart sphere
            case TelemedicineTrainingStep.COMPELETE:
                NormalLungSphere.SetActive(false);
                break;
            default:
                //audioManager.ScheduleAudioClip(audioClips[(int)currentStep], queue: true);
                break;

        }
        audioManager.ScheduleAudioClip(audioClips[(int)currentStep], queue: true);
    }

    public static void MarkUserActionComplete(UserActionType actionType)
    {
        userActionComplete[actionType] = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!audioManager.IsAudioPlaying() && !userActionComplete.ContainsValue(false))
        {
            NextStep();
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            NextStep();
        }
    }
}
