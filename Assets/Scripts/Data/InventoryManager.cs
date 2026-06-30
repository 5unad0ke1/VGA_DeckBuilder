using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DeckBuilder.Data
{
    public class InventoryManager : MonoBehaviour
    {
        public static InventoryManager Instance { get; private set; }

        [Header("Master Data")]
        [SerializeField] private List<CharacterData> masterCharacterList = new List<CharacterData>();

        [Header("Current State")]
        [SerializeField] private List<CharacterData> ownedCharacters = new List<CharacterData>();
        [SerializeField] private CharacterData[] currentDeck = new CharacterData[4];

        public List<CharacterData> OwnedCharacters => ownedCharacters;
        public CharacterData[] CurrentDeck => currentDeck;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                InitializeInventory();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeInventory()
        {
            // Fallback: Generate dummy data if master list is empty
            if (masterCharacterList.Count == 0)
            {
                Debug.Log("Master list empty. Generating 10 dummy character types.");
                for (int i = 0; i < 10; i++)
                {
                    CharacterData dummy = ScriptableObject.CreateInstance<CharacterData>();
                    dummy.characterName = $"Hero {i + 1}";
                    dummy.cost = Random.Range(1, 6);
                    dummy.hp = Random.Range(10, 101);
                    masterCharacterList.Add(dummy);
                }
            }

            ownedCharacters.Clear();
            int count = Random.Range(10, 256); // 10 to 255
            
            for (int i = 0; i < count; i++)
            {
                // Randomly pick from master list
                CharacterData original = masterCharacterList[Random.Range(0, masterCharacterList.Count)];
                
                // Create an instance so we can set acquisition ID without modifying the original asset
                CharacterData instance = Instantiate(original);
                instance.acquisitionId = i;
                ownedCharacters.Add(instance);
            }
        }

        public void AddToDeck(int slotIndex, CharacterData character)
        {
            if (slotIndex < 0 || slotIndex >= currentDeck.Length) return;
            
            // If the character is already in another slot, remove it from there (unique deck check)
            for (int i = 0; i < currentDeck.Length; i++)
            {
                if (currentDeck[i] == character)
                {
                    currentDeck[i] = null;
                }
            }

            currentDeck[slotIndex] = character;
        }

        public void RemoveFromDeck(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= currentDeck.Length) return;
            currentDeck[slotIndex] = null;
        }

        public int GetTotalCost()
        {
            return currentDeck.Where(c => c != null).Sum(c => c.cost);
        }

        public void SortInventory(SortType sortType)
        {
            switch (sortType)
            {
                case SortType.Acquisition:
                    ownedCharacters = ownedCharacters.OrderBy(c => c.acquisitionId).ToList();
                    break;
                case SortType.Cost:
                    ownedCharacters = ownedCharacters.OrderByDescending(c => c.cost).ToList();
                    break;
                case SortType.HP:
                    ownedCharacters = ownedCharacters.OrderByDescending(c => c.hp).ToList();
                    break;
            }
        }
    }

    public enum SortType
    {
        Acquisition,
        Cost,
        HP
    }
}
