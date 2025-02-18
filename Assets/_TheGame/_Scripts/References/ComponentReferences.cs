using _TheGame._Scripts.Board;
using TMPro;
using UnityEngine;

namespace _TheGame._Scripts.References
{
    public class ComponentReferences : Singleton<ComponentReferences>
    {
        public BoardGrid boardGrid;
        public Transform createdBlockParent;
        public TextMeshProUGUI movesText;
        public TextMeshProUGUI levelText;
    }
}
