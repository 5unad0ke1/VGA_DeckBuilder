using DeckBuilder.Data;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DeckBuilder.UI
{
    public class CharacterUIItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private TextMeshProUGUI hpText;
        [SerializeField] private CanvasGroup canvasGroup;

        public CharacterData Data { get; private set; }

        private Transform originalParent;
        private Vector3 originalPosition;
        private int originalSiblingIndex;

        private Canvas mainCanvas;
        private List<RaycastResult> results = new();
        public void Setup(CharacterData data)
        {
            Data = data;
            if (iconImage != null && data.icon != null) iconImage.sprite = data.icon;
            if (costText != null) costText.text = $"Cost: {data.cost}";
            if (hpText != null) hpText.text = $"HP: {data.hp}";

            mainCanvas = GetComponentInParent<Canvas>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            originalSiblingIndex = transform.GetSiblingIndex();
            originalParent = transform.parent;
            originalPosition = transform.position;

            transform.SetParent(mainCanvas.transform);
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 0.6f;
        }

        public void OnDrag(PointerEventData eventData)
        {
            transform.position = eventData.position;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 1.0f;



            results.Clear();
            EventSystem.current.RaycastAll(eventData, results);

            foreach (var item in results)
            {
                if (!item.gameObject.TryGetComponent(out DeckSlot slot))
                    continue;

                if (slot.TryDropItem(this))
                    return;

                break;
            }
            ReturnToOriginal();
        }

        public void ReturnToOriginal()
        {
            transform.SetParent(originalParent);
            transform.SetSiblingIndex(originalSiblingIndex);
            transform.position = originalPosition;
        }
    }
}
