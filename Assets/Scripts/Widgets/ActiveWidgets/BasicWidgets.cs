using System;
using ServerConnection;

namespace Widgets.ActiveWidgets
{
    /// <summary>
    /// This class creates the most essential widgets at startup and manages their state.
    /// </summary>
    public class BasicWidgets : Singleton<BasicWidgets>
    {
        private IconWidget wifiWidget;
        private IconWidget micWidget;
        private IconWidget speakerWidget;
        private IconWidget motionWidget;

        private ServerData serverData;

        public void Start()
        {
            wifiWidget = WidgetFactory.Instance.CreateIconWidget("WifiRed", WidgetPosition.Top, "wifiWidget");
            micWidget = WidgetFactory.Instance.CreateIconWidget("MicroUnavailable", WidgetPosition.Top, "micWidget");
            speakerWidget = WidgetFactory.Instance.CreateIconWidget("SpeakersUnavailable", WidgetPosition.Top, "speakerWidget");
            motionWidget = WidgetFactory.Instance.CreateIconWidget("MotionUnavailable", WidgetPosition.Top, "motionWidget");

            serverData = ServerData.Instance;
        }

        public DateTime nextConnectionCheck = DateTime.Now + TimeSpan.FromSeconds(30);
        public void Update()
        {
            if (!serverData.ConnectedToServer)
            {
                wifiWidget.SetIcon("WifiRed");
                micWidget.SetIcon("MicroUnavailable");
                speakerWidget.SetIcon("SpeakersUnavailable");
                motionWidget.SetIcon("MotionUnavailable");

                if (nextConnectionCheck < DateTime.Now)
                {
                    WidgetFactory.Instance.CreateToastrWidget(
                        $"Trying to connect to server: {ServerData.Instance.IPaddress}", 5, "Connection error");
                    WidgetFactory.Instance.CreateToastrWidget(
                        $"Please make sure the server is running and you're in the same network.", 1, "Connection error 2");
                    nextConnectionCheck = DateTime.Now + TimeSpan.FromSeconds(30);
                }
            }
            else
            {
                nextConnectionCheck = DateTime.Now + TimeSpan.FromSeconds(30);
                wifiWidget.SetIcon("WifiGreen");
                micWidget.SetIcon(serverData.ModalityConnected[Modality.VOICE]? "Micro" : "MicroDisabled");
                speakerWidget.SetIcon(serverData.ModalityConnected[Modality.AUDITION]? "Speakers" : "SpeakersOff");
                motionWidget.SetIcon(serverData.ModalityConnected[Modality.MOTOR]? "MotionOn" : "MotionOff");
            }
        }
    }
}