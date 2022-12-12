namespace BeeEngine.Vector
{
    static class NumberHelper
    {
        public static float Lerp(float firstFloat, float secondFloat, float by)
        {
            return firstFloat + (secondFloat - firstFloat) * by;
        }
        public static int LerpInt(int firstInt, int secondInt, float by)
        {
            return (int)(firstInt + (secondInt - firstInt) * by);
        }
        public static int LerpInt(int firstInt, int secondInt, double by)
        {
            return (int)(firstInt + (secondInt - firstInt) * by);
        }
        public static double LerpDouble(double firstDouble, double secondDouble, double by)
        {
            return firstDouble + (secondDouble - firstDouble) * by;
        }
    }
}
