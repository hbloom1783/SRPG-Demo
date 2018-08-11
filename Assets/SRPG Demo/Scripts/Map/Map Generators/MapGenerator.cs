using Gamelogic.Grids;
using UnityEngine;

namespace SRPGDemo.Map
{
    [RequireComponent(typeof(MapController))]
    public abstract class MapGenerator : GridBehaviour<PointyHexPoint>
    {
        public bool generationEnabled = true;
        
        protected MapController map
        {
            get
            {
                return Controllers.map;
            }
        }

        public void Generate()
        {
            if (generationEnabled) InnerGenerate();
        }

        protected abstract void InnerGenerate();
    }
}
