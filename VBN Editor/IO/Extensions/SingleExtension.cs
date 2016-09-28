namespace System
{
    public static class SingleExtension
    {
        public static float Reverse(this float value)
        {
            return ((uint)value).Reverse();
        }
    }
}
