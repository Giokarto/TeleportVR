using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using JointStateMsg = RosMessageTypes.Sensor.JointStateMsg;

public class RosFaceExpressionsPublisher : MonoBehaviour
{
    ROSConnection ros;
    public string topicName = "/operator/face/expressions";
    public float publishMessageFrequency =1f;

    float timeElapsed;
    JointStateMsg msg;

    OVRFaceExpressions faceExpressions;
    List<float> faceExpressionsList;
    int length;

    // Start is called before the first frame update
    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<JointStateMsg>(topicName);

        msg = new JointStateMsg();
        msg.header = new RosMessageTypes.Std.HeaderMsg();

        length = OVRtoCC.Count;  //System.Enum.GetNames(typeof(OVRFaceExpressions.FaceExpression)).Length;
        Debug.Log($"length: {length}");
        faceExpressionsList = new List<float>(new float[length]);
        msg.name = new string[length];
        msg.position = new double[length];

        int i = 0;
        foreach (OVRFaceExpressions.FaceExpression e in OVRtoCC.Keys)
        {
            msg.name[i] = OVRtoCC[e].ToString();
            i++;
        }


        //var fe = System.Enum.GetValues(typeof(OVRFaceExpressions.FaceExpression));
        //Debug.Log($"values: {fe}");
        //for (int i = 0; i < length; i++)
        //{
        //    Debug.Log(fe.GetValue(i).ToString());
        //    var e = fe.GetValue(i).ToString();

        //    msg.name[i] = "";// CCBlendshapeMap[fe.GetValue(i).ToString()];
        //}
        //Debug.Log($"face names: {string.Join("}, "}, msg.name)}");
        //Debug.Log($"face names: {msg.name}");

