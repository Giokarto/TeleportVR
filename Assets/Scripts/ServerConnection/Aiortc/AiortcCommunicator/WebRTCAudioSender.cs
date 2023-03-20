using System;
using System.Collections;
using System.Collections.Generic;
using Unity.WebRTC;
using UnityEngine;

namespace ServerConnection.Aiortc
{
    public class WebRTCAudioSender : MonoBehaviour
    {
        public RTCDataChannel dataChannel; // TODO: create / assign an audio channel

        private AudioSource Mic;
        private string CurrentDeviceName;
        private bool initialized;
        private int LastAudioTimeSample;
        private int CurrentAudioTimeSample;

        public void Start()
        {
            StartCoroutine(StartMicrophonePublisher());
        }

        public void Update()
        {
            if (!initialized)
            {
                return;
            }
            var micData = GetMicData();
            // TODO: figure out how to send the data
            // dataChannel.Send(micData);
        }


        IEnumerator StartMicrophonePublisher()
        {
            if (Mic == null)
            {
                Mic = gameObject.AddComponent<AudioSource>();
            }

            //Check Target Device
            string[] MicNames = Microphone.devices;
            CurrentDeviceName = MicNames[0];
            AudioSettings.outputSampleRate = 48000;

            Mic.clip = Microphone.Start(CurrentDeviceName, true, 1, 48000);
            Mic.loop = true;

            while (!(Microphone.GetPosition(CurrentDeviceName) > 0))
            {
                yield return null;
            }

            initialized = true;
        }
        
        private List<short> GetMicData()
        {
            LastAudioTimeSample = CurrentAudioTimeSample;
            CurrentAudioTimeSample = Microphone.GetPosition(CurrentDeviceName);
            List<short> data = new List<short>();

            float[] samples = new float[Mic.clip.samples];
            Mic.clip.GetData(samples, 0);

            if (CurrentAudioTimeSample > LastAudioTimeSample)
            {
                for (int i = LastAudioTimeSample; i < CurrentAudioTimeSample; i++)
                {
                    data.Add(FloatToInt16(samples[i]));
                }
            }
            else if (CurrentAudioTimeSample < LastAudioTimeSample)
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

            if (data.Count % 2 != 0)
            {
                data.Add(0);
            }

            data.AddRange(data);

            return data;
        }
        
        private Int16 FloatToInt16(float inputFloat)
        {
            inputFloat *= 32767;
            if (inputFloat < -32768) inputFloat = -32768;
            if (inputFloat > 32767) inputFloat = 32767;
            return Convert.ToInt16(inputFloat);
        }
    }
}