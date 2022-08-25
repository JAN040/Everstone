using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public static class Extensions
{
    public static int ToLevel(this int number)
    {
        return number > 0 ? number : 1;
    }

    public static Vector3 FlipX(this Vector3 vector)
    {
        return new Vector3(-1f * vector.x, vector.y, vector.z);
    }

    public static Vector3 FlipY(this Vector3 vector)
    {
        return new Vector3(vector.x, -1f * vector.y, vector.z);
    }


    /// <summary>
    /// Gets the next value of the enum (numerically)
    ///     if current value is the max, it returns the lowest value.
    /// </summary>
    public static T Next<T>(this T src) where T : Enum
    {
        if (!typeof(T).IsEnum) throw new ArgumentException(String.Format("Argument {0} is not an Enum", typeof(T).FullName));

        T[] Arr = (T[])Enum.GetValues(src.GetType());
        int j = Array.IndexOf<T>(Arr, src) + 1;
        return (Arr.Length == j) ? Arr[0] : Arr[j];
    }

    /// <summary>
    /// Gets the next value of the enum (numerically)
    ///     if current value is the max it returns current value.
    /// </summary>
    public static T NextOrSame<T>(this T src) where T : Enum
    {
        if (!typeof(T).IsEnum) throw new ArgumentException(String.Format("Argument {0} is not an Enum", typeof(T).FullName));

        T[] Arr = (T[])Enum.GetValues(src.GetType());
        int j = Array.IndexOf<T>(Arr, src) + 1;
        return (Arr.Length == j) ? src : Arr[j];
    }
}
