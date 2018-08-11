using UnityEngine;
using SRPGDemo.Map;
using SRPGDemo.Gameplay;
using SRPGDemo.UI;

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

        private static UiController _uiController = null;
        public static UiController ui
        {
            get
            {
                if (_uiController == null)
                    _uiController = Object.FindObjectOfType<UiController>();
                return _uiController;
            }
        }
    }
}
