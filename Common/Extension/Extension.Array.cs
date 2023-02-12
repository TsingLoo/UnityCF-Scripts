using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



public partial class Extension
{
	public static T[] Add<T>(this T[] array, T item)
	{
		array = array
			.Concat<T>(new T[1] { item }).ToArray();
		return array;
	}

    public static T[] AddRange<T>(this T[] sourceArray, T[] addArray)
    {
        sourceArray = sourceArray
            .Concat<T>(addArray).ToArray();
        return sourceArray;
    }
}

