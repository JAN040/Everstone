using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitGrid
{
    //Grid looks like this:
    //|*|*|*|
    //-------
    //|*|*|*|
    //... aka 3 columns, two rows

    public ScriptableUnitBase[,] Allies  = new ScriptableUnitBase[3, 2];
    public ScriptableUnitBase[,] Enemies = new ScriptableUnitBase[3, 2];


    /// <summary>
    /// Tries to add the unit from the front to the grid in question.
    /// If a free spot is not found in the first two cols,
    ///     it doesnt add the unit and returns false.
    /// </summary>
    /// <returns>True on sucessful add</returns>
    public bool AddToFront(Faction faction, ScriptableUnitBase unit)
    {
        var target = faction == Faction.Allies ? Allies : Enemies;

        if (faction == Faction.Allies)
        {
            //for the allies grid, the "front" is the rightmost column
            for (int k = target.GetLength(0) - 1; k >= 1; k--)
                for (int l = 0; l < target.GetLength(1); l++)
                    if (target[k, l] == null)
                    {
                        target[k, l] = unit;
                        return true;
                    }
        }
        else
        {
            for (int k = 0; k < target.GetLength(0) - 1; k++)  //only first two cols are valid for adding
                for (int l = 0; l < target.GetLength(1); l++)
                    if (target[k, l] == null)
                    {
                        target[k, l] = unit;
                        return true;
                    }
        }

        return false;
    }

    private bool AddToBack(Faction faction, ScriptableUnitBase unit)
    {
        var target = faction == Faction.Allies ? Allies : Enemies;

        if (faction == Faction.Allies)
        {
            //for the allies grid, the "back" is the leftmost column
            for (int k = 0; k < target.GetLength(0) - 1; k++)  //only first two cols are valid for adding
                for (int l = 0; l < target.GetLength(1); l++)
                    if (target[k, l] == null)
                    {
                        target[k, l] = unit;
                        return true;
                    }
        }
        else
        {
            for (int k = target.GetLength(0) - 1; k >= 1; k--)
                for (int l = 0; l < target.GetLength(1); l++)
                    if (target[k, l] == null)
                    {
                        target[k, l] = unit;
                        return true;
                    }
        }

        return false;
    }

    /// <summary>
    /// Returns true if the grid in question is full, false otherwise
    /// </summary>
    public bool IsFull(Faction faction)
    {
        var target = faction == Faction.Allies ? Allies : Enemies;

        for (int k = 0; k < target.GetLength(0); k++)
            for (int l = 0; l < target.GetLength(1); l++)
                if (target[k, l] != null)
                    return false;

        return true;
    }

    public void Restructure(Faction faction)
    {
        var target = faction == Faction.Allies ? Allies : Enemies;

        if (faction == Faction.Allies)
        {
            //if we find an empty column we pull the column behind forward
            for (int k = target.GetLength(0) - 1; k >= 1; k--) //dont have to check the first col
            {
                if (target[k, 0] == null && target[k, 1] == null)
                {
                    target[k, 0] = target[k - 1, 0];
                    target[k, 1] = target[k - 1, 1];
                    target[k - 1, 0] = null;
                    target[k - 1, 1] = null;
                }
            }
        }
        else
        {
            //if we find an empty column we pull the column behind forward
            for (int k = 0; k < target.GetLength(0) - 1; k++) //dont have to check the last col
            {
                if (target[k, 0] == null && target[k, 1] == null)
                {
                    target[k, 0] = target[k + 1, 0];
                    target[k, 1] = target[k + 1, 1];
                    target[k + 1, 0] = null;
                    target[k + 1, 1] = null;
                }
            }
        }
                    
    }
}
