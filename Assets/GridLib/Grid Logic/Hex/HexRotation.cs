namespace GridLib.Hex
{
    public class HexRotation
    {
        private uint _CWcount;

        public int CWcount
        {
            get { return (int)_CWcount; }
            set
            {
                _CWcount = NumUtil.WrapPos(value, 6);
            }
        }

        public HexRotation(int CWcount)
        {
            this.CWcount = CWcount;
        }

        public static readonly HexRotation CW = new HexRotation(1);
        public static readonly HexRotation Neutral = new HexRotation(0);
        public static readonly HexRotation CCW = new HexRotation(-1);

        public static HexRotation operator+(HexRotation left, HexRotation right)
        {
            return new HexRotation(left.CWcount + right.CWcount);
        }

        public static HexRotation operator *(HexRotation left, int right)
        {
            return new HexRotation(left.CWcount * right);
        }

        public static HexRotation operator *(int left, HexRotation right)
        {
            return right * left;
        }
    }
}
