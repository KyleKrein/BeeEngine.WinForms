namespace BeeEngine;

public static class Time
{
    internal static double _deltaTime;
    static double Secondframe;
    static readonly Stopwatch stopWatch = new Stopwatch();
    internal static readonly double _fixedDeltaTime = 20;
    public static double fixedDeltaTime
    {
        get
        {
            return _fixedDeltaTime / 1000;
        }
    }

    public static double time
    {
        get
        {
            return Secondframe/1000;
        }
    }

    public static double deltaTime
    {
        get
        {
            return _deltaTime/ 1000;
        }
    }

    internal static double testDeltaTime
    {
        get
        {
            TimeSpan ts = stopWatch.Elapsed;
            double FirstFrame = ts.TotalMilliseconds;

            return FirstFrame - Secondframe;
        }
    }

    static Time()
    {
        Secondframe = 0;
        stopWatch.Start();
    }
    internal static void UpdateDeltaTime()
    {
        TimeSpan ts = stopWatch.Elapsed;
        double FirstFrame = ts.TotalMilliseconds;

        _deltaTime = FirstFrame - Secondframe;
        Secondframe = ts.TotalMilliseconds;
    }
}
