using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Unity.Robotics.ROSTCPConnector;
using Int8Array = RosMessageTypes.Std.Int8MultiArrayMsg;
using Int16Array = RosMessageTypes.Std.Int16MultiArrayMsg;
using NAudio.Wave;
using System.IO;
using System;
using System.Collections.Concurrent;

[RequireComponent(typeof(AudioSource))]
public class AudioDataSubscriber : MonoBehaviour
{
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
        
    }

    private void Update()
    {
        //UpdateFPS();
        //Debug.Log($"Current queue size: {ABufferQueue.Count}");
    }

    private bool ReadyToGetFrame = true;
    public int label = 2001;
    private int dataID = 0;
    private int dataLength = 0;
    private int receivedLength = 0;

    private byte[] dataByte;
    public bool GZipMode = false;

    private int frameCount = 0;
    private int receivedCount = 0;
    private float dt = 0.0f;
    public int FPS = 0;         // Frames Per Second
    public int RPS = 0;         // Received Per Second
    private int updateRate = 1; // 1 update per sec


    public UnityEventFloatArray OnPCMFloatReadyEvent = new UnityEventFloatArray();

    private void ProcessAudio(Int16Array data)
    {
        ////Debug.Log($"max data: {data.data.Max()}");
        ////Debug.Log($"Getting audio data of length: {data.data.Length}");
        //byte[] audioBytes = new byte[data.data.Length];
        //lock (_asyncLock)
        //    System.Buffer.BlockCopy(data.data, 0, audioBytes, 0, audioBytes.Length); // needed to convert from sbyte

        //StartCoroutine(ProcessAudioData(audioBytes));


        StartCoroutine(ProcessAudioData4(data.data));
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

    public int SourceChannels = 2;
    public double SourceSampleRate = 48000;
    public double DeviceSampleRate = 48000;

    private Queue<float> ABufferQueue = new Queue<float>();
    //private ConcurrentQueue<float> ABufferQueue = new ConcurrentQueue<float>();
    //private FixedSizedQueue<float> ABufferQueue = new FixedSizedQueue<float>();
    private object _asyncLock = new object();



    IEnumerator ProcessAudioData(byte[] receivedAudioBytes)
    {
        receivedCount++;

        //if (Application.platform != RuntimePlatform.WebGLPlayer)
        //{
        //   ReadyToGetFrame = false;
        //    if (GZipMode) receivedAudioBytes = receivedAudioBytes.FMUnzipBytes();

        //if (receivedAudioBytes.Length >= 8 + 1024)
        //  {
        //    byte[] _sampleRateByte = new byte[4];
        //    byte[] _channelsByte = new byte[4];

        //byte[] _audioByte = new byte[1];
        //        lock (_asyncLock)
        //        {
        //            _audioByte = new byte[receivedAudioBytes.Length];
        //            //Buffer.BlockCopy(receivedAudioBytes, 0, _sampleRateByte, 0, _sampleRateByte.Length);
        //            //Buffer.BlockCopy(receivedAudioBytes, 4, _channelsByte, 0, _channelsByte.Length);
        //            Buffer.BlockCopy(receivedAudioBytes, 0, _audioByte, 0, _audioByte.Length);
        //        }

                SourceSampleRate = 48000;// 16000;// BitConverter.ToInt32(_sampleRateByte, 0);
                SourceChannels = 1;// BitConverter.ToInt32(_channelsByte, 0);

                float[] ABuffer = ToFloatArray(receivedAudioBytes);
                Debug.Log(ABuffer.ToString());
                Debug.Log($"length: {ABuffer.Length}");
                
                for (int i = 0; i < ABuffer.Length; i++)
                {
                    ABufferQueue.Enqueue(ABuffer[i]);
                }
                
                CreateClip();

                //OnPCMFloatReadyEvent.Invoke(ABuffer);
            //}
            //ReadyToGetFrame = true;
        //}
        yield return null;
    }

    IEnumerator ProcessAudioData2(byte[] receivedAudioBytes)
    {
        Audio.clip = AudioClip.Create("", samplerate, 1, 48000, false);
        
        var data = ToFloatArray(receivedAudioBytes);
        //Debug.Log($"len {data.Length}");
        Audio.clip.SetData(data, 0);
        Audio.loop = true;
        Audio.Play();
        yield return null;
    }

    IEnumerator ProcessAudioData3(short[] receivedAudioArray)
    {
        Audio.clip = AudioClip.Create("", 48000*2, 2, 48000, false);
        //if (Audio.clip == null)
        //{
        //    Audio.clip = AudioClip.Create("", 48000, 2, 48000, false);
        //}

        //Audio.clip.SetData(NormalizeArray(receivedAudioArray), 0);

        //var data = InterleaveArray(receivedAudioArray);
        //var data = GetLeftChannel(receivedAudioArray);

        var data = Array.ConvertAll(receivedAudioArray, (a) => a / 32767f);
        Debug.Log($"max {data.Max()}");

        DestroyImmediate(audioClip);
        Audio.Stop();
        Audio.clip.SetData(data, 0);


        //Audio.loop = true;
        Audio.Play();

        //if (!Audio.isPlaying)
        //{
        //    Audio.Play();
        //}
        yield return null;
    }

    IEnumerator ProcessAudioData4(short[] receivedAudio)
    {
        receivedCount++;
        SourceSampleRate = 48000;// 16000;// BitConverter.ToInt32(_sampleRateByte, 0);
        SourceChannels = 1;// BitConverter.ToInt32(_channelsByte, 0);

        float[] ABuffer = Array.ConvertAll(receivedAudio, (a) => a / 32767f);

        for (int i = 0; i < ABuffer.Length/2; i++)
        {
            ABufferQueue.Enqueue(ABuffer[i]);
        }

        CreateClip();

        //OnPCMFloatReadyEvent.Invoke(ABuffer);
        //}
        //ReadyToGetFrame = true;
        //}
        yield return null;
    }

    private int position = 0;
    private int samplerate = 44100;
    private int channel = 2;

    private AudioClip audioClip;
    private AudioSource Audio;
    //void CreateClip()
    //{
    //    //Debug.Log("Creating clip");
    //    //if (samplerate != (int)SourceSampleRate || channel != SourceChannels)
    //    //{
    //        //Debug.Log("Streaming audio into an audio clip");
    //        //samplerate = (int)SourceSampleRate;
    //        //channel = SourceChannels;

    //        //if (Audio != null) Audio.Stop();
    //        //if (audioClip != null) DestroyImmediate(audioClip);

    //        if (audioClip == null)
    //        {
    //            audioClip = AudioClip.Create("StreamingAudio", 48000 *2, 2, 48000, true, OnAudioRead, OnAudioSetPosition);
    //            Debug.Log("Created a sample");
    //            Audio = GetComponent<AudioSource>();
    //            Audio.clip = audioClip;
    //            //Audio.loop = true;
    //            Audio.Play();
    //        }

    //        ////audioClip = AudioClip.Create("StreamingAudio", samplerate * SourceChannels, SourceChannels, samplerate, true, OnAudioRead, OnAudioSetPosition);
    //        //audioClip = AudioClip.Create("StreamingAudio", 48000*4, 2, 48000, true, OnAudioRead, OnAudioSetPosition);
    //        //Audio = GetComponent<AudioSource>();
    //        //Audio.clip = audioClip;
    //        //Audio.loop = true;
    //        //Audio.Play();
    //    //}

    //}

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
            audioClip = AudioClip.Create("StreamingAudio", 48000*4, 2, 48000, true, OnAudioRead, OnAudioSetPosition);
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
        for (int i = 0; i < array.Length/4; i++)
        {
            ret.Add(array[i*2]/ 32767f);
        }
        //return Array.ConvertAll(ret.ToArray(), (a) => a/32767f);
        return ret.ToArray();
    }

    private float[] GetLeftChannel(short[] array)
    {
        var ret = new List<float>();
        for (int i=0; i<array.Length/2;i++)
        {
            ret.Add(array[i*2]);
        }
        return ret.ToArray();
    }

    private float[] InterleaveArray(short[] array)
    {
        var leftChannel = new Queue<float>(); 
        var rightChannel = new Queue<float>();
        for (int i=0; i<array.Length/2;i++)
        {
            leftChannel.Enqueue(array[i]);
        }
        for (int i=array.Length/2; i<array.Length; i++)
        {
            rightChannel.Enqueue(array[i]);
        }
        var ret = new List<float>();
        for (int i=0; i< array.Length/2; i++)
        {
            ret.Add(leftChannel.Dequeue());
            ret.Add(rightChannel.Dequeue());
        }
        return ret.ToArray();
    }

    public class FixedSizedQueue<T>
    {
        ConcurrentQueue<T> q = new ConcurrentQueue<T>();
        private object lockObject = new object();

        public int Limit { get; set; }
        public void Enqueue(T obj)
        {
            q.Enqueue(obj);
            lock (lockObject)
            {
                T overflow;
                while (q.Count > Limit && q.TryDequeue(out overflow)) ;
            }
        }

        public T Dequeue()
        {
            q.TryDequeue(out T ret);
            return ret;
        }

        public int Size()
        {
            return q.Count;
        }
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

    public class MyQueue<T>
    {
        // The underlying List object that stores the queue data
        private List<T> _data;
        

        // Constructor to initialize the List object
        public MyQueue()
        {
            _data = new List<T>();
        }

        // Method to add an item to the queue (Enqueue operation)
        public void Enqueue(T item)
        {
            // Add the item to the end of the List object (O(1) operation)
            _data.Add(item);
        }

        // Method to remove an item from the queue (Dequeue operation)
        public T Dequeue()
        {
            // Get the first item from the List object (O(1) operation)
            T item = _data[0];

            // Remove the first item from the List object (O(1) operation)
            _data.RemoveAt(0);

            return item;
        }

        public int Count
        {
            get { return _data.Count; }
        }
        
    }

    //public class EfficientQueue
    //{
    //    public void Node(int data)
    //    {
    //        Data = data;
    //    }


    //// Head and tail nodes of the linked list
    //private Node head;
    //private Node tail;

    //// Property for the number of elements in the queue
    //public int Count { get; private set; }

    //// Enqueue method to add an element to the end of the queue
    //public void Enqueue(int data)
    //{
    //    // Create a new node with the given data
    //    var newNode = new Node(data);

    //    // If the queue is empty, set the new node as both the head and tail
    //    if (head == null)
    //    {
    //        head = newNode;
    //        tail = newNode;
    //    }
    //    // If the queue is not empty, set the new node as the new tail and update the next pointer of the previous tail
    //    else
    //    {
    //        tail.Next = newNode;
    //        tail = newNode;
    //    }

    //    // Increment the count
    //    Count++;
    //}

    //// Dequeue method to remove the first element from the queue
    //public int Dequeue()
    //{
    //    // If the queue is empty, throw an exception
    //    if (head == null)
    //    {
    //        throw new InvalidOperationException("The queue is empty.");
    //    }

    //    // Store the data of the head node
    //    var data = head.Data;

    //    // Set the head node to the next node in the queue

    //}
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