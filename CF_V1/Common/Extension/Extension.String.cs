using System;

public static partial class Extension 
{

    public static bool IsValid(this string input)
    {
        return !string.IsNullOrEmpty(input);
    }

    public static bool IsNotValid(this string input)
    {
        return !input.IsValid();
    }
}
