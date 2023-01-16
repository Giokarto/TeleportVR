using System;
using ServerConnection;
using UnityEngine;
using UnityEngine.UI;

public class ImtpEncoder : MonoBehaviour
{
    [SerializeField] public GameObject leftEye;
    [SerializeField] public GameObject rightEye;
    [SerializeField] private VideoStreamViewType videoStreamViewType;
    private Renderer leftEyeRenderer, righEyeRenderer;
    private Texture2D lastReceivedTexture;

    public enum VideoStreamViewType
    {
        Single,
        Stereo
    }

    public void Start()
    {
        leftEyeRenderer = leftEye.GetComponentInChildren<Renderer>();
        righEyeRenderer = rightEye.GetComponentInChildren<Renderer>();
    }

    public void SetLastReceivedTexture(Texture lastReceivedTexture_)
    {
        try
        {
            lastReceivedTexture = GetTexture(lastReceivedTexture_);
            UpdateTextures();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private Texture2D GetTexture(Texture mainTexture)
    {
        Texture2D texture2D = new Texture2D(mainTexture.width, mainTexture.height, TextureFormat.RGBA32, false);
        
        RenderTexture currentRT = RenderTexture.active;
        
        RenderTexture renderTexture = new RenderTexture(mainTexture.width, mainTexture.height, 32);
        Graphics.Blit(mainTexture, renderTexture);
        
        RenderTexture.active = renderTexture;
        texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        
        var faces = (FindObjectOfType(typeof(aiortcConnector)) as aiortcConnector).faceCoordinates;
        foreach (var face in faces)
        {
            Debug.Log($"blue face {face[0]}, {face[1]}, {face[2]}, {face[3]}");
            for (int i = face[0]; i < face[0]+face[2]; i++)
            {
                for (int j = face[1]; j < face[1]+face[3]; j++)
                {
                    texture2D.SetPixel(i, texture2D.height - j, Color.blue);
                }
            }
        }
        
        texture2D.Apply();
        
        RenderTexture.active = currentRT;
        renderTexture.Release();
        return texture2D;
        
        // Texture2D tex = mainTexture as Texture2D;
        //
        // var faces = (FindObjectOfType(typeof(aiortcConnector)) as aiortcConnector).faceCoordinates;
        // foreach (var face in faces)
        // {
        //     for (int i = face[0]; i < face[0]+face[2]; i++)
        //     {
        //         for (int j = face[1]; j < face[1]+face[3]; j++)
        //         {
        //             tex.SetPixel(i, tex.height - j, Color.blue);
        //         }
        //     }
        // }
        // tex.Apply();
        //
        // return tex;
    }

    Texture2D GetEyeTexture2D(Texture2D originalTexture, string side)
    {
        var width = videoStreamViewType == VideoStreamViewType.Single
            ? originalTexture.width
            : originalTexture.width / 2;
        var height = originalTexture.height;
        var rightEyeOffset = videoStreamViewType == VideoStreamViewType.Single ? 0 : originalTexture.width / 2 - 1;
        var croppedTexture = new Texture2D(width * 2, height);
        var colors = originalTexture.GetPixels(side == "left" ? 0 : rightEyeOffset, 0, width, height);
        colors = FlipColors(colors, width, height);
        croppedTexture.SetPixels(width, 0, width, height, colors);
        croppedTexture.Apply();
        return croppedTexture;
    }

    Color[] FlipColors(Color[] colors, int width, int height)
    {
        Color[] newColor = new Color[width*height];
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                newColor[j * width + i] = colors[j * width + width - i - 1];
            }
        }

        return newColor;
    }
    
    static int[,] RotateMatrix(int[,] matrix, int n) {
        int[,] ret = new int[n, n];

        for (int i = 0; i < n; ++i) {
            for (int j = 0; j < n; ++j) {
                ret[i, j] = matrix[j, i];
            }
        }

        return ret;
    }

    private void UpdateTextures()
    {
        leftEyeRenderer.material.mainTexture = GetEyeTexture2D(lastReceivedTexture, "left");
        righEyeRenderer.material.mainTexture = GetEyeTexture2D(lastReceivedTexture, "right");
    }
}