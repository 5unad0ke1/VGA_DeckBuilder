using UnityEngine;

namespace DeckBuilder.Data
{
    [CreateAssetMenu(fileName = "CharacterData", menuName = "DeckBuilder/CharacterData")]
    public class CharacterData : ScriptableObject
    {
        public string characterName;
        public int cost;
        public int hp;
        public Sprite icon;

        [HideInInspector] public int acquisitionId;
    }
}
