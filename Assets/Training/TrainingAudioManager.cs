using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Widgets;

public class TrainingAudioManager : MonoBehaviour
{
    public AudioSource[] audioSourceArray;
    int toggle;
    double prevDuration = 0.0;
    double prevStart = 0.0;
    private float[] clipSampleData;
    private int sampleDataLength = 1024;

    private void Awake()
    {
        clipSampleData = new float[sampleDataLength];
    }


    public void ScheduleAudioClip(AudioClip clip, bool queue = false, double delay = 0, System.Action onStart = null, System.Action onEnd = null)
    {
        var timeLeft = 0.0;
        //queue = false;
        if (IsAudioPlaying() && queue)
        {
            timeLeft = prevDuration - (AudioSettings.dspTime - prevStart);
            if (timeLeft > 0) delay = timeLeft;
        }


        if (queue) toggle = 1 - toggle;
        audioSourceArray[toggle].clip = clip;
        //if (queue)
        //    prevStart = AudioSettings.dspTime + prevDuration + delay;
        //else
        prevStart = AudioSettings.dspTime + delay;

        StartCoroutine(WaitAndCall((float)delay, onStart));
        audioSourceArray[toggle].PlayScheduled(prevStart);
        StartCoroutine(WaitAndCall((float)(delay + clip.length), onEnd));

        //if (queue)
        //    audioSourceArray[toggle].PlayScheduled(AudioSettings.dspTime + prevDuration + delay);
        //else
        //    audioSourceArray[toggle].PlayScheduled(AudioSettings.dspTime + delay);
        prevDuration = (double)clip.samples / clip.frequency;

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


    public void StopAudioClips()
    {
        foreach (var source in audioSourceArray)
        {
            source.Stop();
        }
    }

    ///// <summary>
    ///// Shows a message on the notification widget
    ///// </summary>
    ///// <param name="message"></param>
    //public static void PublishNotification(string message)
    //{
    //    Widget notificationWidget = Manager.Instance.FindWidgetWithID(10);
    //    RosJsonMessage toastrMessage = RosJsonMessage.CreateToastrMessage(10, message, 5,
    //        new byte[] { 255, 40, 15, 255 });
    //    notificationWidget.ProcessRosMessage(toastrMessage);
    //}

    public bool IsAudioPlaying()
    {
        bool playing = false;
        foreach (var source in audioSourceArray)
        {
            playing = playing || source.isPlaying;
        }
        return playing;
    }

    public float GetCurrentAudioClipLoudness()
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
