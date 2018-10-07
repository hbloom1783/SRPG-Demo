using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Profiling;
using Gamelogic.Grids;
using SRPGDemo.Battle.GUI;
using SRPGDemo.Battle.Gameplay;
using SRPGDemo.Utility;

namespace SRPGDemo.Battle.Map
{
    public enum UnitSize
    {
        small,
        large,
    }

    public enum UnitTeam
    {
        player,
        enemy,
    }

    [AddComponentMenu("SRPG Demo/Battle/Map Unit")]
    [RequireComponent(typeof(SpriteRenderer))]
    public class MapUnit : MonoBehaviour
    {
        #region Shorthands

        private MapController map { get { return Controllers.map; } }
        private GameController game { get { return Controllers.game; } }

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

        private Facing _facing = Facing.bad;
        public Facing facing
        {
            get { return _facing; }
            set
            {
                _facing = value;
                if (map.HasUnit(this)) map.ReseatUnit(this);
                UpdatePresentation();
            }
        }

        private void UpdatePresentation()
        {
            switch (facing)
            {
                case Facing.ne:
                case Facing.e:
                case Facing.se:
                    spriteRenderer.flipX = false;
                    break;

                case Facing.nw:
                case Facing.w:
                case Facing.sw:
                    spriteRenderer.flipX = true;
                    break;
            }
        }

        #endregion

        #region Temporary GUI stuff

        private static GuiText PlayerDebugTextGetter()
        {
            return Controllers.gui.cache.GetText(GuiID.playerDebugText);
        }
        private CachedReference<GuiText> playerDebugText =
            new CachedReference<GuiText>(PlayerDebugTextGetter);

        private static GuiText EnemyDebugTextGetter()
        {
            return Controllers.gui.cache.GetText(GuiID.enemyDebugText);
        }
        private CachedReference<GuiText> enemyDebugText =
            new CachedReference<GuiText>(EnemyDebugTextGetter);

        private void UpdateGUI()
        {
            if ((playerDebugText.cache != null) && (enemyDebugText.cache != null))
            {
                // DEBUG OUTPUT
                if (team == UnitTeam.player)
                    playerDebugText.cache.text = "Player: " + hp.value + "/" + hp.max;
                else if (team == UnitTeam.enemy)
                    enemyDebugText.cache.text = "Enemy: " + hp.value + "/" + hp.max;
                // DEBUG OUTPUT
            }
        }

        #endregion

        #region Stats

        public AudioClip attackSound;
        public AudioClip jumpSound;
        public AudioClip runSound;

        public UnitTeam team;

        public Capacitor hp;
        public Capacitor ap;

        private int move;
        private int jump;

        public int moveRange { get { return move; } }
        public int jumpRange
        {
            get
            {
                if ((jump * 2) < move) return jump * 2;
                else return move;
            }
        }

        public UnitSize size;

        public void LoadRecipe(MapUnitRecipe recipe)
        {
            spriteRenderer.sprite = recipe.sprite;

            attackSound = recipe.attackSound;
            jumpSound = recipe.jumpSound;
            runSound = recipe.runSound;

            hp = new Capacitor(recipe.maxHp);
            ap = new Capacitor(recipe.maxAp);

            move = recipe.move;
            jump = recipe.jump;

            size = recipe.size;
        }

        #endregion

        #region Areas

        public PointyHexPoint loc
        {
            get
            {
                Profiler.BeginSample("MapUnit.loc");

                PointyHexPoint result = map.WhereIs(this);

                Profiler.EndSample();

                return result;
            }
        }

        public IEnumerable<PointyHexPoint> GetHardblockArea()
        {
            if (size == UnitSize.small)
                return new List<PointyHexPoint>();
            else if (size == UnitSize.large)
                return map.GetCircle(loc, 0, 0).Where(map.InBounds);
            else
                return null;
        }

        public IEnumerable<PointyHexPoint> GetSoftblockArea()
        {
            if (size == UnitSize.small)
                return map.GetCircle(loc, 0, 0).Where(map.InBounds);
            else if (size == UnitSize.large)
                return map.GetCircle(loc, 1, 1).Where(map.InBounds);
            else
                return null;
        }

        public IEnumerable<PointyHexPoint> GetThreatArea()
        {
            if (size == UnitSize.small)
                return map.GetCircle(loc, 1, 1).Where(map.InBounds);
            else if (size == UnitSize.large)
                return map.GetArc(loc, facing.CCW(2), facing.CW(2), 2, 2).Where(map.InBounds);
            else
                return null;
        }

        public IEnumerable<PointyHexPoint> GetAttackArea()
        {
            if (size == UnitSize.small)
                return map.GetCircle(loc, 1, 1).Where(map.InBounds);
            else if (size == UnitSize.large)
                return map.GetCircle(loc, 2, 2).Where(map.InBounds);
            else
                return null;
        }

        public IEnumerable<PointyHexPoint> GetLandingArea(MapUnit target)
        {
            PointyHexPoint targetLoc = map.WhereIs(target);

            if ((target.size == UnitSize.small) && (size == UnitSize.small))
                return map.GetCircle(targetLoc, 1, 1)
                    .Where(map.InBounds);
            else
                return map.GetCircle(targetLoc, 2, 2)
                    .Where(map.InBounds);
        }

        #endregion

        #region Verbs

        public void TakeDamage(int damage)
        {
            hp.Increment(-damage);

            // DEBUG OUTPUT
            UpdateGUI();

            if (hp <= 0)
                Die();
        }

        public void Die()
        {
            map.UnplaceUnit(this);
            Destroy(gameObject);
        }

        public void Face(PointyHexPoint loc)
        {
            facing = this.loc.FacingTo(loc);
        }

        public void Face(MapUnit unit)
        {
            Face(unit.loc);
        }

        #endregion

        #region Monobehaviour

        void Start()
        {
            // DEBUG OUTPUT
            UpdateGUI();
        }

        void OnDestroy()
        {
            if (Controllers.map != null)
                Controllers.map.cache.UnplaceUnit(this);
        }

        #endregion
    }
}
