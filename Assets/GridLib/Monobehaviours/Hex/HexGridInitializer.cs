using UnityEngine;

namespace GridLib.Hex
{
    public abstract class HexGridInitializer<TCell> : MonoBehaviour where TCell : HexGridCell
    {
        public bool generationEnabled = true;

        public abstract void InitGrid(HexGridManager<TCell> grid, bool isPlaying);
    }
}
