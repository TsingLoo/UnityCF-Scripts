using Humanizer;
using System;
using System.Linq;

public static partial class Extension 
{
    #region Camel - Humanizer
    // https://github.com/Humanizr/Humanizer

    /// <summary>
    /// "some_title for" => "someTitleFor", –°Õ’∑Â
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string ToLowerCamel(this string input)
    {
        return input.Camelize();
    }

    /// <summary>
    /// "some_title for" => "SomeTitleFor", ¥ÛÕ’∑Â
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string ToUpperCamel(this string input)
    {
        return input.Pascalize();
    }
    #endregion

    public static string FirstToUpper(this string input)
    {
        return input.First().ToString().ToUpper() + input.Substring(1);
    }
    public static string FirstToLower(this string input)
    {
        return input.First().ToString().ToLower() + input.Substring(1);
    }

    public static bool IsValid(this string input)
    {
        return !string.IsNullOrEmpty(input);
    }

    public static bool IsNotValid(this string input)
    {
        return !input.IsValid();
    }
}
