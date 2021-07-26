using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Training
{
    public class AudioManager : MonoBehaviour
    {
        private struct AudioData
        {
            public AudioClip clip;
            public double delay, earliestStart;
            public System.Action onStart, onEnd;
        }

        public AudioSource audioSource;

        // heap would be better here, but importing one from lib is a PITA
        private Dictionary<double, AudioData> priority;
        // need to keep track of all running coroutines with onEnds to be called, 
        // such that if onStart was called, onEnd will also always be.
        private Dictionary<System.Action, Coroutine> onEndCallbacks;

        private float[] clipSampleData;
        private const int sampleDataLength = 1024;

        private void Awake()
        {
            clipSampleData = new float[sampleDataLength];
            priority = new Dictionary<double, AudioData>();
            onEndCallbacks = new Dictionary<System.Action, Coroutine>();
        }

        private void Update()
        {
            if (IsAudioPlaying() || priority.Count == 0) return;
            var pair = GetMinPriority();
            var data = pair.Value;
            if (Time.timeAsDouble <= data.earliestStart) return;

            // pop key
            priority.Remove(pair.Key);

            StartCoroutine(WaitAndCall(0, data.onStart));

            audioSource.clip = data.clip;
            audioSource.Play();
            if (data.onEnd != null)
            {
                var handle = StartCoroutine(WaitAndCall(data.clip.length, data.onEnd));
                onEndCallbacks[data.onEnd] = handle;
            }
        }

        private KeyValuePair<double, AudioData> GetMinPriority()
        {
            double min = double.PositiveInfinity;
            KeyValuePair<double, AudioData> pair = new KeyValuePair<double, AudioData>();
            foreach (var entry in priority)
            {
                if (entry.Key < min)
                {
                    min = entry.Key;
                    pair = entry;
                }
            }
            return pair;
        }


        public void ScheduleAudioClip(AudioClip clip, bool queue = false, double delay = 0, System.Action onStart = null, System.Action onEnd = null)
        {
            if (!queue)
            {
                StopAudioClips();
            }
            double start = Time.timeAsDouble + delay;
            priority[start] = new AudioData() { clip = clip, delay = delay, onStart = onStart, onEnd = onEnd, earliestStart = start };
        }

        private IEnumerator WaitAndCall(float waitTime, System.Action callback)
        {
            if (callback == null)
            {
                yield break;
            }
            yield return new WaitForSeconds(waitTime);
            onEndCallbacks.Remove(callback);
            callback();
        }


        public void StopAudioClips()
        {
            foreach (var entry in onEndCallbacks)
            {
                StopCoroutine(entry.Value);
                entry.Key();
            }
            onEndCallbacks.Clear();
            priority.Clear();
            audioSource.Stop();
        }

        public void ClearQueue()
        {
            onEndCallbacks.Clear();
            priority.Clear();
        }

        public bool IsAudioPlaying()
        {
            return audioSource.isPlaying;
        }

        public float CurrentLoudness()
        {
            if (audioSource.clip != null)
            {
                audioSource.clip.GetData(clipSampleData, audioSource.timeSamples); //I read 1024 samples, which is about 80 ms on a 44khz stereo clip, beginning at the current sample position of the clip.
                double clipLoudness = 0f;
                foreach (var sample in clipSampleData)
                {
                    clipLoudness += Mathf.Abs(sample);
                }
                return (float)(clipLoudness / sampleDataLength);
            }

            return 0;
        }

    }

}

