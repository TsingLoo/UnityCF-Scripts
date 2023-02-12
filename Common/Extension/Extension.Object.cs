using System.Reflection;
using System.Runtime.Serialization.Json;

public static partial class Extension
{
    private static BindingFlags bindingFlags { get; }
        = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static;


    public static bool ContainsProperty(this object obj, string propertyName)
    {
        return obj.GetType().GetProperty(propertyName, bindingFlags) != null;
    }

    public static object GetPropertyValue(this object obj, string propertyName)
    {
        return obj.GetType().GetProperty(propertyName, bindingFlags).GetValue(obj);
    }

    public static void SetPropertyValue(this object obj, string propertyName, object value)
    {
        obj.GetType().GetProperty(propertyName, bindingFlags).SetValue(obj, value);
    }

    public static bool ContainsField(this object obj, string fieldName)
    {
        return obj.GetType().GetField(fieldName, bindingFlags) != null;
    }

    public static object GetFieldValue(this object obj, string fieldName)
    {
        return obj.GetType().GetField(fieldName, bindingFlags).GetValue(obj);
    }

    public static void SetFieldValue(this object obj, string fieldName, object value)
    {
        obj.GetType().GetField(fieldName, bindingFlags).SetValue(obj, value);
    }

    public static MethodInfo GetMethod(this object obj, string methodName)
    {
        return obj.GetType().GetMethod(methodName, bindingFlags);
    }

    // End
}