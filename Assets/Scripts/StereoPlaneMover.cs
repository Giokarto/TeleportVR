using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StereoPlaneMover : Singleton<StereoPlaneMover>
{
    public bool showingImages = false, manualCalibration = false;

    public Transform leftImage, rightImage;
    public Texture2D leftCalibrationTexture, rightCalibrationTexture;

    public float operatorIPD
    {
        get { return GameConfig.Instance.settings.OperatorIPD; }
        set { GameConfig.Instance.settings.OperatorIPD = value; }
    }

    [Header("Manual Calibration")]
    public float horizontal = 1;
    public float vertical = 1;
    public float depth = 1;
    public float keyStep = 0.1f;


    private Vector3 leftInitPos, rightInitPos;
    private Renderer leftRenderer, rightRenderer;
    private Texture oldLeftTexture, oldRightTexture;
    private bool oldLeftActive, oldRightActive;


    // Start is called before the first frame update
    void Start()
    {
        leftRenderer = leftImage.GetComponent<Renderer>();
        rightRenderer = rightImage.GetComponent<Renderer>();
        leftInitPos = leftImage.localPosition;
        rightInitPos = rightImage.localPosition;
        UpdateIPD();
    }

    public void UpdateIPD()
    {
        // linear regression of user derived calibration values
        // Study Data can be found here: https://docs.google.com/spreadsheets/d/17Bjk4q2Xs9SZGZ0OZsH9OgV_PBdQkzd_srlOClHPXj4/edit?usp=sharing 
        horizontal = operatorIPD * 0.0170266812f - 0.6450280125f;
        vertical = operatorIPD * 0.006009852773f - 0.1035260069f;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.H))
        {
            manualCalibration = true;
            horizontal += keyStep * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.J))
        {
            manualCalibration = true;
            vertical += keyStep * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.K))
        {
            manualCalibration = true;
            vertical -= keyStep * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.L))
        {
            manualCalibration = true;
            horizontal -= keyStep * Time.deltaTime;
        }
        else if (Input.GetKeyDown(KeyCode.F))
        {
            if (!showingImages)
            {
                // init & save old values
                oldLeftTexture = leftRenderer.material.mainTexture;
                oldRightTexture = rightRenderer.material.mainTexture;
                oldLeftActive = leftImage.gameObject.activeSelf;
                oldRightActive = rightImage.gameObject.activeSelf;

                leftRenderer.material.mainTexture = leftCalibrationTexture;
                leftRenderer.material.SetTextureScale("_MainTex", new Vector2(0.1f, 0.1f));
                rightRenderer.material.SetTextureScale("_MainTex", new Vector2(0.1f, 0.1f));
                rightRenderer.material.mainTexture = rightCalibrationTexture;
                leftImage.gameObject.SetActive(true);
                rightImage.gameObject.SetActive(true);
                showingImages = true;
            }
            else
            {
                // revert initialization
                leftImage.gameObject.SetActive(oldLeftActive);
                rightImage.gameObject.SetActive(oldRightActive);
                leftRenderer.material.mainTexture = oldLeftTexture;
                rightRenderer.material.mainTexture = oldRightTexture;
                showingImages = false;
            }
        }

        if (!manualCalibration)
        {
            UpdateIPD();
        }

        horizontal = Mathf.Max(horizontal, 0);
        vertical = Mathf.Max(vertical, 0);

        var center = (leftInitPos + rightInitPos) / 2;
        leftImage.localPosition = center + new Vector3(-horizontal, vertical, depth);
        rightImage.localPosition = center + new Vector3(horizontal, -vertical, depth);
    }

}
