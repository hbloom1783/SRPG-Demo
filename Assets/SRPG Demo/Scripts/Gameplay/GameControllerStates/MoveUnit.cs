using Gamelogic.Grids;
using SRPGDemo.Map;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;

namespace SRPGDemo.Gameplay
{
    class MoveUnit : GameControllerAnimation
    {
        #region Unit movement

        private MapMobile mobile;
        private IEnumerable<PointyHexPoint> path;
        private float timePer;

        public MoveUnit(
            MapMobile mobile,
            IEnumerable<PointyHexPoint> path,
            float timePer = 0.5f)
        {
            this.mobile = mobile;
            this.path = path;
            this.timePer = timePer;
        }

        public override IEnumerator AnimationCoroutine()
        {
            PointyHexPoint oldLoc = map.WhereIs(mobile);

            foreach (PointyHexPoint newLoc in path)
            {
                for (float timePassed = 0.0f;
                    timePassed < timePer;
                    timePassed += Time.deltaTime)
                {
                    mobile.transform.position = Vector3.Lerp(
                        map.mapGrid[oldLoc].transform.position,
                        map.mapGrid[newLoc].transform.position,
                        timePassed / timePer);

                    yield return null;
                }

                oldLoc = newLoc;
            }

            map.UnplaceMobile(mobile);
            map.PlaceMobile(mobile, map.mapGrid[oldLoc]);

            turn.StateTransition(new PlayerSelectUnit());
        }

        #endregion
    }
}
