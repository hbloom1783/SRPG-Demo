using GridLib.Hex;
using UnityEngine;
using UnityEngine.Profiling;

namespace SRPGDemo.Strategic.Map
{
    public class MapUnit : MonoBehaviour
    {
        #region Shorthands

        private MapController map { get { return MapController.instance; } }

        #endregion

        #region Presentation

        private SpriteRenderer _spriteRenderer = null;
        public SpriteRenderer spriteRenderer
        {
            get
            {
                if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();
                return _spriteRenderer;
            }
        }

        #endregion
        
        public HexCoords loc
        {
            get
            {
                Profiler.BeginSample("Strategic.MapUnit.loc");

                HexCoords result = map.WhereIs(this);

                Profiler.EndSample();

                return result;
            }
        }
    }
}
