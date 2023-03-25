using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

public static partial class Extension
{
    public static string ToJsonString<T>(this IEnumerable<T> list)
    {
        return JsonConvert.SerializeObject
            (list, Formatting.Indented);
    }

    public static bool HasValue<T>(this IEnumerable<T> list)
    {
        return list != null && list.Count() > 0;
    }

    public static bool IsEmpty<T>(this IEnumerable<T> list)
    {
        return !list.HasValue();
    }
}
