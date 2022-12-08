using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Unity.Robotics.ROSTCPConnector;
using Int8Array = RosMessageTypes.Std.Int8MultiArrayMsg;
using NAudio.Wave;
using System.IO;
using System;

[RequireComponent(typeof(AudioSource))]
public class AudioDataSubscriber : MonoBehaviour
{
    void Start()
    {
        AudioSettings.GetDSPBufferSize(out int l, out int n);
        Debug.Log($"dsp buffer size: {l} {n}");
        AudioSettings.SetDSPBufferSize(256,8);
        AudioSettings.GetDSPBufferSize(out l, out n);
        Debug.Log($"dsp buffer size2: {l} {n}");
        ROSConnection.GetOrCreateInstance().Subscribe<Int8Array>("/audio/audio", ProcessAudio);

        //Application.runInBackground = true;
        DeviceSampleRate = AudioSettings.GetConfiguration().sampleRate;

        if (Audio == null) Audio = GetComponent<AudioSource>();
        Audio.volume = volume;
    }

    private bool ReadyToGetFrame = true;
    public int label = 2001;
    private int dataID = 0;
    private int dataLength = 0;
    private int receivedLength = 0;

    private byte[] dataByte;
    public bool GZipMode = false;


    public UnityEventFloatArray OnPCMFloatReadyEvent = new UnityEventFloatArray();

