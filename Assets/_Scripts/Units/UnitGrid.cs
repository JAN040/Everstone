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

    private const float x1 = 2f;
    private const float x2 = 4.5f;
    private const float x3 = 7f;

    private const float y1   = 1f;
    private const float y1_5 = -0.25f;  //the "middle" row (when only one enemy in the column)
    private const float y2   = -1.5f;

    public Vector2[,] AllyGridPoints = {
        { new Vector2(-x3, y1),    new Vector2(-x3, y2) },
        { new Vector2(-x2, y1),    new Vector2(-x2, y2) },
        { new Vector2(-x1, y1),    new Vector2(-x1, y2) },
    };
    public Vector2[,] EnemyGridPoints = { 
        { new Vector2(x1, y1),    new Vector2(x1, y2) },
        { new Vector2(x2, y1),    new Vector2(x2, y2) },
        { new Vector2(x3, y1),    new Vector2(x3, y2) },
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

    /// <summary>
    /// Try to fix the unit positions into a valid formation (no empty front or middle lines)
    /// </summary>
    public void Restructure(Faction faction)
    {
        var target = faction == Faction.Allies ? Allies : Enemies;
        
        //if we find an empty column we pull the column behind forward
        if (faction == Faction.Allies)
        {
            //run it twice to cover the special case of only the last col having units
            for (int i = 0; i < 2; i++)
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
            for (int i = 0; i < 2; i++)
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

    /// <summary>
    /// Reassigns unit prefab Idle positions based on their grid position
    /// </summary>
    public void RecalcIdlePositions(Faction faction)
    {
        Vector2 assignedPosition;

        if (faction == Faction.Allies)
        {
            for (int k = 0; k < Allies.GetLength(0); k++)
                for (int l = 0; l < Allies.GetLength(1); l++)
                    if (Allies[k, l] != null)
                    {
                        assignedPosition = AllyGridPoints[k, l];

                        //if this is the only unit in this column
                        if (l == 0 && Allies[k, 1] == null ||
                            l == 1 && Allies[k, 0] == null)
                            assignedPosition.y = y1_5;  //assign him the "middle" row

                        AssignPosition(Allies[k, l], assignedPosition);
                    }
        }

        if (faction == Faction.Enemies)
        { 
            for (int k = 0; k < Enemies.GetLength(0); k++)
                for (int l = 0; l < Enemies.GetLength(1); l++)
                    if (Enemies[k, l] != null)
                    {
                        assignedPosition = EnemyGridPoints[k, l];
                        
                        //if this is the only unit in this column
                        if (l == 0 && Enemies[k, 1] == null ||
                            l == 1 && Enemies[k, 0] == null)
                            assignedPosition.y = y1_5;  //assign him the "middle" row

                        AssignPosition(Enemies[k, l], assignedPosition);
                    }
        }
    }

    private void AssignPosition(ScriptableUnitBase unit, Vector2 position)
    {
        unit.Prefab.transform.position = position;
        unit.Prefab.GetComponent<Unit>().IdlePosition = position;
    }

    public void Clear(Faction faction)
    {
        var target = faction == Faction.Allies ? Allies : Enemies;

        for (int k = 0; k < target.GetLength(0); k++)
            for (int l = 0; l < target.GetLength(1); l++)
                target[k, l] = null;
    }

    public ScriptableUnitBase GetDefaultTarget(Faction faction)
    {
        var target = faction == Faction.Allies ? Allies : Enemies;
        ScriptableUnitBase firstRowUp;
        ScriptableUnitBase firstRowDown;

        if (faction == Faction.Allies)
        {
            firstRowUp   = target[target.GetLength(0), 0];
            firstRowDown = target[target.GetLength(0), 1];
        }
        else
        {
            firstRowUp   = target[0, 0];
            firstRowDown = target[0, 1];
        }

        //if the first row only has one unit, that one will always be the target
        if (firstRowDown == null)
            return firstRowUp;
        else if (firstRowUp == null)
            return firstRowDown;
        //otherwise roll a dice to see which of the front row units will be targeted
        else
        {
            return Helpers.DiceRoll(0.5f) ? 
                firstRowUp
                :
                firstRowDown;
        }
    }

    /// <summary>
    /// Removes the unit from its grid
    /// </summary>
    public void Remove(ScriptableUnitBase unit)
    {
        var target = unit.Faction == Faction.Allies ? Allies : Enemies;

        for (int k = 0; k < target.GetLength(0); k++)
            for (int l = 0; l < target.GetLength(1); l++)
                if (target[k, l] == unit)
                    target[k, l] = null;

        Restructure(unit.Faction);
    }
}
