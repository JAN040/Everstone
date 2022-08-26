using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class UnitGrid
{
    //Grid looks like this:
    //|*|*|*|
    //-------
    //|*|*|*|
    //... aka 3 columns, two rows

    public ScriptableUnitBase[,] Allies  = new ScriptableUnitBase[3, 2];
    public ScriptableUnitBase[,] Enemies = new ScriptableUnitBase[3, 2];

    public Vector2[,] AllyGridPoints = {
        { new Vector2(-7, 1),    new Vector2(-7, -1.5f) },
        { new Vector2(-4.5f, 1), new Vector2(-4.5f, -1.5f) },
        { new Vector2(-2, 1),    new Vector2(-2, -1.5f) },
    };
    public Vector2[,] EnemyGridPoints = { 
        { new Vector2(2, 1),    new Vector2(2, -1.5f) },
        { new Vector2(4.5f, 1), new Vector2(4.5f, -1.5f) },
        { new Vector2(7, 1),    new Vector2(7, -1.5f) },
    };

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
                        RecalcIdlePositions(faction);

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
                        RecalcIdlePositions(faction);

                        return true;
                    }
        }


        return false;
    }

    public bool AddToBack(Faction faction, ScriptableUnitBase unit)
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
                        RecalcIdlePositions(faction);
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
                        RecalcIdlePositions(faction);
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

        RecalcIdlePositions(faction);
    }

    public void RecalcIdlePositions(Faction faction)
    {
        if (faction == Faction.Allies)
        {
            for (int k = 0; k < Allies.GetLength(0); k++)
                for (int l = 0; l < Allies.GetLength(1); l++)
                    if (Allies[k, l] != null)
                    {
                        Allies[k, l].Prefab.transform.position = AllyGridPoints[k, l];
                        Allies[k, l].Prefab.GetComponent<Unit>().IdlePosition = AllyGridPoints[k, l];
                    }
        }

        if (faction == Faction.Enemies)
        { 
            for (int k = 0; k < Enemies.GetLength(0); k++)
                for (int l = 0; l < Enemies.GetLength(1); l++)
                    if (Enemies[k, l] != null)
                    {
                        Enemies[k, l].Prefab.transform.position = EnemyGridPoints[k, l];
                        Enemies[k, l].Prefab.GetComponent<Unit>().IdlePosition = EnemyGridPoints[k, l];
                    }
        }
    }

    public void Clear(Faction faction)
    {
        var target = faction == Faction.Allies ? Allies : Enemies;

        for (int k = 0; k < target.GetLength(0); k++)
            for (int l = 0; l < target.GetLength(1); l++)
                target[k, l] = null;
    }
}
