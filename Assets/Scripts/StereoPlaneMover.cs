using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class StereoPlaneMover : Singleton<StereoPlaneMover>
{
    public Transform leftImage, rightImage;
    public Texture2D leftCalibrationTexture, rightCalibrationTexture;
    public float horizontal = 1;
    public float vertical = 1;
    public float depth = 1;
    public float keyStep = 0.1f;
    public bool showingImages = false;
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
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.H))
        {
            horizontal += keyStep * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.J))
        {
            vertical += keyStep * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.K))
        {
            vertical -= keyStep * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.L))
        {
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
        horizontal = Mathf.Max(horizontal, 0);
        vertical = Mathf.Max(horizontal, 0);
        var center = (leftInitPos + rightInitPos) / 2;
        leftImage.localPosition = center + new Vector3(-horizontal, vertical, depth);
        rightImage.localPosition = center + new Vector3(horizontal, -vertical, depth);
    }
}
