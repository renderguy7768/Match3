namespace Assets.Scripts.Match3
{
    public static class Utility
    {
        public static void Swap<T>(ref T left, ref T right)
        {
            var temp = left;
            left = right;
            right = temp;
        }
    }
}
