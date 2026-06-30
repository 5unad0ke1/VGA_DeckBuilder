using DeckBuilder.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DeckBuilder.UI
{
    public class DeckBuilderUI : MonoBehaviour
    {
        public static DeckBuilderUI Instance { get; private set; }

        [Header("Inventory")]
        [SerializeField] private Transform inventoryContainer;
        [SerializeField] private GameObject characterPrefab;

        [Header("Deck")]
        [SerializeField] private DeckSlot[] deckSlots;

        [Header("Labels & Buttons")]
        [SerializeField] private TextMeshProUGUI totalCostText;
        [SerializeField] private TextMeshProUGUI statusMessageText;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button sortAcquisitionButton;
        [SerializeField] private Button sortCostButton;
        [SerializeField] private Button sortHPButton;

        [Header("Settings")]
        [SerializeField] private Color normalCostColor = Color.white;
        [SerializeField] private Color overLimitCostColor = Color.red;
        [SerializeField] private int maxCost = 10;

        private void Awake()
        {
            if (Instance == null) Instance = this;

            confirmButton.onClick.AddListener(OnConfirmClicked);
            sortAcquisitionButton.onClick.AddListener(() => SortAndRefresh(SortType.Acquisition));
            sortCostButton.onClick.AddListener(() => SortAndRefresh(SortType.Cost));
            sortHPButton.onClick.AddListener(() => SortAndRefresh(SortType.HP));
        }

        private void Start()
        {
            // Initial render
            RefreshInventory();
            RefreshUI();
        }

        public void RefreshInventory()
        {
            // Clear current inventory UI
            foreach (Transform child in inventoryContainer)
            {
                Destroy(child.gameObject);
            }

            // Populate from InventoryManager
            foreach (var charData in InventoryManager.Instance.OwnedCharacters)
            {
                GameObject obj = Instantiate(characterPrefab, inventoryContainer);
                var uiItem = obj.GetComponent<CharacterUIItem>();
                uiItem.Setup(charData);
            }
        }

        public void RefreshUI()
        {
            int totalCost = InventoryManager.Instance.GetTotalCost();
            totalCostText.text = $"Total Cost: {totalCost} / {maxCost}";

            if (totalCost > maxCost)
            {
                totalCostText.color = overLimitCostColor;
            }
            else
            {
                totalCostText.color = normalCostColor;
            }

            statusMessageText.text = ""; // Clear status
        }

        private void SortAndRefresh(SortType sortType)
        {
            InventoryManager.Instance.SortInventory(sortType);
            RefreshInventory();
        }

        private void OnConfirmClicked()
        {
            int totalCost = InventoryManager.Instance.GetTotalCost();
            if (totalCost > maxCost)
            {
                statusMessageText.text = "The cost has exceeded the limit.";
                statusMessageText.color = overLimitCostColor;
            }
            else
            {
                statusMessageText.text = "The deck has been saved.";
                statusMessageText.color = Color.green;
            }
        }
    }
}
