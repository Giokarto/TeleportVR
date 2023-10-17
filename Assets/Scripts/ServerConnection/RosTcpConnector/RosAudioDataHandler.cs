using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Robotics.ROSTCPConnector;
using Int16MultiArrayMsg = RosMessageTypes.Std.Int16MultiArrayMsg;
using System;
using InputDevices.VRControllers;
using UnityEngine.Android;
using UnityEngine.Rendering.PostProcessing;
using Widgets;

namespace ServerConnection.RosTcpConnector
{
    [RequireComponent(typeof(AudioSource))]
    public class RosAudioDataHandler : MonoBehaviour
    {
        private int frameCount = 0;
        private int receivedCount = 0;
        private float dt = 0.0f;
        public int FPS = 0; // Frames Per Second
        public int RPS = 0; // Received Per Second
        private int updateRate = 1; // 1 update per sec

        public int SourceChannels = 2;
        public double SourceSampleRate = 48000;
        public double DeviceSampleRate = 48000;

        private Queue<float> ABufferQueue = new Queue<float>();
        private int ABufferQueueMaxItems = 48000; // 48000

        private int position = 0;
        private int samplerate = 44100;
        private int channel = 2;

        private AudioClip audioClip;
        [SerializeField] private AudioSource Audio;

        ROSConnection ros;
        public string outgoingTopicName = "/operator/audio";
        public string incomingTopicName = "/audio/audio";
        private Int16MultiArrayMsg msg;
        private float timeElapsed;

        [SerializeField] AudioSource AudioMic;

        public string DetectedDevices;

        //public MicDeviceMode DeviceMode = MicDeviceMode.Default;
        string CurrentDeviceName = null;

        public int OutputSampleRate = 11025;

        public int OutputChannels = 1;
        //private object _asyncLockAudio = new object();

        private int CurrentAudioTimeSample = 0;
        private int LastAudioTimeSample = 0;

        private bool stop = false;

        private int lastSample = 0;
        
        private object _asyncLockAudio = new object();

        [NonSerialized] public bool micEnabled = false;

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

            ros = ROSConnection.GetOrCreateInstance();
            ros.Subscribe<Int16MultiArrayMsg>(incomingTopicName, ProcessAudio);
            

            //Application.runInBackground = true;
            SetupAudio();
            
            //if (Permission.HasUserAuthorizedPermission(Permission.Microphone))
            //{
                // The user authorized use of the microphone.
                StartMicrophonePublisher();
            //}
            //else
            //{
            //    var callbacks = new PermissionCallbacks();
            //    callbacks.PermissionDenied += PermissionCallbacks_PermissionDenied;
            //    callbacks.PermissionGranted += PermissionCallbacks_PermissionGranted;
            //    Permission.RequestUserPermission(Permission.Microphone, callbacks);
            //}
            
            CreateClip();

        }

        void PermissionCallbacks_PermissionDenied(string permissionName)
        {
            WidgetFactory.Instance.CreateToastrWidget($"Microphone permission denied, microphone data won't be sent.", 5, "Mic Denied");
        }

        void PermissionCallbacks_PermissionGranted(string permissionName)
        {
            StartMicrophonePublisher();
        }

        private IEnumerator SetupAudio()
        {
            lock (_asyncLockAudio)
            {
                DeviceSampleRate = AudioSettings.GetConfiguration().sampleRate;
                if (Audio == null) Audio = GetComponent<AudioSource>();
                Audio.volume = volume;
                AudioSettings.outputSampleRate = 48000;


            }

            yield return null;
        }

        private void Update()
        {
            UpdateFPS();
            //if (Permission.HasUserAuthorizedPermission(Permission.Microphone))
            //{
                // The user authorized use of the microphone.
                StartCoroutine(AddMicData());
            //}
            //Debug.Log($"activated: {VRControllerInputSystem.UserActivated()}");
            // if (VRControllerInputSystem.UserDeactivated())
            // {
            //     //Debug.Log("Unsubscribed from /audio/audio");
            //     ros.Unsubscribe(incomingTopicName);
            // }
            // if (VRControllerInputSystem.UserActivated())
            // {
            //     
            //     //Debug.Log("User activated. Clearing audio buffer");
            //     ABufferQueue.Clear();
            //     ros.Subscribe<Int16MultiArrayMsg>(incomingTopicName, ProcessAudio);
            // }
            
            
        }

        private void ProcessAudio(Int16MultiArrayMsg data)
        {
            Debug.Log("Getting audio");
            if (!VRControllerInputSystem.IsUserActive()) return;
            //Debug.LogError($"Processing audio. user present: {VRControllerInputSystem.IsUserActive()}");
// #if UNITY_EDITOR
//             StartCoroutine(ProcessAudioData(data.data));
// #else
            StartCoroutine(ProcessAudioData(data.data));
//#endif

        }

        private double now;

