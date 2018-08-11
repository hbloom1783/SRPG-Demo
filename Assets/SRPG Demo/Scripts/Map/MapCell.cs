using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gamelogic.Grids;
using Gamelogic.Extensions;

namespace SRPGDemo.Map
{
    [AddComponentMenu("SRPG/Map Cell")]
    [RequireComponent(typeof(SpriteRenderer))]
    public class MapCell : TileCell
    {
        // Universal

        #region MobilesPresent

        private List<MapMobile> mobilesPresent = new List<MapMobile>();

        public void AddMobile(MapMobile mobile)
        {
            mobilesPresent.Add(mobile);
            __UpdatePresentation(true);
        }

        public void RemoveMobile(MapMobile mobile)
        {
            mobilesPresent.Remove(mobile);
            __UpdatePresentation(true);
        }

        public IEnumerable<MapMobile> MobilesPresent()
        {
            return mobilesPresent;
        }

        #endregion
        
        #region Sprite passthrough

        private SpriteRenderer _spriteRenderer = null;
        public SpriteRenderer spriteRenderer
        {
            get
            {
                if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();
                return _spriteRenderer;
            }
        }

        public override Color Color
        {
            get
            {
                return spriteRenderer.color;
            }

            set
            {
                spriteRenderer.color = value;
            }
        }

        public override Vector2 Dimensions
        {
            get
            {
                return spriteRenderer.bounds.size;
            }
        }

        public override void SetAngle(float angle)
        {
            spriteRenderer.transform.SetLocalRotationZ(angle);
        }

        public override void AddAngle(float angle)
        {
            spriteRenderer.transform.RotateAroundZ(angle);
        }

        #endregion

        #region Tint controls

        private IEnumerator TintShifter(Color startColor, Color endColor, float cycleTime)
        {
            Color a = startColor;
            Color b = endColor;

            while (true)
            {
                for (float timeElapsed = 0.0f; timeElapsed < cycleTime; timeElapsed += Time.deltaTime)
                {
                    spriteRenderer.color = Color.Lerp(a, b, timeElapsed / cycleTime);
                    yield return null;
                }

                Color swap = a;
                a = b;
                b = swap;
            }
        }

        private Coroutine tintShifter = null;

        public void SetTint(Color color)
        {
            ClearTint();

            spriteRenderer.color = color;
        }

        public void SetTint(Color startColor, Color endColor, float cycleTime = 1.0f)
        {
            ClearTint();

            tintShifter = StartCoroutine(TintShifter(startColor, endColor, cycleTime));
        }

        public void ClearTint()
        {
            if (tintShifter != null)
            {
                StopCoroutine(tintShifter);
                tintShifter = null;
            }

            spriteRenderer.color = Color.white;
        }

        #endregion

        // Implementation-specific

        public int tempCounter = 0;

        #region Presentation Properties

        public Sprite walkableSprite = null;
        public Sprite nonWalkableSprite = null;

        private bool _walkable = true;
        public bool walkable
        {
            get { return _walkable; }
            set
            {
                _walkable = value;
                __UpdatePresentation();
            }
        }

        public override void __UpdatePresentation(bool forceUpdate = false)
        {
            switch (walkable)
            {
                case true:
                    spriteRenderer.sprite = walkableSprite;
                    break;
                case false:
                    spriteRenderer.sprite = nonWalkableSprite;
                    break;
            }
        }

        #endregion

        private void Start()
        {
            __UpdatePresentation(true);
        }
    }
}
