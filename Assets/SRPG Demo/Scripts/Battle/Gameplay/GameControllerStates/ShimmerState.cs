using SRPGDemo.Battle.Map;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SRPGDemo.Battle.Gameplay
{
    class ShimmerState : GameControllerState
    {
        private Coroutine shimmerCoroutine;
        public override void EnterState()
        {
            InitShimmer();
            shimmerCoroutine = game.StartCoroutine(TrackShimmer());
            base.EnterState();
        }

        public override void LeaveState()
        {
            base.LeaveState();
            ClearShimmer(shimmerCells);
            game.StopCoroutine(shimmerCoroutine);
        }

        protected virtual void InitShimmer()
        {
            // Nothing by default
        }

        private List<MapCell> shimmerCells = new List<MapCell>();
        
        protected float cycleTime = 0.25f;
        private float shimmerLerp = 0.0f;

        private IEnumerator TrackShimmer()
        {
            while (true)
            {
                // Cycle from 0 to cycleTime
                for (float t = 0.0f; t < cycleTime; t += Time.deltaTime)
                {
                    // Calculate shimmer value
                    shimmerLerp = t / cycleTime;

                    // Distribute shimmer value
                    SetShimmer(shimmerCells);

                    // Yield until next frame
                    yield return null;
                }

                // Cycle from cycleTime to 0
                for (float t = cycleTime; t > 0.0f; t -= Time.deltaTime)
                {
                    // Calculate shimmer value
                    shimmerLerp = t / cycleTime;

                    // Distribute shimmer value
                    SetShimmer(shimmerCells);

                    // Yield until next frame
                    yield return null;
                }
            }
        }

        private void SetShimmer(MapCell cell)
        {
            cell.tintLerp = shimmerLerp;
        }

        private void SetShimmer(IEnumerable<MapCell> cells)
        {
            foreach (MapCell cell in cells)
                SetShimmer(cell);
        }

        private void ClearShimmer(MapCell cell)
        {
            cell.tintLerp = 0.0f;
        }

        private void ClearShimmer(IEnumerable<MapCell> cells)
        {
            foreach (MapCell cell in cells)
                ClearShimmer(cell);
        }

        protected void AddShimmer(MapCell cell)
        {
            shimmerCells.Add(cell);
            SetShimmer(cell);
        }

        protected void AddShimmer(IEnumerable<MapCell> cells)
        {
            foreach (MapCell cell in cells)
                AddShimmer(cell);
        }

        protected void RemoveShimmer(MapCell cell)
        {
            shimmerCells.Remove(cell);
            ClearShimmer(cell);
        }

        protected void RemoveShimmer(IEnumerable<MapCell> cells)
        {
            foreach (MapCell cell in cells)
                RemoveShimmer(cell);
        }
    }
}
