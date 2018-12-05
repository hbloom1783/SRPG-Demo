using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;
using GridLib.Hex;
using SRPGDemo.Battle.UI;

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

        private HexFacing _facing = HexFacing.bad;
        public HexFacing facing
        {
            get { return _facing; }
            set
            {
                _facing = value;
                //if (map.HasUnit(this)) map.ReseatUnit(this);
                UpdatePresentation();
            }
        }

        private void UpdatePresentation()
        {
            if ((facing == HexFacing.yz) ||
                (facing == HexFacing.xz) ||
                (facing == HexFacing.xy))
                spriteRenderer.flipX = false;

            if ((facing == HexFacing.yx) ||
                (facing == HexFacing.zx) ||
                (facing == HexFacing.zy))
                spriteRenderer.flipX = true;
        }

        #endregion

        #region Temporary GUI stuff

        private void UpdateGUI()
        {
            // DEBUG OUTPUT
            if (team == UnitTeam.player)
                GuiController.instance.playerDebugText.text = "Player: " + hp.value + "/" + hp.max;
            else if (team == UnitTeam.enemy)
                GuiController.instance.enemyDebugText.text = "Enemy: " + hp.value + "/" + hp.max;
            // DEBUG OUTPUT
        }

        #endregion

        #region Stats

        public AudioClip attackSound;
        public AudioClip jumpSound;
        public AudioClip runSound;

        public UnitTeam team;

        public Capacitor hp;
        public Capacitor ap;

        private uint move;
        private uint jump;

        public uint moveRange { get { return move; } }
        public uint jumpRange
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
        
        public void ResetForPool()
        {
            spriteRenderer.sprite = null;

            attackSound = null;
            jumpSound = null;
            runSound = null;

            hp = null;
            ap = null;

            move = 0;
            jump = 0;

            size = UnitSize.small;
        }

        #endregion

        #region Areas

        public HexCoords loc
        {
            get
            {
                Profiler.BeginSample("Battle.MapUnit.loc");

                HexCoords result = map.WhereIs(this);

                Profiler.EndSample();

                return result;
            }
        }

        public IEnumerable<HexCoords> GetHardblockArea()
        {
            if (size == UnitSize.small)
                return new List<HexCoords>();
            else if (size == UnitSize.large)
                return loc.Ring(0).Where(map.InBounds);
            else
                return null;
        }

        public IEnumerable<HexCoords> GetSoftblockArea()
        {
            if (size == UnitSize.small)
                return loc.Ring(0).Where(map.InBounds);
            else if (size == UnitSize.large)
                return loc.Ring(1).Where(map.InBounds);
            else
                return null;
        }

        public IEnumerable<HexCoords> GetThreatArea()
        {
            if (size == UnitSize.small)
                return loc.Ring(1).Where(map.InBounds);
            else if (size == UnitSize.large)
            {
                var b = facing.Rotate(-2);
                var e = facing.Rotate(2);
                var q = loc.Arc(2, facing.Rotate(-2), facing.Rotate(2)).Where(map.InBounds).ToList();
                return loc.Arc(2, facing.Rotate(-2), facing.Rotate(2)).Where(map.InBounds);
            }
            else
                return null;
        }

        public IEnumerable<HexCoords> GetAttackArea()
        {
            if (size == UnitSize.small)
                return loc.Ring(1).Where(map.InBounds);
            else if (size == UnitSize.large)
                return loc.Ring(2).Where(map.InBounds);
            else
                return null;
        }

        public IEnumerable<HexCoords> GetLandingArea(MapUnit target)
        {
            HexCoords targetLoc = map.WhereIs(target);

            if ((target.size == UnitSize.small) && (size == UnitSize.small))
                return targetLoc.Ring(1).Where(map.InBounds);
            else
                return targetLoc.Ring(2).Where(map.InBounds);
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

        public void Face(HexCoords loc)
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
            if (MapController.instance != null)
                MapController.instance.UnplaceUnit(this);
        }

        #endregion
    }
}
