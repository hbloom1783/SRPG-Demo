using UnityEngine;
using SRPGDemo.Map;
using SRPGDemo.Gameplay;

namespace SRPGDemo
{
    public static class Controllers
    {
        private static MapController _mapController = null;
        public static MapController map
        {
            get
            {
                if (_mapController == null)
                    _mapController = Object.FindObjectOfType<MapController>();
                return _mapController;
            }
        }

        private static GameController _gameController = null;
        public static GameController game
        {
            get
            {
                if (_gameController == null)
                    _gameController = Object.FindObjectOfType<GameController>();
                return _gameController;
            }
        }
    }
}
