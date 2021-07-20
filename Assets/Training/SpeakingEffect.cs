using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeakingEffect : MonoBehaviour
{
    public TrainingAudioManager audioManager;
    public float updateStep = 0.1f;
    public float scaleRate = 3f;

    private float currentUpdateTime = 0f;

    private float clipLoudness;
    private Vector3 originalScale;
    private Vector3 scaleChange = new Vector3(0.15f, 0.15f, 0.15f);

    // Start is called before the first frame update
    void Start()
    {
        originalScale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        currentUpdateTime += Time.deltaTime;
        if (currentUpdateTime >= updateStep)
        {
            currentUpdateTime = 0f;
            clipLoudness = audioManager.GetCurrentAudioClipLoudness();           
        }
        
        transform.localScale = Vector3.Lerp(transform.localScale, originalScale + clipLoudness * scaleChange, scaleRate * Time.deltaTime); ;// originalScale + clipLoudness * scaleChange;
       // Debug.LogWarning(clipLoudness);
    }
}
