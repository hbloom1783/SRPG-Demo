using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SRPGDemo.Battle.Gameplay;

namespace SRPGDemo.Battle.GUI
{
    public enum GuiID
    {
        marqueeText,
        endTurnButton,
        jumpButton,
        playerDebugText,
        enemyDebugText,
    }
    
    public class GuiElement : MonoBehaviour
    {
        public GuiID id;

        #region Shorthands

        private GuiController gui { get { return Controllers.gui; } }

        #endregion

        #region Activity

        public bool activeOnStartup;

        public void Activate()
        {
            gameObject.SetActive(true);
        }

        public void Deactivate()
        {
            gameObject.SetActive(false);
        }

        #endregion

        #region Monobehaviour

        void Start()
        {
            gui.RegisterElement(this);
            gameObject.SetActive(activeOnStartup);
        }

        #endregion
    }

    [AddComponentMenu("SRPG Demo/Battle/Gui Controller")]
    public class GuiController : MonoBehaviour
    {
        #region Shorthands

        private GameController game { get { return Controllers.game; } }

        #endregion
        
        #region GUI Element management

        Dictionary<GuiID, GuiElement> guiElements = new Dictionary<GuiID, GuiElement>();

        public void RegisterElement(GuiElement newElement)
        {
            if (guiElements.ContainsKey(newElement.id))
                throw new ArgumentException("Tried to add a second element with ID " + newElement.id.ToString());

            Debug.Log("Registering " + newElement.id.ToString());
            guiElements[newElement.id] = newElement;
        }

        public void UnregisterElement(GuiElement oldElement)
        {
            if (guiElements.ContainsKey(oldElement.id))
                guiElements.Remove(oldElement.id);
        }

        public T GetElement<T>(GuiID id) where T : GuiElement
        {
            if (guiElements.ContainsKey(id) && (guiElements[id] is T))
                return (T)guiElements[id];
            else
                return null;
        }

        public GuiElement this[GuiID id]
        {
            get { return GetElement<GuiElement>(id); }
        }

        public GuiButton GetButton(GuiID id)
        {
            return GetElement<GuiButton>(id);
        }

        public GuiText GetText(GuiID id)
        {
            return GetElement<GuiText>(id);
        }

        #endregion

        #region Input handling

        public void ButtonOnClick(GuiButton button)
        {
            Debug.Log("Received input from " + button.id.ToString());

            game.ReceiveUiSignal(button.id);
        }

        #endregion
    }
}
