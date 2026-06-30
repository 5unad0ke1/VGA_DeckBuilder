using DeckBuilder.Data;
using UnityEngine;

namespace DeckBuilder.UI
{
    public class DeckSlot : MonoBehaviour
    {
        [SerializeField] private int slotIndex;
        private CharacterUIItem currentItem;

        public void SetSlotIndex(int index) => slotIndex = index;

        public bool TryDropItem(CharacterUIItem item)
        {
            if (item != null && currentItem == null)
            {
                InventoryManager.Instance.AddToDeck(slotIndex, item.Data);
                SetItem(item);
                DeckBuilderUI.Instance.RefreshUI();
                return true;
            }
            return false;
        }

        private void SetItem(CharacterUIItem item)
        {
            // If there was an item already, we might want to return it to inventory or swap
            // For simplicity in this prototype, we'll just allow overriding

            item.transform.SetParent(transform);
            item.transform.localPosition = Vector3.zero;
            currentItem = item;
        }

        private void ClearSlot()
        {
            if (currentItem != null)
            {
                Destroy(currentItem.gameObject);
                currentItem = null;
            }
        }
    }
}
