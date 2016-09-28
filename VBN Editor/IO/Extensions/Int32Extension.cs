namespace System
{
    public static class Int32Extension
    {
        public static int Reverse(this int value)
        {
            return ((value >> 24) & 0xFF) | (value << 24) | ((value >> 8) & 0xFF00) | ((value & 0xFF00) << 8);
        }

        public static int Align(this int value, int align)
        {
            return (value + align - 1) / align * align;
        }

        public static int Clamp(this int value, int min, int max)
        {
            if (value <= min) return min;
            if (value >= max) return max;
            return value;
        }
    }
}
