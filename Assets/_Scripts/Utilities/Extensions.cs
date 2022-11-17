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
        Vector3 res = new Vector3(-1f * vector.x, vector.y, vector.z);
        return res;
    }

    public static Vector3 FlipY(this Vector3 vector)
    {
        return new Vector3(vector.x, -1f * vector.y, vector.z);
    }

    public static int Round(this float num)
    {
        return (int)Math.Round(num);
    }

    /// <summary>
    /// Same as round but takes care of the < 0.5 hp scenario xD
    /// </summary>
    public static int RoundHP(this float num)
    {
        if (num < 1f && num > 0)
            return 1;
        
        return (int)Math.Round(num);
    }

    public static Unit GetUnit(this ScriptableUnitBase scriptableUnit)
    {
        if (scriptableUnit == null || scriptableUnit.Prefab == null)
            return null;

        return scriptableUnit.Prefab.GetComponent<Unit>();
    }

    //round down and convert to kilo format, ie. 1345 = 1.34K
    public static string ToKiloString(this float num)
    {
        if (num > 1000000f)
        {
            num /= 1000000f;
            return $"{num:0}M";
        }

        if (num > 1000f)
        {
            num /= 1000f;
            return $"{num:0}k";
        }

        return ((int)num).ToString();
    }

    public static string ToKiloString(this int num)
    {
        if (num > 1000000)
        {
            num /= 1000000;
            return $"{num:0}M";
        }

        if (num > 1000)
        {
            num /= 1000;
            return $"{num:0}k";
        }

        return num.ToString();
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

    /// <summary>
    /// Returns true if the item is one of the elements in the list (uses the List.Contains method)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <param name="list"></param>
    /// <returns></returns>
    public static bool In<T>(this T item, params T[] list)
    {
        return list.Contains(item);
    }
}