    private void ProcessAudio(Int8Array data)
    {

        byte[] audioBytes = new byte[data.data.Length];
        lock (_asyncLock)
            System.Buffer.BlockCopy(data.data, 0, audioBytes, 0, audioBytes.Length);
        //ProcessAudioData(audioBytes);
        StartCoroutine(ProcessAudioData(audioBytes));
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

    public int SourceChannels = 1;
    public double SourceSampleRate = 48000;
    public double DeviceSampleRate = 48000;

    private Queue<float> ABufferQueue = new Queue<float>();
    private object _asyncLock = new object();

    IEnumerator ProcessAudioData(byte[] receivedAudioBytes)
    {
        if (Application.platform != RuntimePlatform.WebGLPlayer)
        {
            ReadyToGetFrame = false;
            if (GZipMode) receivedAudioBytes = receivedAudioBytes.FMUnzipBytes();

            if (receivedAudioBytes.Length >= 8 + 1024)
            {
                //byte[] _sampleRateByte = new byte[4];
                //byte[] _channelsByte = new byte[4];
                byte[] _audioByte = new byte[1];
                lock (_asyncLock)
                {
                    _audioByte = new byte[receivedAudioBytes.Length];
                    //Buffer.BlockCopy(receivedAudioBytes, 0, _sampleRateByte, 0, _sampleRateByte.Length);
                    //Buffer.BlockCopy(receivedAudioBytes, 4, _channelsByte, 0, _channelsByte.Length);
                    Buffer.BlockCopy(receivedAudioBytes, 0, _audioByte, 0, _audioByte.Length);
                }

                SourceSampleRate = 48000;// BitConverter.ToInt32(_sampleRateByte, 0);
                SourceChannels = 2;// BitConverter.ToInt32(_channelsByte, 0);

                float[] ABuffer = ToFloatArray(_audioByte);

                for (int i = 0; i < ABuffer.Length; i++)
                {
                    ABufferQueue.Enqueue(ABuffer[i]);
                }

                CreateClip();

                OnPCMFloatReadyEvent.Invoke(ABuffer);
            }
            ReadyToGetFrame = true;
        }
        yield return null;
    }

    private int position = 0;
    private int samplerate = 44100;
    private int channel = 2;

    private AudioClip audioClip;
    private AudioSource Audio;
    void CreateClip()
    {
        if (samplerate != (int)SourceSampleRate || channel != SourceChannels)
        {
            samplerate = (int)SourceSampleRate;
            channel = SourceChannels;

            if (Audio != null) Audio.Stop();
            if (audioClip != null) DestroyImmediate(audioClip);

            audioClip = AudioClip.Create("StreamingAudio", samplerate * SourceChannels, SourceChannels, samplerate, true, OnAudioRead, OnAudioSetPosition);
            Audio = GetComponent<AudioSource>();
            Audio.clip = audioClip;
            Audio.loop = true;
            Audio.Play();
        }

    }

    void OnAudioRead(float[] data)
    {
        int count = 0;
        while (count < data.Length)
        {
            if (ABufferQueue.Count > 0)
            {
                lock (_asyncLock) data[count] = ABufferQueue.Dequeue();
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
        float[] floatArray = new float[len];
        for (int i = 0; i < byteArray.Length; i += 2)
        {
            floatArray[i / 2] = ((float)BitConverter.ToInt16(byteArray, i)) / 32767f;
        }
        return floatArray;
    }
}


//{
//    public string TopicName;
//    private AudioSource audioSource;
//    private volatile Queue<AudioClip> queue = new Queue<AudioClip>();
//    private AudioClip clip;

//    private Queue<float> ABufferQueue = new Queue<float>();
//    private int position = 0;
//    private object _asyncLock = new object();

//    private bool messageProcessed = false;

//    private int frameCount = 0;
//    private int receivedCount = 0;
//    private float dt = 0.0f;
//    public int FPS = 0;         // Frames Per Second
//    public int RPS = 0;         // Received Per Second
//    private int updateRate = 1; // 1 update per sec

//    private void Start()
//    {
//        ROSConnection.GetOrCreateInstance().Subscribe<Int8Array>(TopicName, ProcessAudio);
//        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        
//    }

//    // Update is called once per frame
//    void Update()
//    {
//        //if (!audioSource.isPlaying)
//        //{
//        //    Debug.LogError("AudioSource is not playing");
//        //}

//        // Check that the AudioSource component has the correct settings for the audio data
//        //if (audioSource.clip.frequency != 48000)
//        //{
//        //    Debug.LogError("AudioSource has incorrect sample rate");
//        //}

//        //if (audioSource.clip.channels != 1)
//        //{
//        //    Debug.LogError("AudioSource has incorrect number of channels");
//        //}
//    }

//    void FixedUpdate()
//    {

//        //if (audioFloatData != null)
//        //{
//        //    if (Audio.clip == null)
//        //    {
//        //        var clip = AudioClip.Create("stream", audioFloatData.Length, 1, 44100, false);

//        //        Audio.clip = clip;
//        //    }
//        //    Audio.clip.SetData(audioFloatData, 0);
//        //    //Audio.Play();
//        //}

//        //Debug.Log($"queue: {queue.Count}");

//        //if (queue.Count > 0)
//        //{
//        //    audioSource.clip = queue.Dequeue();

//        //    if (!audioSource.isPlaying)
//        //        audioSource.Play();
//        //}

//    }

//    private float[] PCM2Floats(byte[] bytes)
//    {
//        float max = -(float)System.Int16.MinValue * 0.5f;

//        float[] samples = new float[bytes.Length / 4];
//        for (int i = 0; i < samples.Length; i++)
//        {
//            short int16sample = System.BitConverter.ToInt16(bytes, i * 4);
//            samples[i] = (float)int16sample / max;
//        }

//        //for (int i = 0; i < samples.Length; i++)
//        //{
//        //    int sampleIndex = i * 4;
//        //    short intSample = System.BitConverter.ToInt16(bytes, sampleIndex);
//        //    float sample = intSample / 32768.0f;
//        //    samples[i] = sample;
//        //}
//        Debug.Log($"max: {samples.Max()}, min: {samples.Min()}");
//        return samples;
//    }

//    private float[] GPTByteToFloat(byte[] byteArray)
//    {
//        // Calculate the number of audio samples in the byte array
//        int numSamples = byteArray.Length / 2; // 2 bytes per sample, 1 channel

//        // Create a new float array to hold the converted audio data
//        float[] audioData = new float[numSamples];

//        // Iterate over the byte array, reading 2 bytes per sample
//        for (int i = 0; i < numSamples; i++)
//        {
//            // Read the 2 bytes for the sample
//            byte[] sampleBytes = new byte[2];
//            System.Array.Copy(byteArray, i * 2, sampleBytes, 0, 2);

//            // Convert the 2 bytes to a 16-bit integer
//            short sample = System.BitConverter.ToInt16(sampleBytes, 0);

//            // Scale the 16-bit integer value to the range [-1, 1]
//            float scaledSample = (float)sample / 32768.0f;

//            // Store the scaled value in the float array
//            audioData[i] = scaledSample;
//        }
//        return audioData;
//    }
//    //{
//    //    // Calculate the number of audio samples in the byte array
//    //    int numSamples = byteArray.Length / (2 * 2); // 2 bytes per sample, 2 channels

//    //    // Create a new float array to hold the converted audio data
//    //    float[] audioData = new float[numSamples];

//    //    // Iterate over the byte array, reading 2 bytes per sample
//    //    for (int i = 0; i < numSamples; i++)
//    //    {
//    //        // Read the 2 bytes for the sample
//    //        byte[] sampleBytes = new byte[2];
//    //        System.Array.Copy(byteArray, i * 2, sampleBytes, 0, 2);

//    //        // Convert the 2 bytes to a 16-bit integer
//    //        short sample = System.BitConverter.ToInt16(sampleBytes, 0);

//    //        // Scale the 16-bit integer value to the range [-1, 1]
//    //        float scaledSample = (float)sample / 32768.0f;

//    //        // Store the scaled value in the float array
//    //        audioData[i] = scaledSample;
//    //    }
//    //    return audioData;
//    //}

//    private float[] ConvertToFloatArray(byte[] array)
//    {
//        //Debug.Log(array.ToString());
//        float max = (float)System.Int16.MinValue;// * 0.5f;
//        float[] floatArr = new float[array.Length / 4];
//        for (int i = 0; i < floatArr.Length; i++)
//        {
//            if (System.BitConverter.IsLittleEndian)
//            {
//                System.Array.Reverse(array, i * 4, 4);
//            }
//            floatArr[i] = System.BitConverter.ToSingle(array, i * 4) / max;
//        }
//        //Debug.Log($"float length: {floatArr.Length}");
//        return floatArr;
//    }

//    private void ProcessAudio(Int8Array data)
//    {

//        //float[] audioFloats = data.data.Select(x => -x/127.0f).ToArray();
//        //byte[] audioBytes = (byte[])(System.Array) data.data;// data.data.Select(x =>(byte) x).ToArray();

//        byte[] audioBytes = new byte[data.data.Length];
//        lock (_asyncLock)
//            System.Buffer.BlockCopy(data.data, 0, audioBytes, 0, audioBytes.Length);


//        //float[] audioFloats = ConvertToFloatArray(audioBytes);
//        //float[] audioFloats = PCM2Floats(audioBytes);
//        float[] audioData = GPTByteToFloat(audioBytes);

//        /*
//        // Create a new float array to hold the interleaved audio data
//        int numSamples = audioData.Length / 2; // 2 channels
//        float[] interleavedData = new float[numSamples * 2];

//        // Interleave the left and right channel data
//        for (int i = 0; i < numSamples; i++)
//        {
//            interleavedData[i * 2] = audioData[i]; // Left channel
//            interleavedData[i * 2 + 1] = audioData[i + numSamples]; // Right channel
//        }
//        */

//        for (int i = 0; i < audioData.Length; i++)
//        {
//            ABufferQueue.Enqueue(audioData[i]);
//        }

//        clip = AudioClip.Create("StreamingAudio", audioData.Length, 1, 44100, true, OnAudioRead, OnAudioSetPosition);
//        //clip = AudioClip.Create("stream", audioData.Length, 1, 16000, false);

       

//        //clip.SetData(audioData,0);
        
//        //queue.Enqueue(clip);
//        //clip.SetData(ConvertToFloatArray(audioBytes),0);

//        audioSource.clip = clip;
//        if(!audioSource.isPlaying)
//            audioSource.Play();
//        //Debug.Log("Done");
//    }

//    void OnAudioRead(float[] data)
//    {
//        int count = 0;
//        while (count < data.Length)
//        {
//            if (ABufferQueue.Count > 0)
//            {
//                lock (_asyncLock) data[count] = ABufferQueue.Dequeue();
//            }
//            else { data[count] = 0f; }

//            position++;
//            count++;
//        }
//    }

//    void OnAudioSetPosition(int newPosition)
//    {
//        position = newPosition;
//    }

//    //private float[] ToFloatArray(byte[] byteArray)
//    //{
//    //    int len = byteArray.Length / 2;
//    //    float[] floatArray = new float[len];
//    //    for (int i = 0; i < byteArray.Length; i += 2)
//    //    {
//    //        floatArray[i / 2] = ((float)System.BitConverter.ToInt16(byteArray, i)) / 32767f;
//    //    }
//    //    return floatArray;
//    //}

//}