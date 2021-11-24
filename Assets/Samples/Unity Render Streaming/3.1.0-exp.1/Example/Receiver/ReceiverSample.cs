using System;
using Unity.RenderStreaming.Samples;
using UnityEngine;
using UnityEngine.UI;

using System.Collections;

namespace Unity.RenderStreaming.Samples
{
    class ReceiverSample : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField] private RenderStreaming renderStreaming;
        [SerializeField] private Button startButton;
        [SerializeField] private Button stopButton;
        [SerializeField] private InputField connectionIdInput;
        [SerializeField] private RawImage remoteVideoImage;
        [SerializeField] private ReceiveVideoViewer receiveVideoViewer;
        [SerializeField] private SingleConnection connection;
#pragma warning restore 0649

        private string connectionId;

        void Awake()
        {
            startButton.onClick.AddListener(OnStart);
            stopButton.onClick.AddListener(OnStop);
            if (connectionIdInput != null)
                connectionIdInput.onValueChanged.AddListener(input => connectionId = input);
            receiveVideoViewer.OnUpdateReceiveTexture += texture => remoteVideoImage.texture = texture;
        }

        IEnumerator Example()
        {
            print(Time.time);
            yield return new WaitForSecondsRealtime(3);
            print(Time.time);
            OnStart();
        }

        void Start()
        {
            if (renderStreaming.runOnAwake)
                return;
            renderStreaming.Run(
                hardwareEncoder: RenderStreamingSettings.EnableHWCodec,
                signaling: RenderStreamingSettings.Signaling);
            StartCoroutine(Example());

        }

        private void OnDestroy()
        {
            OnStop();
        }

        private void OnStart()
        {
            Debug.Log("On start");
            if (string.IsNullOrEmpty(connectionId))
            {
                connectionId = System.Guid.NewGuid().ToString("N");
                connectionIdInput.text = connectionId;
            }
            connectionIdInput.interactable = false;

            connection.CreateConnection(connectionId);
            startButton.gameObject.SetActive(false);
            stopButton.gameObject.SetActive(true);
        }

        private void OnStop()
        {
            connection.DeleteConnection(connectionId);
            connectionId = String.Empty;
            connectionIdInput.text = String.Empty;
            connectionIdInput.interactable = true;
            startButton.gameObject.SetActive(true);
            stopButton.gameObject.SetActive(false);

        }
    }
}
