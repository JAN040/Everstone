using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class StatGrowthParameters
{
    [Header("Default stat growth parameters")]

    [Range(1f, 300f)]
    public static float DefaultAdditionMultiplier = 300;
    [Range(2f, 4f)]
    public static float DefaultPowerMultiplier = 2;
    [Range(7f, 14f)]
    public static float DefaultDivisionMultiplier = 7;


    public float AdditionMultiplier;
    public float PowerMultiplier;
    public float DivisionMultiplier;

    public StatGrowthParameters(float additionMultiplier = -1, float powerMultiplier = -1, float divisionMultiplier = -1)
    {
        if (additionMultiplier == -1)
            AdditionMultiplier = DefaultAdditionMultiplier;
        else
            AdditionMultiplier = additionMultiplier;

        if (powerMultiplier == -1)
            PowerMultiplier = DefaultPowerMultiplier;
        else
            PowerMultiplier = powerMultiplier;

        if (divisionMultiplier == -1)
            DivisionMultiplier = DefaultDivisionMultiplier;
        else
            DivisionMultiplier = divisionMultiplier;
    }
}

