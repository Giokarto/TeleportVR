using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.IO.Ports;

namespace RudderPedals
{
    public class SerialReader : MonoBehaviour
    {
        private SerialPort stream;
        [Tooltip("Serial port of the arduino")]
        public string port = "COM6";
        public int baudRate = 9600;
        [Tooltip("Time step to refresh presence detector in (seconds)")]

        public float readTimeout = 0.01f, refresh = 0;
        [SerializeField] private bool connecting = false;

        void Awake()
        {
            StartCoroutine(TryConnection());
        }

        private IEnumerator TryConnection(float retryRate = 1)
        {
            connecting = true;
            while (connecting)
            {
                stream = new SerialPort(port, baudRate);
                stream.ReadTimeout = (int)(1000 * readTimeout);
                stream.DtrEnable = true;
                stream.RtsEnable = true;
                try
                {
                    stream.Open();
                    Debug.Log($"Opened serial connection on {port} @ {baudRate}");
                    connecting = false;
                    yield break;

                }
                catch (Exception)
                {
                Debug.LogError($"Could not open serial connection {port} @ {baudRate} in {retryRate}s");
                }
                yield return new WaitForSeconds(retryRate);
            }
        }

        /// <summary>
        /// Finalizer performing cleanup (closing open ressources)
        /// </summary>
        ~SerialReader()
        {
            this.stream.Close();
        }


        public IEnumerator readAsyncContinously(Action<string> callback, Action<string> onError = null)
        {
            string data = null;
            // Wait for serial to connect 
            while (stream == null || !stream.IsOpen)
            {
                yield return new WaitForEndOfFrame();
            }
            while (true)
            {
                if (!connecting)
                {
                    try
                    {
                        // request data
                        stream.WriteLine("GET");
                        stream.BaseStream.Flush();
                        var res = stream.ReadLine();
                        
                        if (res != null)
                        {
                            if (res.StartsWith("ERROR:"))
                            {
                                if (onError != null)
                                {
                                    onError(res);
                                }
                                yield break;
                            }

                            // only publish if data is new
                            if (data == null || !res.Equals(data))
                            {
                                data = res;
                                callback(data);
                            }

                        }
                    }
                    catch (TimeoutException)
                    {
                    }
                    catch (Exception)
                    {
                        onError($"Serial port {port} @ {baudRate} is not open, tying to reconnect");
                        if (!connecting)
                        {
                            StartCoroutine(TryConnection());
                        }
                    }
                }

                yield return new WaitForEndOfFrame();
            }
        }

    }

}
