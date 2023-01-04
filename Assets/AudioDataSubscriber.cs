using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Robotics.ROSTCPConnector;
using Int16Array = RosMessageTypes.Std.Int16MultiArrayMsg;
using System;

[RequireComponent(typeof(AudioSource))]
public class AudioDataSubscriber : MonoBehaviour
{
    private int frameCount = 0;
    private int receivedCount = 0;
    private float dt = 0.0f;
    public int FPS = 0;         // Frames Per Second
    public int RPS = 0;         // Received Per Second
    private int updateRate = 1; // 1 update per sec

    public int SourceChannels = 2;
    public double SourceSampleRate = 48000;
    public double DeviceSampleRate = 48000;

    private Queue<float> ABufferQueue = new Queue<float>();
    
    private int position = 0;
    private int samplerate = 44100;
    private int channel = 2;

    private AudioClip audioClip;
    private AudioSource Audio;

    void Start()
    {
        //AudioSettings.GetDSPBufferSize(out int l, out int n);
        //Debug.Log($"dsp buffer size: {l} {n}");
        //AudioSettings.SetDSPBufferSize(4096*2, 8);// 2048*2,1);
        //AudioSettings.GetDSPBufferSize(out l, out n);
        //Debug.Log($"dsp buffer size2: {l} {n}");

        //// Get the current AudioSettings object
        //var audioSettings = AudioSettings.GetConfiguration();

        ////audioSettings.dspBufferSize = 512;
        //AudioSettings.Reset(audioSettings);

        //// Create a new AudioConfiguration object
        //AudioConfiguration config = new AudioConfiguration();


        //// Apply the modified audio configuration
        //AudioSettings.Reset(config);


        ROSConnection.GetOrCreateInstance().Subscribe<Int16Array>("/audio/audio", ProcessAudio);

        //Application.runInBackground = true;
        DeviceSampleRate = AudioSettings.GetConfiguration().sampleRate;

        if (Audio == null) Audio = GetComponent<AudioSource>();
        Audio.volume = volume;

        AudioSettings.outputSampleRate = 48000;

    }

    private void Update()
    {
        //UpdateFPS();
    }

    private void ProcessAudio(Int16Array data)
    {
        StartCoroutine(ProcessAudioData(data.data));
    }

    [Range(0f, 1f)]
    [SerializeField] private float volume = 1f;
    public float Volume
    {
        get { return volume; }
        set
        {
            volume = Mathf.Clamp(value, 0f, 1f);
            if (Audio == null) Audio = GetComponent<AudioSource>();
            Audio.volume = volume;
        }
    }


    IEnumerator ProcessAudioData3(short[] receivedAudioArray)
    {

        Audio.clip = AudioClip.Create("", 44100, 1, 44100, false);

        var data = InterleaveArray(receivedAudioArray);
        var scaledData = Array.ConvertAll(data, (a) => a / 32767f);

        DestroyImmediate(audioClip);
        Audio.Stop();
        Audio.clip.SetData(scaledData, 0);
        Audio.Play();

        yield return null;
    }

    IEnumerator ProcessAudioData(short[] receivedAudio)
    {
        receivedCount++;
        SourceSampleRate = 48000;// 16000;// BitConverter.ToInt32(_sampleRateByte, 0);
        SourceChannels = 1;// BitConverter.ToInt32(_channelsByte, 0);

        float[] ABuffer = Array.ConvertAll(receivedAudio, (a) => a / 32767f);

        for (int i = 0; i < ABuffer.Length / 2; i++)
        {
            ABufferQueue.Enqueue(ABuffer[i]);
        }

        CreateClip();

        yield return null;
    }

    void CreateClip()
    {
        //Debug.Log("Creating clip");
        if (samplerate != (int)SourceSampleRate || channel != SourceChannels)
        {
            //Debug.Log("Streaming audio into an audio clip");
            samplerate = (int)SourceSampleRate;
            channel = SourceChannels;

            if (Audio != null) Audio.Stop();
            if (audioClip != null) DestroyImmediate(audioClip);

            //audioClip = AudioClip.Create("StreamingAudio", samplerate * SourceChannels, SourceChannels, samplerate, true, OnAudioRead, OnAudioSetPosition);
            audioClip = AudioClip.Create("StreamingAudio", 48000 * 4, 2, 48000, true, OnAudioRead, OnAudioSetPosition);
            Audio = GetComponent<AudioSource>();
            Audio.clip = audioClip;
            Audio.loop = true;
            Audio.Play();
        }

    }

    void OnAudioRead(float[] data)
    {
        Debug.Log($"size of data: {data.Length} \t size of buf: {ABufferQueue.Count}");
        int count = 0;

        while (count < data.Length)
        {
            if (ABufferQueue.Count > 0)
            {

                //lock (_asyncLock)
                data[count] = ABufferQueue.Dequeue();
                //ABufferQueue.TryDequeue(out data[count]);
            }
            else { data[count] = 0f; }

            position++;
            count++;
        }

    }

    void OnAudioSetPosition(int newPosition)
    {
        position = newPosition;
    }

    private float[] ToFloatArray(byte[] byteArray)
    {
        int len = byteArray.Length / 2;
        //Debug.Log($"Largest incoming array value: {byteArray.Max()}");
        float[] floatArray = new float[len];
        for (int i = 0; i < byteArray.Length; i += 2)
        {
            floatArray[i / 2] = ((float)BitConverter.ToInt16(byteArray, i)) / 32767f;
        }
        //Debug.Log($"Largest float: {floatArray.Max()}");
        return floatArray;
    }

    private float[] NormalizeArray(short[] array)
    {
        // get left channel only
        var ret = new List<float>();
        for (int i = 0; i < array.Length / 4; i++)
        {
            ret.Add(array[i * 2] / 32767f);
        }
        //return Array.ConvertAll(ret.ToArray(), (a) => a/32767f);
        return ret.ToArray();
    }

    private float[] GetLeftChannel(short[] array)
    {
        var ret = new List<float>();
        for (int i = 0; i < array.Length / 2; i++)
        {
            ret.Add(array[i * 2]);
        }
        return ret.ToArray();
    }

    private float[] InterleaveArray(short[] array)
    {
        var leftChannel = new List<float>();
        var rightChannel = new List<float>();
        for (int i = 0; i < array.Length / 2; i++)
        {
            leftChannel.Add(array[i]);
        }
        for (int i = array.Length / 2; i < array.Length; i++)
        {
            rightChannel.Add(array[i]);
        }
        var ret = new List<float>();
        for (int i = 0; i < array.Length / 2; i++)
        {
            ret.Add(leftChannel[i]);
            ret.Add(rightChannel[i]);
        }
        return ret.ToArray();
    }

    private void UpdateFPS()
    {
        dt += Time.deltaTime;
        if (dt > 1.0f / updateRate)
        {
            FPS = Mathf.RoundToInt(frameCount / dt);
            RPS = Mathf.RoundToInt(receivedCount / dt);
            frameCount = 0;
            receivedCount = 0;
            dt -= 1.0f / updateRate;
        }
    }

}