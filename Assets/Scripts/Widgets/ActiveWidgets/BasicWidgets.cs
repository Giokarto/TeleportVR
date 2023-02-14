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
            motionWidget = WidgetFactory.Instance.CreateIconWidget("HandUnavailable", WidgetPosition.Top, "motionWidget");

            serverData = ServerData.Instance;
        }

        public void Update()
        {
            if (!serverData.ConnectedToServer)
            {
                wifiWidget.SetIcon("WifiRed");
                micWidget.SetIcon("MicroUnavailable");
                speakerWidget.SetIcon("SpeakersUnavailable");
                motionWidget.SetIcon("HandUnavailable");
            }
            else
            {
                wifiWidget.SetIcon("WifiGreen");
                micWidget.SetIcon(serverData.ModalityConnected[Modality.VOICE]? "Micro" : "MicroDisabled");
                speakerWidget.SetIcon(serverData.ModalityConnected[Modality.AUDITION]? "Speakers" : "SpeakersOff");
                motionWidget.SetIcon(serverData.ModalityConnected[Modality.MOTOR]? "HandEnabled" : "HandDisabled");
            }
        }
    }
}