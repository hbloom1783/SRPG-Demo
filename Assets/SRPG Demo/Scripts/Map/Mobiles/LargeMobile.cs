using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gamelogic.Grids;
using Gamelogic.Extensions;
using SRPGDemo.Extensions;
using UnityEngine;

namespace SRPGDemo.Map
{
    [AddComponentMenu("SRPG/Mobiles/Large Mobile")]
    public class LargeMobile : MapMobile
    {
        private Facing _facing = Facing.e;
        public Facing facing
        {
            get { return _facing; }
            set
            {
                _facing = value;
                UpdatePresentation();
            }
        }

        public void UpdatePresentation()
        {
            if (facing == Facing.e || facing == Facing.ne || facing == Facing.se)
                spriteRenderer.flipX = false;
            else
                spriteRenderer.flipX = true;
        }

        public override IEnumerable<PointyHexPoint> ThreatArea()
        {
            IEnumerable<PointyHexPoint> result = null;

            result = Controllers.map.Map
                .GetArc(
                    map.WhereIs(this),
                    facing.CCW(2),
                    facing.CW(2),
                    2, 2)
                .Where(map.InBounds);

            return result;
        }
    }
}
