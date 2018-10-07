using UnityEngine;
using SRPGDemo.Strategic.Map;
using SRPGDemo.Strategic.Gameplay;
using SRPGDemo.Strategic.GUI;
using SRPGDemo.Utility;

namespace SRPGDemo.Strategic
{
    public static class Controllers
    {
        public static CachedReference<MapController> map =
            new CachedReference<MapController>(Object.FindObjectOfType<MapController>);

        public static CachedReference<GameController> game =
            new CachedReference<GameController>(Object.FindObjectOfType<GameController>);

        public static CachedReference<GuiController> gui =
            new CachedReference<GuiController>(Object.FindObjectOfType<GuiController>);

        public static void Reset()
        {
            map.Reset();
            game.Reset();
            gui.Reset();
        }
    }
}
