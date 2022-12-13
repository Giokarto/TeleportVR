using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System;

using Unity.Robotics.ROSTCPConnector;
using Int8MultiArrayMsg = RosMessageTypes.Std.Int8MultiArrayMsg;


public class MicrophoneDataPublisher : MonoBehaviour
{
    ROSConnection ros;
    public string topicName = "/audio/audio";
    public float publishMessageFrequency = 0.025f;
    private Int8MultiArrayMsg msg;
    private float timeElapsed;

    AudioSource AudioMic;
    private Queue<byte> AudioBytes = new Queue<byte>();
    public string DetectedDevices;
    public MicDeviceMode DeviceMode = MicDeviceMode.Default;
    string CurrentDeviceName = null;

    public int OutputSampleRate = 11025;
    public int OutputChannels = 1;
    private object _asyncLockAudio = new object();

    private int CurrentAudioTimeSample = 0;
    private int LastAudioTimeSample = 0;

    private int chunkSize = 1400; //32768;
    private float next = 0f;
    private bool stop = false;
    private byte[] dataByte;
    private byte[] dataByteTemp;

    //void Start()
    //{
    //    var audio = GetComponent<AudioSource>();
    //    audio.volume = 1f;
    //    audio.Play();

    //    //string[] MicNames = Microphone.devices;
    //    //foreach (string _name in MicNames)
    //    //{
    //    //    DetectedDevices += _name + "\n";
    //    //    //Debug.Log($"mic: {_name}");
    //    //}
    //    //Debug.Log(DetectedDevices);
    //    ////Debug.Log(MicNames);
    //    //var audio = GetComponent<AudioSource> ();
    //    //var rec = Microphone.Start(MicNames[0], true, 1, 44100);
    //    ////audio.clip = rec;
    //    //audio.loop = true;
    //    //audio.volume = 0.5f;
    //    //Debug.Log($"audio device name: {audio.name}");
    //    ////while (!(Microphone.GetPosition(MicNames[0]) > 0)) { }
    //    //Debug.Log("Playing sound");


    //}

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<Int8MultiArrayMsg>(topicName);
        msg = new Int8MultiArrayMsg();

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

        CurrentDeviceName = MicNames[1];//DeviceMode == MicDeviceMode.Default ? (MicNames.Length > 0 ? MicNames[0] : null) : TargetDeviceName;

        AudioMic.clip = Microphone.Start(CurrentDeviceName, true, 10, 44100);// OutputSampleRate);
        AudioMic.loop = true;
        while (!(Microphone.GetPosition(CurrentDeviceName) > 0)) { }
        Debug.Log("playing sound");
        AudioMic.Play();
        Debug.Log("Start Mic(pos): " + Microphone.GetPosition(CurrentDeviceName));
        //AudioMic.Play();

        AudioMic.volume = 0f;

        OutputChannels = AudioMic.clip.channels;

        while (!stop)
        {
            //AddMicData();
            yield return null;
        }
        yield return null;
    }

    private void AddMicData()
    {
        LastAudioTimeSample = CurrentAudioTimeSample;
        //CurrentAudioTimeSample = AudioMic.timeSamples;
        CurrentAudioTimeSample = Microphone.GetPosition(CurrentDeviceName);

        if (CurrentAudioTimeSample != LastAudioTimeSample)
        {
            float[] samples = new float[AudioMic.clip.samples];
            AudioMic.clip.GetData(samples, 0);

            if (CurrentAudioTimeSample > LastAudioTimeSample)
            {
                lock (_asyncLockAudio)
                {
                    for (int i = LastAudioTimeSample; i < CurrentAudioTimeSample; i++)
                    {
                        byte[] byteData = BitConverter.GetBytes(FloatToInt16(samples[i]));
                        msg.data = Array.ConvertAll(byteData, (a) => (sbyte)a);
                        //foreach (byte _byte in byteData) AudioBytes.Enqueue(_byte);
                        Debug.Log(byteData.Length);
                    }
                }
            }
            else if (CurrentAudioTimeSample < LastAudioTimeSample)
            {
                lock (_asyncLockAudio)
                {
                    for (int i = LastAudioTimeSample; i < samples.Length; i++)
                    {
                        byte[] byteData = BitConverter.GetBytes(FloatToInt16(samples[i]));
                        foreach (byte _byte in byteData) AudioBytes.Enqueue(_byte);
                    }
                    for (int i = 0; i < CurrentAudioTimeSample; i++)
                    {
                        byte[] byteData = BitConverter.GetBytes(FloatToInt16(samples[i]));
                        msg.data = Array.ConvertAll(byteData, (a) => (sbyte)a);
                        Debug.Log(byteData.Length);
                        //foreach (byte _byte in byteData) AudioBytes.Enqueue(_byte);
                    }
                }
            }
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

    void FixedUpdate()
    {
        timeElapsed += Time.deltaTime;

        if (timeElapsed > publishMessageFrequency)
        {
            //ros.Publish(topicName, msg);
            timeElapsed = 0;
        }
    }
}
