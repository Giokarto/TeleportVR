using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Training
{
    public class AudioManager : MonoBehaviour
    {
        public AudioSource[] audioSourceArray;
        int nextItemIdx;
        double prevDuration = 0.0;
        double prevStart = 0.0;
        private float[] clipSampleData;
        private int sampleDataLength = 1024;
        SortedDictionary<double, System.Action> callbacks = new SortedDictionary<double, System.Action>();
        

        private void Awake()
        {
            clipSampleData = new float[sampleDataLength];
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (callbacks.Count > 0)
            {
                if (AudioSettings.dspTime > callbacks.First().Key && callbacks.First().Value != null)
                {
                    StartCoroutine(WaitAndCall(0, callbacks.First().Value));
                    //Debug.Log($"Executing callback scheduled for {callbacks.First().Key}");
                    callbacks.Remove(callbacks.First().Key);
                }


            }
        }
        private IEnumerator WaitAndCall(float waitTime, System.Action callback)
        {
            if (callback == null)
            {
                yield break;
            }
            yield return new WaitForSeconds(waitTime);
            callback();
        }


        public void ScheduleAudioClip(AudioClip clip, bool queue = false, double delay = 0, System.Action onStart = null, System.Action onEnd = null)
        {

            if (IsAudioPlaying() && queue)
            {
                var timeLeft = prevDuration - (AudioSettings.dspTime - prevStart);
                if (timeLeft > 0) delay = timeLeft;
            }

            if (queue) nextItemIdx = (nextItemIdx + 1) % audioSourceArray.Length;
            audioSourceArray[nextItemIdx].clip = clip;

            prevStart = AudioSettings.dspTime + delay;
            audioSourceArray[nextItemIdx].PlayScheduled(prevStart);
            Debug.Log($"Audiosource #{nextItemIdx}: scheduled {clip.name} at {prevStart}");

            prevDuration = (double)clip.samples / clip.frequency;

            if (onStart != null)
            {
                callbacks.Add(prevStart, onStart);
            }
            if (onEnd != null)
            {
                callbacks.Add(prevStart + prevDuration, onEnd);
            }
        }


        public void StopAudioClips()
        {
            foreach (var source in audioSourceArray)
            {
                Debug.Log("STOPPED ALL AUDIO SOURCES");
                source.Stop();
            }
        }


        public bool IsAudioPlaying()
        {
            bool playing = false;
            foreach (var source in audioSourceArray)
            {
                playing = playing || source.isPlaying;
            }
            return playing;
        }

        public float CurrentLoudness()
        {
            var maxClipLoudness = 0f;
            foreach (var audioSource in audioSourceArray)
            {
                if (audioSource.clip != null)
                {
                    audioSource.clip.GetData(clipSampleData, audioSource.timeSamples); //I read 1024 samples, which is about 80 ms on a 44khz stereo clip, beginning at the current sample position of the clip.
                    var clipLoudness = 0f;
                    foreach (var sample in clipSampleData)
                    {
                        clipLoudness += Mathf.Abs(sample);
                    }
                    clipLoudness /= sampleDataLength;
                    if (clipLoudness > maxClipLoudness)
                        maxClipLoudness = clipLoudness;
                }

            }
            return maxClipLoudness;
        }
    }

}

