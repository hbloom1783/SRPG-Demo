using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GridLib.Hex
{
    public enum HexFacing
    {
        xy = 0,
        xz = 1,
        yz = 2,
        yx = 3,
        zx = 4,
        zy = 5,

        fSE = xy,
        fNE = xz,
        fN  = yz,
        fNW = yx,
        fSW = zx,
        fS  = zy,

        pSE = xy,
        pE  = xz,
        pNE = yz,
        pNW = yx,
        pW  = zx,
        pSW = zy,

        bad = -1,
    }

    public static class HexFacingMethods
    {
        public static HexFacing random { get { return HexFacing.xy.Rotate(Random.Range(0, 5)); } }

        #region Rotation

        public static HexFacing Apply(this HexFacing facing, HexRotation rot)
        {
            if (facing == HexFacing.bad) return HexFacing.bad;
            else
            {
                return (HexFacing)NumUtil.WrapPos((int)facing - rot.CWcount, 6);
            }
        }

        public static HexFacing Rotate(this HexFacing facing, int CWcount)
        {
            return facing.Apply(new HexRotation(CWcount));
        }

        #endregion

        #region Compass turns

        public static IEnumerable<HexFacing> CompassTurnCW(this HexFacing facing, HexFacing final)
        {
            HexFacing current = facing;

            while (current != final)
            {
                yield return current;
                current = current.Apply(HexRotation.CW);
            }

            yield return final;
        }

        public static IEnumerable<HexFacing> CompassTurnCW(this HexFacing facing, HexRotation rot)
        {
            return facing.CompassTurnCW(facing.Apply(rot));
        }

        public static IEnumerable<HexFacing> CompassTurnCW(this HexFacing facing)
        {
            return facing.CompassTurnCW(HexRotation.CCW);
        }

        public static IEnumerable<HexFacing> CompassTurnCCW(this HexFacing facing, HexFacing final)
        {
            HexFacing current = facing;

            while (current != final)
            {
                yield return current;
                current = current.Apply(HexRotation.CCW);
            }

            yield return final;
        }

        public static IEnumerable<HexFacing> CompassTurnCCW(this HexFacing facing, HexRotation rot)
        {
            return facing.CompassTurnCCW(facing.Apply(rot));
        }

        public static IEnumerable<HexFacing> CompassTurnCCW(this HexFacing facing)
        {
            return facing.CompassTurnCCW(HexRotation.CCW);
        }

        #endregion

        #region Spatial relationships

        public static HexCoords ToOffset(this HexFacing facing)
        {
            switch(facing)
            {
                case HexFacing.xy: return HexCoords.XY;
                case HexFacing.xz: return HexCoords.XZ;
                case HexFacing.yz: return HexCoords.YZ;
                case HexFacing.yx: return HexCoords.YX;
                case HexFacing.zx: return HexCoords.ZX;
                case HexFacing.zy: return HexCoords.ZY;
                default: return HexCoords.O;
            }
        }

        #endregion
    }

    [Serializable]
    public class HexCoords
    {
        #region Internal state

        public readonly int x, y;

        public int z { get { return -x - y; } }

        public HexCoords(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public override string ToString()
        {
            return "(" + x + ", " + y + ")";
        }

        #endregion

        #region Operators

        public static HexCoords operator +(HexCoords left, HexCoords right)
        {
            return new HexCoords(
                left.x + right.x,
                left.y + right.y);
        }

        public static HexCoords operator -(HexCoords left, HexCoords right)
        {
            return new HexCoords(
                left.x - right.x,
                left.y - right.y);
        }

        public static HexCoords operator *(HexCoords left, int right)
        {
            return new HexCoords(
                left.x * right,
                left.y * right);
        }

        public static HexCoords operator +(HexCoords left, HexFacing right)
        {
            return left + right.ToOffset();
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is HexCoords)) return false;
            else
            {
                HexCoords other = (HexCoords)obj;
                return (x == other.x) && (y == other.y);
            }
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash *= (23 + x.GetHashCode());
                hash *= (23 + y.GetHashCode());
                return hash;
            }
        }

        public static bool operator ==(HexCoords left, HexCoords right)
        {
            if ((object)left == null) return (object)right == null;
            else return left.Equals(right);
        }

        public static bool operator !=(HexCoords left, HexCoords right)
        {
            return !(left == right);
        }

        #endregion

        #region Static definitions

        public static HexCoords XY = new HexCoords( 1, -1);
        public static HexCoords XZ = new HexCoords( 1,  0);
        public static HexCoords YZ = new HexCoords( 0,  1);
        public static HexCoords YX = new HexCoords(-1,  1);
        public static HexCoords ZX = new HexCoords(-1,  0);
        public static HexCoords ZY = new HexCoords( 0, -1);

        public static HexCoords O = new HexCoords(0, 0);

        #endregion

        #region Measurements

        public bool Adjacent(HexCoords other)
        {
            return DistanceTo(other) == 1;
        }

        public int DistanceTo(HexCoords other)
        {
            HexCoords d = other - this;

            return (Math.Abs(d.x) + Math.Abs(d.y) + Math.Abs(d.z)) / 2;
        }
        
        public static int Distance(HexCoords left, HexCoords right)
        {
            return left.DistanceTo(right);
        }

        public HexFacing FacingTo(HexCoords other)
        {
            if (DistanceTo(other) == 0) return HexFacing.bad;
            else return (LerpTo(other, 1.0f / DistanceTo(other)) - this).ToFacing();
        }

        public static HexCoords Round(float x, float y, float z)
        {
            int rx = (int)Math.Round(x);
            int ry = (int)Math.Round(y);
            int rz = (int)Math.Round(z);

            float dx = Math.Abs(rx - x);
            float dy = Math.Abs(ry - y);
            float dz = Math.Abs(rz - z);

            if ((dx > dy) && (dx > dz))
                rx = -ry - rz;
            else if (dy > dz)
                ry = -rx - rz;

            return new HexCoords(rx, ry);
        }

        public static HexCoords Round(Vector3 orig)
        {
            return Round(orig.x, orig.y, orig.z);
        }

        public static HexCoords Round(Vector2 orig)
        {
            return Round(orig.x, orig.y, -orig.x - orig.y);
        }

        public HexFacing ToFacing()
        {
            if (this == XY) return HexFacing.xy;
            if (this == XZ) return HexFacing.xz;
            if (this == YZ) return HexFacing.yz;
            if (this == YX) return HexFacing.yx;
            if (this == ZX) return HexFacing.zx;
            if (this == ZY) return HexFacing.zy;

            Debug.Log("Tried to convert " + this + " to a facing!");
            return HexFacing.bad;
        }

        #endregion

        #region One-to-one relationships

        public HexCoords Rotate(HexCoords center, HexRotation rot)
        {
            HexCoords d = this - center;

            switch (rot.CWcount)
            {
                default:
                case 0: return center + d; //          x,  y,      z
                case 1: return center + new HexCoords(-z, -x); // -y
                case 2: return center + new HexCoords( y,  z); //  x
                case 3: return center + new HexCoords(-x, -y); // -z
                case 4: return center + new HexCoords( z,  x); //  y
                case 5: return center + new HexCoords(-y, -z); // -x
            }
        }

        public HexCoords Rotate(HexRotation rot)
        {
            return Rotate(HexCoords.O, rot);
        }

        public HexCoords LerpTo(HexCoords other, float t)
        {
            const float eX =  1E-6F;
            const float eY =  2E-6F;
            const float eZ = -3E-6F;

            return Round(
                NumUtil.Lerp(x + eX, other.x + eX, t),
                NumUtil.Lerp(y + eY, other.y + eY, t),
                NumUtil.Lerp(z + eZ, other.z + eZ, t));
        }

        public static HexCoords Lerp(HexCoords left, HexCoords right, float t)
        {
            return left.LerpTo(right, t);
        }
        
        #endregion

        #region One-to-many relationships

        public IEnumerable<HexCoords> neighbors { get { return Ring(1); } }

        public IEnumerable<HexCoords> Ring(uint radius)
        {
            if (radius == 0)
            {
                yield return this;
            }
            else
            {
                HexCoords cursor = this + (HexFacing.yz.ToOffset() * (int)radius);

                foreach(HexFacing facing in HexFacing.xy.CompassTurnCW())
                {
                    for (int idx = 0; idx < radius; idx++)
                    {
                        cursor += facing;
                        yield return cursor;
                    }
                }
            }
        }

        public IEnumerable<HexCoords> CompoundRing(uint minRadius, uint maxRadius)
        {
            for (uint radius = minRadius; radius <= maxRadius; radius++)
            {
                foreach (HexCoords loc in Ring(radius))
                    yield return loc;
            }
        }

        public IEnumerable<HexCoords> Arc(uint radius, HexFacing begin, HexFacing end)
        {
            HexCoords cursor = this + (begin.ToOffset() * (int)radius);

            // Yielding the initial cursor position is unique to arcs,
            // and is why this code works at radius 0 while rings do not.
            yield return cursor;

            // We need to track our travel facing.
            // When following a circle, we start at the southwest corner heading north.
            // Southwest to north is rotating CW twice.
            // Any corner you start at, your travel on that side will be CW twice.
            HexRotation turnIntoSide = new HexRotation(2);

            foreach (HexFacing facing in begin.Rotate(2).CompassTurnCW(end.Rotate(1)))
            {
                for (int idx = 0; idx < radius; idx++)
                {
                    cursor += facing;
                    yield return cursor;
                }
            }
        }

        public IEnumerable<HexCoords> CompoundArc(uint minRadius, uint maxRadius, HexFacing begin, HexFacing end)
        {
            for (uint radius = minRadius; radius <= maxRadius; radius++)
            {
                foreach (HexCoords loc in Arc(radius, begin, end))
                    yield return loc;
            }
        }

        public IEnumerable<HexCoords> LineTo(HexCoords other)
        {
            int dist = DistanceTo(other);

            if (dist == 0)
                yield return other;
            else
            {
                // 0 step is this, dist step is other
                for (int step = 0; step <= dist; step++)
                {
                    yield return LerpTo(other, step / (float)dist);
                }
            }
        }

        public static IEnumerable<HexCoords> Line(HexCoords left, HexCoords right)
        {
            return left.LineTo(right);
        }
        
        #endregion
    }
}
