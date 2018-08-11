using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Gamelogic.Grids;
using SRPGDemo.Extensions;
using SRPGDemo.Map;

namespace SRPGDemo.Gameplay
{
    class BeginEnemyTurn : GameControllerAnimation
    {
        public override IEnumerator AnimationCoroutine()
        {
            // Display marquee
            ui.marqueeText.gameObject.SetActive(true);
            ui.marqueeText.text = "Enemy Turn";
            ui.marqueeText.color = Color.white.Alpha(0.0f);

            for (float timePassed = 0.0f; timePassed < 1.0f; timePassed += Time.deltaTime)
            {
                ui.marqueeText.color = Color.Lerp(Color.white.Alpha(0.0f), Color.red, timePassed / 1.0f);
                yield return null;
            }

            for (float timePassed = 0.0f; timePassed < 1.0f; timePassed += Time.deltaTime)
            {
                ui.marqueeText.color = Color.Lerp(Color.red, Color.red.Alpha(0.0f), timePassed / 1.0f);
                yield return null;
            }

            // Clear marquee
            ui.marqueeText.gameObject.SetActive(false);

            // Begin enemy turn
            game.StateTransition(new EnemyTurnAI());
        }
    }

    class EnemyTurnAI : GameControllerAnimation
    {
        public override IEnumerator AnimationCoroutine()
        {
            // Stall for one frame to get out of transition
            yield return null;

            // Find all movable units
            IEnumerable<MapMobile> movableUnits = map.Mobiles()
                .Where(x => x.team == MobileTeam.enemy)
                .Where(x => x.actions > 0);

            // If we have a unit to move
            if (movableUnits.Count() > 0)
            {
                MapMobile mobile = movableUnits.RandomPick();
                PointyHexPoint mobileLoc = map.WhereIs(mobile);

                IEnumerable<MapMobile> targets = map.Mobiles()
                    .Where(x => x.team == MobileTeam.player);
                MapMobile target = targets.RandomPickMin(x => mobileLoc.DistanceFrom(map.WhereIs(x)));
                PointyHexPoint targetLoc = map.WhereIs(target);

                Dictionary<PointyHexPoint, PathNode> pathFlood = mobile.PathFlood();
                PointyHexPoint destination = pathFlood.Keys.RandomPickMin(x => x.DistanceFrom(targetLoc));

                game.StateTransition(new MoveUnit(
                    mobile,
                    pathFlood[destination].PathTo().Select(x => x.loc),
                    new EnemyTurnAI()));
            }
            // No units to move
            else
            {
                // Turn complete
                game.StateTransition(new EndEnemyTurn());
            }
        }
    }

    class EndEnemyTurn : GameControllerAnimation
    {
        public override IEnumerator AnimationCoroutine()
        {
            // Reset player APs for next turn
            map.Mobiles().Where(x => x.team == MobileTeam.enemy).ForEach(x => x.actions.Reset());

            // Stall for one frame before changing states
            yield return null;
            
            // Pass the baton
            game.StateTransition(new BeginPlayerTurn());
        }
    }
}
