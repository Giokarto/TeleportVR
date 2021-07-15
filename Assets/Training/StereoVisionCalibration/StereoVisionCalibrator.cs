using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StereoVisionCalibrator : Singleton<StereoVisionCalibrator>
{

    public bool calibrating = false;
    public GameObject leftEyePlane, rightEyePlane;
    public Texture2D leftCalibrationTexture, rightCalibrationTexture;
    public float stepSize = 0.05f;

    private bool running = false;
    private Renderer leftRenderer, rightRenderer;
    private Texture oldLeftTexture, oldRightTexture;
    private bool oldLeftActive, oldRightActive;
    private Dictionary<KeyCode, Vector3> moveDict;
    private const string FQN = "StereoVisionCalibrator";

    // Start is called before the first frame update
    void Start()
    {
        leftRenderer = leftEyePlane.GetComponent<Renderer>();
        rightRenderer = rightEyePlane.GetComponent<Renderer>();
        moveDict = new Dictionary<KeyCode, Vector3>
        {
        {KeyCode.W, -leftEyePlane.transform.forward},
        {KeyCode.A, leftEyePlane.transform.right},
        {KeyCode.S, leftEyePlane.transform.forward},
        {KeyCode.D, -leftEyePlane.transform.right},
        };
        Load();
    }

    // Update is called once per frame
    void Update()
    {
        if (!calibrating)
        {
            return;
        }
        if (calibrating && !running)
        {
            StartCoroutine(Calibration());
            running = true;
        }
    }


    IEnumerator Calibration()
    {
        // init & save old values
        oldLeftTexture = leftRenderer.material.mainTexture;
        oldRightTexture = rightRenderer.material.mainTexture;
        oldLeftActive = leftEyePlane.activeSelf;
        oldRightActive = rightEyePlane.activeSelf;

        leftRenderer.material.mainTexture = leftCalibrationTexture;
        leftRenderer.material.SetTextureScale("_MainTex", new Vector2(0.1f, 0.1f));
        rightRenderer.material.SetTextureScale("_MainTex", new Vector2(0.1f, 0.1f));
        rightRenderer.material.mainTexture = rightCalibrationTexture;
        leftEyePlane.SetActive(true);
        rightEyePlane.SetActive(true);

        while (calibrating)
        {
            if (Input.GetKey(KeyCode.G))
            {
                calibrating = false;
            }
            foreach (KeyCode key in moveDict.Keys)
            {
                if (Input.GetKey(key))
                {
                    leftEyePlane.transform.position += stepSize * moveDict[key];
                }
            }
            yield return new WaitForEndOfFrame();
        }

        // average positon devation
        Vector3 diff = leftEyePlane.transform.position - rightEyePlane.transform.position;
        Debug.Log($"Done calibrating stereo vision, eye difference {diff.magnitude}m");
        leftEyePlane.transform.position += -diff / 2;
        rightEyePlane.transform.position += -diff / 2;
        Save();

        // revert initialization
        leftEyePlane.SetActive(oldLeftActive);
        rightEyePlane.SetActive(oldRightActive);
        leftRenderer.material.mainTexture = oldLeftTexture;
        rightRenderer.material.mainTexture = oldRightTexture;

        running = false;
        yield break;
    }

    private void Save()
    {
        // saved flag
        PlayerPrefs.SetInt(FQN, 1);
        PlayerPrefs.SetFloat($"{FQN}_left_position.x", leftEyePlane.transform.position.x);
        PlayerPrefs.SetFloat($"{FQN}_left_position.y", leftEyePlane.transform.position.y);
        PlayerPrefs.SetFloat($"{FQN}_left_position.z", leftEyePlane.transform.position.z);
        PlayerPrefs.SetFloat($"{FQN}_right_position.x", rightEyePlane.transform.position.x);
        PlayerPrefs.SetFloat($"{FQN}_right_position.y", rightEyePlane.transform.position.y);
        PlayerPrefs.SetFloat($"{FQN}_right_position.z", rightEyePlane.transform.position.z);
        PlayerPrefs.Save();
        Debug.Log("Saved StereoVisionCalibration");
    }

    private void Load()
    {
        if (!PlayerPrefs.HasKey(FQN))
        {
            Debug.LogWarning("Could not load StereoVision calibration: No preferences saved");
            return;
        }
        leftEyePlane.transform.position = new Vector3(
            PlayerPrefs.GetFloat($"{FQN}_left_position.x"), 
            PlayerPrefs.GetFloat($"{FQN}_left_position.y"), 
            PlayerPrefs.GetFloat($"{FQN}_left_position.z"));
        rightEyePlane.transform.position = new Vector3(
            PlayerPrefs.GetFloat($"{FQN}_right_position.x"), 
            PlayerPrefs.GetFloat($"{FQN}_right_position.y"), 
            PlayerPrefs.GetFloat($"{FQN}_right_position.z"));
        Debug.Log("Successfully loaded StereoVisionCalibration");
    }
}
