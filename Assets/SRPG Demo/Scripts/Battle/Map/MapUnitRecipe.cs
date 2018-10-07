using UnityEngine;

namespace SRPGDemo.Battle.Map
{
    [CreateAssetMenu(fileName = "Unit Name", menuName = "SRPG Demo/Battle/Unit Recipe")]
    public class MapUnitRecipe : ScriptableObject
    {
        public Sprite sprite;

        public AudioClip attackSound;
        public AudioClip jumpSound;
        public AudioClip runSound;

        public int maxHp;
        public int maxAp;

        public int move;
        public int jump;

        public UnitSize size;
    }
}
