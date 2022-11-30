using ServerConnection;

namespace Widgets
{
    /// <summary>
    /// This class creates the most essential widgets at startup and manages their state.
    /// </summary>
    public class BasicWidgets : Singleton<BasicWidgets>
    {
        private IconWidget wifiWidget;
        private IconWidget micWidget;
        private IconWidget speakerWidget;
        private IconWidget motorsWidget;

        private ServerData serverData;

        public void Start()
        {
            wifiWidget = WidgetFactory.Instance.CreateIconWidget("WifiRed", WidgetPosition.Top, "wifiWidget");
            micWidget = WidgetFactory.Instance.CreateIconWidget("MicroUnavailable", WidgetPosition.Top, "micWidget");
            speakerWidget = WidgetFactory.Instance.CreateIconWidget("SpeakersUnavailable", WidgetPosition.Top, "speakerWidget");
            motorsWidget = WidgetFactory.Instance.CreateIconWidget("MotorsRed", WidgetPosition.Top, "motorsWidget");

            serverData = ServerData.Instance;
        }

        public void Update()
        {
            if (!serverData.ConnectedToServer)
            {
                wifiWidget.SetIcon("WifiRed");
                micWidget.SetIcon("MicroUnavailable");
                speakerWidget.SetIcon("SpeakersUnavailable");
                motorsWidget.SetIcon("MotorsRed");
            }
            else
            {
                wifiWidget.SetIcon("WifiGreen");
                micWidget.SetIcon(serverData.ModalityConnected[Modality.AUDITION]? "Micro" : "MicroDisabled");
                speakerWidget.SetIcon(serverData.ModalityConnected[Modality.AUDITION]? "Speakers" : "SpeakersUnavailable");
                motorsWidget.SetIcon(serverData.ModalityConnected[Modality.AUDITION]? "MotorsGreen" : "MotorsRed");
            }
        }
    }
}