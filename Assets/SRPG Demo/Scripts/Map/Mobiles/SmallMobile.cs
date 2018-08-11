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
    [AddComponentMenu("SRPG/Mobiles/Small Mobile")]
    public class SmallMobile : MapMobile
    {
        public override IEnumerable<PointyHexPoint> ThreatArea()
        {
            IEnumerable<PointyHexPoint> result = null;
            
            result = map.Map
                .GetCircle(Controllers.map.WhereIs(this), 1, 1)
                .Where(Controllers.map.InBounds);

            return result;
        }
    }
}
