using CurvedUI;
using TMPro;
using UnityEngine;

namespace Widgets
{
    public class ToastrWidget : Widget
    {
        private readonly float SLERP_DURATION = 0.5f;

        private bool slerpActive = false;
        private Vector3 localSlerpStartPos;
        private Vector3 localSlerpStopPos;
        private Timer slerpTimer;

        private TextMeshProUGUI textMeshPro;

        public float duration;

        public void Awake()
        {
            textMeshPro = gameObject.GetComponentInChildren<TextMeshProUGUI>();
            
            slerpTimer = new Timer();

            gameObject.AddComponent<CurvedUIVertexEffect>();
        }

        public void SetMessage(string message)
        {
            textMeshPro.SetText(message);
            
            textMeshPro.fontSize = 30;
            textMeshPro.color = Color.black;
        }

        public void SetDuration(float duration)
        {
            this.duration = duration;
        }

        /// <summary>
        /// Move the toastr upwards over time by a slerp. Called when top toastr is deleted. 
        /// For nicer animation, a time offset is used depending on the position of the toastr to delay animation for lower toastr.
        /// </summary>
        /// <param name="offsetInY">How far to slerp upwards</param>
        /// <param name="timeOffset">Delay slerp for this toastr by offset.</param>
        public void SlerpUp(float offsetInY, float timeOffset)
        {
            localSlerpStartPos = transform.localPosition;
            localSlerpStopPos = transform.localPosition + new Vector3(0, offsetInY, 0);
            slerpTimer.SetTimer(SLERP_DURATION + timeOffset, StopSlerp);
            slerpActive = true;
        }

        /// <summary>
        /// Manage timer.
        /// </summary>
        public void Update()
        {
            if (slerpActive)
            {
                slerpTimer.LetTimePass(Time.deltaTime);

                transform.localPosition = Vector3.Slerp(localSlerpStartPos, localSlerpStopPos, slerpTimer.GetFraction());
            }
        }

        /// <summary>
        /// Set slerp flag false.
        /// </summary>
        public void StopSlerp()
        {
            slerpActive = false;
        }
    }
}
