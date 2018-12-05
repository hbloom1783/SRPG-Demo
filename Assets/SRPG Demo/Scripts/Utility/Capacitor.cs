using UnityEngine;

namespace SRPGDemo
{
    public class Capacitor
    {
        private int _max;
        private int _min;
        private int current;

        public Capacitor(int start, int max, int min)
        {
            _max = max;
            _min = min;
            current = start;
        }

        public Capacitor(int start, int max)
        {
            _max = max;
            _min = 0;
            current = start;
        }

        public Capacitor(int startMax)
        {
            _max = current = startMax;
            _min = 0;
        }

        public float fraction
        {
            get { return current / max; }
            set { current = Mathf.RoundToInt(max * value); }
        }

        public Capacitor Clamp()
        {
            if (current > _max) current = _max;
            else if (current < _min) current = _min;
            return this;
        }

        public Capacitor Reset()
        {
            current = _max;
            return this;
        }

        public Capacitor Increment(int value)
        {
            current += value;
            return this;
        }

        public static implicit operator int(Capacitor cap)
        {
            return cap.current;
        }

        public int value { get { return current; } }
        public int max { get { return _max; } }
    }
}
