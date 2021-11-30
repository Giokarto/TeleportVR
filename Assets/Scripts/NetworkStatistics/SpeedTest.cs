using System;
using UnityEngine;

using System.Net;
using System.Net.NetworkInformation;
//using Ping = System.Net.NetworkInformation.Ping;
using System.Text;
using Debug = UnityEngine.Debug;
using System.Collections;
using UnityEngine.UI;

public class SpeedTest : MonoBehaviour
{

    public Text pingNumber, downNumber, upNumber;

    float lastUpdate;
    double dspeed, uspeed, prevDspeed = 0.0, prevUspeed = 0.0;
    bool download_done = true, upload_done = true;
    DateTime uploadStartTime, downloadStartTime;
    double filesize = 10000;
    //Ping pingSender;
    //PingOptions options;
    //byte[] buffer;
    //int timeout = 1000;
    //PingReply reply;
    double latency = 0, prevLatency = 0;
    UnityEngine.Ping ping;
    // Start is called before the first frame update
    void Start()
    {

        //pingSender = new Ping();
        //options = new PingOptions(64, true);

        //// Use the default Ttl value which is 128,
        //// but change the fragmentation behavior.
        //options.DontFragment = true;

        //// Create a buffer of 32 bytes of data to be transmitted.
        //string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        //buffer = Encoding.ASCII.GetBytes(data);

        var client = new WebClient();
        client.DownloadFile(new Uri("http://speedtest.tele2.net/1MB.zip"), Application.persistentDataPath + "1MB.zip");
        
        ping = new UnityEngine.Ping("8.8.8.8");

        StartCoroutine(PingUpdate());
    }

    // Update is called once per frame
    void Update()
    {
     if (Time.time - lastUpdate > 5)
        {
            //Debug.Log("Start ping");
            //reply = pingSender.Send("142.251.36.238", timeout, buffer, options);
            //if (reply.Status == IPStatus.Success)
            //{
            //    latency = reply.RoundtripTime;
            //}
            //Debug.Log("ping reply: " + reply.Status.ToString());
            StartCoroutine(PingUpdate());

            if (download_done)
            {
                MeasureDownload();
            }
            if (upload_done)
            {
                MeasureUpload();
            }

            var txt = string.Format("Download Speed: {0}Mb/s \t Upload Speed: {1}Mb/s \t Latency: {2} ms", dspeed, uspeed, latency );
            //Debug.Log(txt);

            //Training.TutorialSteps.PublishNotification(txt);
            lastUpdate = Time.time;
        }
     

    }

    private void FixedUpdate()
    {
        var eps = 0.1;
        if (Math.Abs(prevLatency -latency) > eps)
        {
            var step = (prevLatency > latency) ? -1*eps : eps;
            prevLatency += step;
        }
        pingNumber.text = (Math.Round(prevLatency)).ToString();

        if (Math.Abs(prevDspeed - dspeed) > eps)
        {
            var step = (prevDspeed > dspeed) ? -1 * eps : eps;
            prevDspeed += step;
        }
        downNumber.text = (Math.Round(prevDspeed)).ToString();

        if (Math.Abs(prevUspeed - uspeed) > eps)
        {
            var step = (prevUspeed > uspeed) ? -1 * eps : eps;
            prevUspeed += step;
        }
        upNumber.text = (Math.Round(prevUspeed)).ToString();

    }

    void MeasureDownload()
    {
        //Debug.Log("Download start");
        download_done = false;
        var client = new WebClient();
        client.DownloadFileCompleted += new System.ComponentModel.AsyncCompletedEventHandler(DownloadFileCompleted);
        
        downloadStartTime = DateTime.Now;
        client.DownloadFileAsync(new Uri("http://speedtest.tele2.net/10MB.zip"), Application.persistentDataPath+"10MB-download.zip");

    }

    void MeasureUpload()
    {
        var client = new WebClient();
        
        client.UploadFileCompleted += new System.Net.UploadFileCompletedEventHandler(UploadFileCompleted);
        upload_done = false;
        uploadStartTime = DateTime.Now;
        client.UploadFileAsync(new Uri("http://speedtest.tele2.net/upload.php"), WebRequestMethods.Http.Post, Application.persistentDataPath+"1MB.zip");

    }

    void DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
    {
        if (e.Error == null)
        {
            DateTime endTime = DateTime.Now;
            var speed = Math.Round((filesize / (endTime - downloadStartTime).TotalSeconds));
            
            dspeed = speed* 0.008;
            
        }
        else
        {
            dspeed = 0;
            Debug.Log(e.Error.Message);
        }
        download_done = true;
        //downNumber.text = Math.Round(dspeed, 1).ToString();
    }

    void UploadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
    {
        if (e.Error == null)
        {
            DateTime endTime = DateTime.Now;
            var speed = Math.Round((1000 / (endTime - uploadStartTime).TotalSeconds));

            uspeed = speed * 0.008;
            
        } else
        {
            uspeed = 0;
        }
        upload_done = true;
        //upNumber.text = Math.Round(uspeed, 1).ToString();
    }

    IEnumerator PingUpdate()
    {
        yield return new WaitForSeconds(1f);
        if (ping.isDone)
        {
            //prevLatency = latency;
            latency = ping.time;
            ping = new UnityEngine.Ping("8.8.8.8");
        }
        else
        {
            latency = 0;
            Debug.LogError("Too long ping");
        }
        
    }


}