        IEnumerator ProcessAudioData(short[] receivedAudio)
        {
            receivedCount++;
            //SourceSampleRate = 48000;// 16000;// BitConverter.ToInt32(_sampleRateByte, 0);
            //SourceChannels = 1;// BitConverter.ToInt32(_channelsByte, 0);
            //Debug.Log("ProcessAudioData: " + ABufferQueue.Count + " " + receivedAudio.Length);
            now = Time.realtimeSinceStartupAsDouble;
            //if (receivedCount%100 == 0) 
            //    Debug.Log($"Adding to buffer while userIsActive: {VRControllerInputSystem.IsUserActive()}");

            float[] ABuffer = Array.ConvertAll(receivedAudio, (a) => a / 32767f);
            for (int i = 0; i < ABuffer.Length / 2; i++)
            {
                while (ABufferQueue.Count >= ABufferQueueMaxItems)
                {
                    ABufferQueue.Dequeue();
                }
                
                ABufferQueue.Enqueue(ABuffer[i]);
            }

            yield return null;
        }

        void CreateClip()
        {
            //Debug.Log("Creating clip");
            //if (samplerate != (int)SourceSampleRate || channel != SourceChannels)
            //{
            //Debug.Log("Streaming audio into an audio clip");
            samplerate = (int)SourceSampleRate;
            channel = SourceChannels;


            if (Audio != null) Audio.Stop();
            if (audioClip != null) DestroyImmediate(audioClip);

            //audioClip = AudioClip.Create("StreamingAudio", samplerate * SourceChannels, SourceChannels, samplerate, true, OnAudioRead, OnAudioSetPosition);

            //clip length has no influence on latency
            //reducing to 1 channel has no influence on latency
            //
            audioClip = AudioClip.Create("StreamingAudio", 48000 * 4, 1, 48000, true, OnAudioRead, OnAudioSetPosition);
            Audio = GetComponent<AudioSource>();
            Audio.clip = audioClip;
            Audio.loop = true;
            Audio.Play();

            //}

        }

        void OnAudioRead(float[] data)
        {
            if (!VRControllerInputSystem.IsUserActive())
            {
                return;
            }
            //Debug.Log($"size of data: {data.Length} \t size of buf: {ABufferQueue.Count}");
            int count = 0;

            while (count < data.Length)
            {
                if (ABufferQueue.Count > 0)
                {

                    //lock (_asyncLock)
                    data[count] = ABufferQueue.Dequeue();
                    //ABufferQueue.TryDequeue(out data[count]);
                }
                else
                {
                    data[count] = 0f;
                }

                position++;
                count++;
            }

        }

        void OnAudioSetPosition(int newPosition)
        {
            position = newPosition;
        }

        [Range(0f, 1f)] [SerializeField] private float volume = 1f;

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


        void StartMicrophonePublisher()
        {
            micEnabled = true;
            ros.RegisterPublisher<Int16MultiArrayMsg>(outgoingTopicName, queue_size: 1);
            msg = new Int16MultiArrayMsg();

            StartCoroutine(CaptureMic());
        }

        private IEnumerator CaptureMic()
        {
            //if (AudioMic == null) AudioMic = GetComponent<AudioSource>();
            if (AudioMic == null) AudioMic = gameObject.AddComponent<AudioSource>();

            //Check Target Device
            DetectedDevices = "";
            string[] MicNames = Microphone.devices;
            foreach (string _name in MicNames)
            {
                DetectedDevices += _name + "\n";
                //Debug.Log($"mic: {_name}");
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

            //CurrentDeviceName = "Echo Cancelling Speakerphone (BCC950 ConferenceCam)";// MicNames[1];//DeviceMode == MicDeviceMode.Default ? (MicNames.Length > 0 ? MicNames[0] : null) : TargetDeviceName;
            //CurrentDeviceName = "Headset Microphone (Oculus Virtual Audio Device)";
            CurrentDeviceName = MicNames[0];
            AudioSettings.outputSampleRate = 48000;

            Debug.Log("starting mic device...");

            AudioMic.clip = Microphone.Start(CurrentDeviceName, true, 1, 48000); // OutputSampleRate);
            AudioMic.loop = true;

            while (!(Microphone.GetPosition(CurrentDeviceName) > 0))
            {
            }

            /*while (!stop)
            {
                //Debug.Log("entering mic cb");
                //AddMicData2();
                yield return null;
            }*/
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

                float[] samples = new float[diff]; //[AudioMic.clip.samples];
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

                //ros.Publish(outgoingTopicName, msg);

            }

            lastSample = pos;
        }

        private IEnumerator AddMicData()
        {
            LastAudioTimeSample = CurrentAudioTimeSample;
            //CurrentAudioTimeSample = AudioMic.timeSamples;
            CurrentAudioTimeSample = Microphone.GetPosition(CurrentDeviceName);
            List<short> data = new List<short>();

            float[] samples = new float[AudioMic.clip.samples];
            lock (_asyncLockAudio)
            {
                AudioMic.clip.GetData(samples, 0);
            }

            if (CurrentAudioTimeSample > LastAudioTimeSample)
            {
                lock (_asyncLockAudio)
                {
                    for (int i = LastAudioTimeSample; i < CurrentAudioTimeSample; i++)
                    {
                        data.Add(FloatToInt16(samples[i]));
                    }
                }
            }
            else if (CurrentAudioTimeSample < LastAudioTimeSample)
            {
                lock (_asyncLockAudio)
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
            ros.Publish(outgoingTopicName, msg);

            yield return null;
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
            for (int i = 0; i < floatArray.Length; i++)
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
}