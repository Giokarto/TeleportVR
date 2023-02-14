using UnityEngine;
using UnityEngine.EventSystems;

namespace OperatorUserInterface
{
    public class PointerTest : MonoBehaviour, IPointerEnterHandler
    {
        public void OnPointerEnter(PointerEventData eventData)
        {
            print("TEST");
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}