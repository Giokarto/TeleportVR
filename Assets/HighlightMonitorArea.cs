using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class HighlightMonitorArea : MonoBehaviour
{
   public Image ECG;//, BP, HR, T, SPO2;
    
    private static Dictionary<string, Image> monitors;
    float timeLeft;
    Color targetColor, originalColor;
    static string changeTag;
    // Start is called before the first frame update
    void Start()
    {
        List<string> names = new List<string>{ "ECG", "BP", "T", "SPO2", "HR" };
        monitors = new Dictionary<string, Image>();
        foreach (var name in names)
        {
            monitors.Add(name, GetComponentsInChildren<Image>().FirstOrDefault(r => r.tag == name));
        }
        originalColor = monitors["ECG"].color;
        //ECG = GetComponentsInChildren<Image>().FirstOrDefault(r => r.tag == "ECG");
        changeTag = "ECG";
        targetColor = originalColor;
        targetColor.g = 160;
        timeLeft = 10f;
    }

    // Update is called once per frame
    void Update()
    {
        //if (changeTag.Length != 0)
        //{
        //    if (timeLeft <= Time.deltaTime)
        //    {
        //        // transition complete
        //        // assign the target color
        //        monitors[changeTag].color = targetColor;

        //        // start a new transition
        //        targetColor = originalColor;// new Color(Random.value, Random.value, Random.value); // originalColor; // 
        //        timeLeft = 10.0f;
        //    }
        //    else
        //    {
        //        // transition in progress
        //        // calculate interpolated color
        //        monitors[changeTag].color = Color.Lerp(monitors[changeTag].color, targetColor, Time.deltaTime / timeLeft);
        //        //Debug.Log(monitors[changeTag].color);

        //        // update the timer
        //        timeLeft -= Time.deltaTime;
        //    }
        //}
        
    }

    public static void Highlight(string tag)
    {
        changeTag = tag;

        //GetComponentsInChildren<Image>()
        var c = monitors[tag].color;  //color.g = 160;
        c.g = 160;
        monitors[tag].color = c;


        //var t = 1;
        ////monitors[tag].color = c;
        //while (monitors[tag].color.g < 160)
        //{
        //    monitors[changeTag].color = Color.Lerp(monitors[changeTag].color, c, 0.01f*t);
        //    //yield return new WaitForSeconds(0.1f);
        //    t++;
        //   // monitors[tag].color = c;

        //}
        ////Color.Lerp(monitors[tag].color, c, t); // Mathf.PingPong(Time.time, 1));

    }
}
