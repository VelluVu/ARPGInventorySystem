using UnityEngine;

namespace ARPGInventory
{
    public class ItemVisualStorage : MonoBehaviour
    {
        private RectTransform rectTransform;
        public RectTransform RectTransform { get => rectTransform = rectTransform != null ? rectTransform : GetComponent<RectTransform>(); }
    }
}