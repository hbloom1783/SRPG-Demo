using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SRPGDemo.Extensions;
using SRPGDemo.Battle.Map;
using SRPGDemo.Battle.Map.Pathing;
using GridLib.Hex;

namespace SRPGDemo.Battle.Gameplay
{
    public static class UnitAIExt
    {
        private static MapController map = MapController.instance;
        private static GameController game = GameController.instance;

        public static void PickAction(this MapUnit unit)
        {
            // If we can melee a player unit, melee one with the least HP
            IEnumerable<MapUnit> attackTargets = unit.GetAttackArea()
                .Select(map.CellAt)
                .Where(x => x.unitPresent != null)
                .Select(x => x.unitPresent)
                .Where(x => x.team != unit.team);
            if (attackTargets.Any())
            {
                MapUnit target = attackTargets.RandomPickMin(x => x.hp);

                unit.MeleeAttack(target);
            }
            else
            {
                //If there's a player unit, move towards them
                MapUnit target = map
                    .Units(UnitTeam.player)
                    .RandomPickMin(x => unit.loc.DistanceTo(map.WhereIs(x)));

                unit.MoveTowards(target);
            }
        }

        public static void MeleeAttack(this MapUnit unit, MapUnit target)
        {
            game.ChangeState(new MeleeAttack(unit, target));
        }

        public static void MoveTowards(this MapUnit unit, MapUnit target)
        {
            HexCoords targetLoc = map.WhereIs(target);

            IDictionary<HexCoords, PathNode> pathFlood = unit.Dijkstra();
            HexCoords destination = pathFlood.Keys.RandomPickMin(x => x.DistanceTo(targetLoc));

            game.ChangeState(new MoveUnit(
                unit,
                pathFlood[destination].pathTo));
        }
    }

    static class EnemyExt
    {
        public static IEnumerator EnemyLoses()
        {
            yield return null;
            GameController.instance.ChangeState(new PlayerWins());
        }
    }

    class BeginEnemyTurn : GameControllerAnimation
    {
        public override IEnumerator AnimationCoroutine()
        {
            // If the player is out of units, defeat them
            if (map.Units(UnitTeam.enemy).Count() <= 0)
            {
                game.StartCoroutine(PlayerExt.PlayerLoses());
            }
            else
            {
                // Display marquee
                ui.marqueeText.Activate();
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
                ui.marqueeText.Deactivate();

                // Remove enemy threat
                foreach (MapUnit unit in map.Units(UnitTeam.enemy))
                    foreach (MapCell cell in unit.GetThreatArea().Select(map.CellAt))
                        cell.RemoveThreat(unit);

                // Begin enemy turn
                game.ChangeState(new EnemyTurnAI());
            }
        }
    }

    class EnemyTurnAI : GameControllerAnimation
    {
        public override IEnumerator AnimationCoroutine()
        {
            // If the player is out of units, defeat them
            if (map.Units(UnitTeam.player).Count() <= 0)
            {
                game.StartCoroutine(PlayerExt.PlayerLoses());
            }
            else
            {
                // Stall for one frame to get out of transition
                yield return null;

                // Find all movable units
                IEnumerable<MapUnit> movableUnits = map.Units(UnitTeam.enemy)
                    .Where(x => x.ap > 0);

                // If we have a unit to move
                if (movableUnits.Any())
                {
                    movableUnits.RandomPick().PickAction();
                }
                // No units to move
                else
                {
                    // Turn complete
                    game.ChangeState(new EndEnemyTurn());
                }
            }
        }
    }

    class EndEnemyTurn : GameControllerAnimation
    {
        public override IEnumerator AnimationCoroutine()
        {
            // Reset player APs for next turn
            map.Units(UnitTeam.enemy).ForEach(x => x.ap.Reset());

            // Re-establish enemy threat
            foreach (MapUnit unit in map.Units(UnitTeam.enemy))
                foreach (MapCell cell in unit.GetThreatArea().Select(map.CellAt))
                    cell.AddThreat(unit);

            // Stall for one frame before changing states
            yield return null;
            
            // Pass the baton
            game.ChangeState(new BeginPlayerTurn());
        }
    }

    class EnemyWins : GameControllerAnimation
    {
        public override IEnumerator AnimationCoroutine()
        {
            yield return null;
            game.LoadSceneByName("Main Menu");
        }
    }
}
