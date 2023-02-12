using System;

public static partial class Extension 
{
    public static bool HasValue(this float value)
    {
        return Math.Abs(value) > 0.01;
    }

}