        faceExpressions = GetComponent<OVRFaceExpressions>();

    }

    // Update is called once per frame
    void Update()
    {
        timeElapsed += Time.deltaTime;

        if (timeElapsed > publishMessageFrequency) // && VRControllerInputSystem.IsUserActive())
        {
            ros.Publish(topicName, GetLatestFaceExpressions());
            timeElapsed = 0;
        }
    }

    JointStateMsg GetLatestFaceExpressions()
    {
        int i = 0;
        foreach (OVRFaceExpressions.FaceExpression e in OVRtoCC.Keys)
        {
            float weight;
            faceExpressions.TryGetFaceExpressionWeight(e, out weight);
            msg.position[i] = weight * 75.0;
            if (e == OVRFaceExpressions.FaceExpression.EyesClosedL || e == OVRFaceExpressions.FaceExpression.EyesClosedR)
            {
                msg.position[i] = weight * 60.0;
            } else
            {
                msg.position[i] = weight * 75.0;
            }
            i += 1;
        }
        
        //for (int i=0; i<length; i++)
        //{
        //    float weight;
        //    faceExpressions.TryGetFaceExpressionWeight((OVRFaceExpressions.FaceExpression)i, out weight);
        //    var e = (OVRFaceExpressions.FaceExpression)i;
        //    if (CCBlendshapeMap.ContainsKey(e.ToString())) 
        //    {
        //        Debug.Log($"{((OVRFaceExpressions.FaceExpression)i).ToString()}");
        //        msg.name[i] = CCBlendshapeMap[((OVRFaceExpressions.FaceExpression)i).ToString()];
        //        msg.position[i] = weight;
        //    }
            
        //}
        //Debug.Log($"face weights:  {string.Join("}, "}, msg.position)}");
        return msg;
    }

    Dictionary<OVRFaceExpressions.FaceExpression, int> OVRtoCC = new Dictionary<OVRFaceExpressions.FaceExpression, int>
    {
        {OVRFaceExpressions.FaceExpression.InnerBrowRaiserL, 88},
        {OVRFaceExpressions.FaceExpression.BrowLowererL, 89},
        {OVRFaceExpressions.FaceExpression.BrowLowererR, 90},
        {OVRFaceExpressions.FaceExpression.OuterBrowRaiserL, 91},
        {OVRFaceExpressions.FaceExpression.OuterBrowRaiserR, 92},
        {OVRFaceExpressions.FaceExpression.EyesLookUpL, 93},
        {OVRFaceExpressions.FaceExpression.EyesLookUpR, 94},
        {OVRFaceExpressions.FaceExpression.EyesLookDownL, 95},
        {OVRFaceExpressions.FaceExpression.EyesLookDownR, 96},
        {OVRFaceExpressions.FaceExpression.EyesLookLeftL, 97},
        {OVRFaceExpressions.FaceExpression.EyesLookRightL, 98},
        {OVRFaceExpressions.FaceExpression.EyesLookLeftR, 99},
        {OVRFaceExpressions.FaceExpression.EyesLookRightR, 100},
        {OVRFaceExpressions.FaceExpression.EyesClosedL, 101},
        {OVRFaceExpressions.FaceExpression.EyesClosedR, 102},
        {OVRFaceExpressions.FaceExpression.LidTightenerL, 103},
        {OVRFaceExpressions.FaceExpression.LidTightenerR, 104},
        {OVRFaceExpressions.FaceExpression.UpperLidRaiserL, 105},
        {OVRFaceExpressions.FaceExpression.UpperLidRaiserR, 106},
        {OVRFaceExpressions.FaceExpression.CheekPuffL, 107},
        {OVRFaceExpressions.FaceExpression.CheekRaiserL, 108},
        {OVRFaceExpressions.FaceExpression.CheekRaiserR, 109},
        {OVRFaceExpressions.FaceExpression.NoseWrinklerL, 110},
        {OVRFaceExpressions.FaceExpression.NoseWrinklerR, 111},
        {OVRFaceExpressions.FaceExpression.JawDrop, 19},
        {OVRFaceExpressions.FaceExpression.JawThrust, 113},
        {OVRFaceExpressions.FaceExpression.JawSidewaysLeft, 114},
        {OVRFaceExpressions.FaceExpression.JawSidewaysRight, 115},
        {OVRFaceExpressions.FaceExpression.LipFunnelerLB, 116},
        {OVRFaceExpressions.FaceExpression.LipPuckerL, 117},
        {OVRFaceExpressions.FaceExpression.MouthLeft, 118},
        {OVRFaceExpressions.FaceExpression.MouthRight, 119},
        {OVRFaceExpressions.FaceExpression.LipSuckLT, 120},
        {OVRFaceExpressions.FaceExpression.LipSuckLB, 121},
        {OVRFaceExpressions.FaceExpression.ChinRaiserT, 122},
        {OVRFaceExpressions.FaceExpression.ChinRaiserB, 123},
        {OVRFaceExpressions.FaceExpression.LipsToward, 124},
        {OVRFaceExpressions.FaceExpression.LipCornerPullerL, 125},
        {OVRFaceExpressions.FaceExpression.LipCornerPullerR, 126},
        {OVRFaceExpressions.FaceExpression.LipCornerDepressorL, 127},
        {OVRFaceExpressions.FaceExpression.LipCornerDepressorR, 128},
        {OVRFaceExpressions.FaceExpression.DimplerL, 129},
        {OVRFaceExpressions.FaceExpression.DimplerR, 130},
        {OVRFaceExpressions.FaceExpression.UpperLipRaiserL, 131},
        {OVRFaceExpressions.FaceExpression.UpperLipRaiserR, 132},
        {OVRFaceExpressions.FaceExpression.LowerLipDepressorL, 133},
        {OVRFaceExpressions.FaceExpression.LowerLipDepressorR, 134},
        {OVRFaceExpressions.FaceExpression.LipPressorL, 135},
        {OVRFaceExpressions.FaceExpression.LipPressorR, 136},
        {OVRFaceExpressions.FaceExpression.LipStretcherL, 137},
        {OVRFaceExpressions.FaceExpression.LipStretcherR, 138}
    };

    Dictionary<string, string> CCBlendshapeMap = new Dictionary<string, string>
    {
        {"EyesClosedL","Eye_Blink_L"},
        {"LidTightenerL","Eye_Squint_L"},
        {"UpperLidRaiserL","Eye_Wide_L"},
        {"EyesClosedR","Eye_Blink_R"},
        {"LidTightenerR","Eye_Squint_R"},
        {"UpperLidRaiserR","Eye_Wide_R"},
        {"LipsToward","Lip_Open"},
        {"LipFunnelerLB","Mouth_Blow"},
        {"LipPuckerL","Mouth_Pucker"},
        {"MouthLeft","Mouth_L"},
        {"MouthRight","Mouth_R"},
        {"LipCornerPullerL","Mouth_Smile_L"},
        {"LipCornerPullerR","Mouth_Smile_R"},
        {"LipCornerDepressorL","Mouth_Frown_L"},
        {"LipCornerDepressorR","Mouth_Frown_R"},
        {"DimplerL","Mouth_Dimple_L"},
        {"DimplerR","Mouth_Dimple_R"},
        {"LowerLipDepressorL","Mouth_Snarl_Upper_L"},
        {"LowerLipDepressorR","Mouth_Snarl_Upper_R"},
        {"UpperLipRaiserL","Mouth_Snarl_Lower_L"},
        {"UpperLipRaiserR","Mouth_Snarl_Lower_R"},
        {"BrowLowererL","Brow_Raise_Inner_L"},
        {"BrowLowererR","Brow_Raise_Inner_R"},
        {"OuterBrowRaiserL","Brow_Raise_Outer_L"},
        {"OuterBrowRaiserR","Brow_Raise_Outer_R"},
        {"CheekPuffL","Cheek_Suck"},
        {"CheekRaiserL","Cheek_Raise_L"},
        {"CheekRaiserR","Cheek_Raise_R"},
        {"NoseWrinklerL","Nose_Flank_Raise_L"},
        {"NoseWrinklerR","Nose_Flank_Raise_R"},
    };
}
