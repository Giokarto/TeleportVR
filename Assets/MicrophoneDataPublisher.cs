using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System;

using Unity.Robotics.ROSTCPConnector;
using Int16MultiArrayMsg = RosMessageTypes.Std.Int16MultiArrayMsg;


public class MicrophoneDataPublisher : MonoBehaviour
{
    ROSConnection ros;
    public string topicName = "/audio/audio";
    public float publishMessageFrequency = 0.0001f;
    private Int16MultiArrayMsg msg;
    private float timeElapsed;

    AudioSource AudioMic;
    public string DetectedDevices;
    public MicDeviceMode DeviceMode = MicDeviceMode.Default;
    string CurrentDeviceName = null;

    public int OutputSampleRate = 11025;
    public int OutputChannels = 1;
    private object _asyncLockAudio = new object();

    private int CurrentAudioTimeSample = 0;
    private int LastAudioTimeSample = 0;

    private bool stop = false;

    private int lastSample = 0;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<Int16MultiArrayMsg>(topicName, queue_size:10);
        msg = new Int16MultiArrayMsg();

        StartCoroutine(CaptureMic());
    }

    private IEnumerator CaptureMic()
    {
        if (AudioMic == null) AudioMic = GetComponent<AudioSource>();
        if (AudioMic == null) AudioMic = gameObject.AddComponent<AudioSource>();

        //Check Target Device
        DetectedDevices = "";
        string[] MicNames = Microphone.devices;
        foreach (string _name in MicNames)
        {
            DetectedDevices += _name + "\n";
            Debug.Log($"mic: {_name}");
        }
        //Debug.Log(MicNames);
        //if (DeviceMode == MicDeviceMode.TargetDevice)
        //{
        //    bool IsCorrectName = false;
        //    for (int i = 0; i < MicNames.Length; i++)
        //    {
        //        if (MicNames[i] == TargetDeviceName)
        //        {
        //            IsCorrectName = true;
        //            break;
        //        }
        //    }
        //    if (!IsCorrectName) TargetDeviceName = null;
        //}
        //Check Target Device

        CurrentDeviceName = "Echo Cancelling Speakerphone (BCC950 ConferenceCam)";// MicNames[1];//DeviceMode == MicDeviceMode.Default ? (MicNames.Length > 0 ? MicNames[0] : null) : TargetDeviceName;
        
        AudioSettings.outputSampleRate = 48000;

        AudioMic.clip = Microphone.Start(CurrentDeviceName, true, 1, 48000);// OutputSampleRate);

        AudioMic.loop = true;
        while (!(Microphone.GetPosition(CurrentDeviceName) > 0)) { }
        while (!stop)
        {
            AddMicData();
            //AddMicData2();
            yield return null;
        }
        yield return null;
    }

    private void AddMicData2()
    {
        int pos = Microphone.GetPosition(CurrentDeviceName);
        int diff = pos - lastSample;

        if (diff > 0)
        {
            //float[] samples = new float[diff * sendingClip.channels];
            //sendingClip.GetData(samples, lastSample);
            //byte[] ba = ToByteArray(samples);
            //networkView.RPC("Send", RPCMode.Others, ba, sendingClip.channels);
            //Debug.Log(Microphone.GetPosition(null).ToString());

            float[] samples = new float[diff];//[AudioMic.clip.samples];
            AudioMic.clip.GetData(samples, lastSample);
            //Debug.Log($"samples: {samples.Length}");
            //foreach (float sample in samples)
            //{
            //    AudioInts.Enqueue(FloatToInt16(sample));
            //}
            msg.data = ToShortArray(samples); // AudioInts.ToArray();
            //msg.data = AudioInts.ToArray();
            //msg.data = ToByteArray(samples);
            //Debug.Log($"ros: {msg.data.Length}");
            ros.Publish(topicName, msg);

        }
        lastSample = pos;
    }

    private void AddMicData()
    {
        LastAudioTimeSample = CurrentAudioTimeSample;
        //CurrentAudioTimeSample = AudioMic.timeSamples;
        CurrentAudioTimeSample = Microphone.GetPosition(CurrentDeviceName);
        List<short> data = new List<short>();

        if (CurrentAudioTimeSample != LastAudioTimeSample)
        {
            float[] samples = new float[AudioMic.clip.samples];
            AudioMic.clip.GetData(samples, 0);
            

            if (CurrentAudioTimeSample > LastAudioTimeSample)
            {
                //lock (_asyncLockAudio)
                {
                    for (int i = LastAudioTimeSample; i < CurrentAudioTimeSample; i++)
                    {
                        data.Add(FloatToInt16(samples[i]));
                    }
                }
            }
            else if (CurrentAudioTimeSample < LastAudioTimeSample)
            {
                //lock (_asyncLockAudio)
                {
                    for (int i = LastAudioTimeSample; i < samples.Length; i++)
                    {
                        data.Add(FloatToInt16(samples[i]));
                    }
                    for (int i = 0; i < CurrentAudioTimeSample; i++)
                    {
                        data.Add(FloatToInt16(samples[i]));
                    }
                }
            }
            if (data.Count % 2 != 0)
            {
                data.Add(0);
            }
            data.AddRange(data);
            msg.data = data.ToArray();
            ros.Publish(topicName, msg);
        }
    }


    private Int16 FloatToInt16(float inputFloat)
    {
        inputFloat *= 32767;
        if (inputFloat < -32768) inputFloat = -32768;
        if (inputFloat > 32767) inputFloat = 32767;
        return Convert.ToInt16(inputFloat);
    }

     
    private sbyte[] ToByteArray(float[] floatArray)
    {
        List<sbyte> ret = new List<sbyte>();
        for (int i=0;i<floatArray.Length;i++)
        {
            var bytes = BitConverter.GetBytes(FloatToInt16(floatArray[i]));
            ret.AddRange(Array.ConvertAll(bytes, (a) => (sbyte)a));
        }
        return ret.ToArray();

    }

    private short[] ToShortArray(float[] floatArray)
    {
        List<short> ret = new List<short>();
        for (int i = 0; i < floatArray.Length; i++)
        {
            //var bytes = BitConverter.GetBytes(FloatToInt16(floatArray[i]));
            ret.Add(FloatToInt16(floatArray[i]));
            //ret.Add(FloatToInt16(floatArray[i]));
        }
        return ret.ToArray();

    }

    void FixedUpdate()
    {
        //timeElapsed += Time.deltaTime;

        //if (timeElapsed > publishMessageFrequency)
        //{
            //ros.Publish(topicName, msg);
        //ros.Publish(topicName, msg);
        timeElapsed = 0;
        //}
    }
}
