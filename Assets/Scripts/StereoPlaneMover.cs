using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StereoPlaneMover : Singleton<StereoPlaneMover>
{

    public Transform leftImage, rightImage;
    public Texture2D leftCalibrationTexture, rightCalibrationTexture;


    [Header("Manual Calibration")]
    public float horizontal = 1;
    public float vertical = 1;
    public float depth = 1;
    public float keyStep = 0.1f;


    [Header("Auto Calibration Values")]
    // y = a * x + b
    public float horizontal_a;
    public float horizontal_b;
    public float vertical_a;
    public float vertical_b;


    private Vector3 leftInitPos, rightInitPos;
    private Renderer leftRenderer, rightRenderer;
    private Texture oldLeftTexture, oldRightTexture;
    private bool oldLeftActive, oldRightActive;

    public bool showingImages = false;
    [SerializeField] private bool manualCalibration = false, dirtySettings = false;

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
        //if (GameConfig.Instance.settings.OperatorManualOverwrite)
        //{
        //    // if manually saved settings use those
        //    horizontal = GameConfig.Instance.settings.OperatorHorizontal;
        //    vertical = GameConfig.Instance.settings.OperatorVertical;
        //    manualCalibration = true;
        //}
        //else
        //{
        //    // linear regression of user derived calibration values
        //    // Study Data can be found here: https://docs.google.com/spreadsheets/d/17Bjk4q2Xs9SZGZ0OZsH9OgV_PBdQkzd_srlOClHPXj4/edit?usp=sharing 
        //    horizontal = GameConfig.Instance.settings.OperatorIPD * horizontal_a + horizontal_b;
        //    vertical = GameConfig.Instance.settings.OperatorIPD * vertical_a + vertical_b;
        //}
        //TODO remove this hack
        horizontal = 0;// Mathf.Max(horizontal, 0);
        vertical = 0.1432849f;// Mathf.Max(vertical, 0);
        //Debug.Log(vertical);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.H))
        {
            dirtySettings = true;
            manualCalibration = true;
            horizontal += keyStep * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.J))
        {
            dirtySettings = true;
            manualCalibration = true;
            vertical += keyStep * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.K))
        {
            dirtySettings = true;
            manualCalibration = true;
            vertical -= keyStep * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.L))
        {
            dirtySettings = true;
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

        if (dirtySettings)
        {
            Debug.Log("Overwrote IPD calibration");
            GameConfig.Instance.settings.OperatorVertical = vertical;
            GameConfig.Instance.settings.OperatorHorizontal = horizontal;
            GameConfig.Instance.settings.OperatorManualOverwrite = true;
            GameConfig.Instance.WriteSettings();
            dirtySettings = false;
        }

        UpdateIPD();

        var center = (leftInitPos + rightInitPos) / 2;
        leftImage.localPosition = center + new Vector3(-horizontal, vertical, depth);
        rightImage.localPosition = center + new Vector3(horizontal, -vertical, depth);
    }

}
