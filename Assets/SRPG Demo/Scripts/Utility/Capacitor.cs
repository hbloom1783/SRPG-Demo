using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SRPGDemo
{
    public class Capacitor
    {
        private int max;
        private int current;

        public Capacitor(int start, int max)
        {
            this.max = max;
            current = start;
        }

        public Capacitor(int max)
        {
            this.max = current = max;
        }

        public float fraction
        {
            get { return current / max; }
            set { current = Mathf.RoundToInt(max * value); }
        }

        public void Reset()
        {
            current = max;
        }

        public void Increment(int value)
        {
            current += value;
        }

        public static implicit operator int(Capacitor cap)
        {
            return cap.current;
        }
    }
}
