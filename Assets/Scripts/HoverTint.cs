using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts
{
    class HoverTint : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public void OnPointerEnter(PointerEventData eventData)
        {
            gameObject.GetComponent<Image>().color = new Color(.7f, .7f, .7f, 1);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            gameObject.GetComponent<Image>().color = new Color(1, 1, 1, 1);
        }
    }
}
