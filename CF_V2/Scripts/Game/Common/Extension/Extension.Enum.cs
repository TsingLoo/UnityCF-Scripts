using System;
using System.ComponentModel;
using System.Reflection;

public static partial class Extension
{
    public static int GetIntValue(this Enum input)
    {
        return input.GetHashCode();
    }

    /// <summary>
    /// Get Enum Code
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string GetCode(this Enum input)
    {
        return input.ToString();
    }

    /// <summary>
    /// Get Enum Description
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string GetDescription(this Enum input)
    {
        if (input == null)
            return "";

        FieldInfo fieldInfo = input.GetType().GetField(input.ToString());
        if (fieldInfo == null)
            return string.Empty;

        object[] attribArray = fieldInfo.GetCustomAttributes(false);
        if (attribArray.Length == 0)
            return input.ToString();
        else
            return ((DescriptionAttribute)attribArray[0]).Description;
    }
}
