using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Encapsulates damage data
///     Allows differentiation between Physical and Arts damage types,
///     also supports mixed damage
/// </summary>
public class Damage
{
    public float Amount { get; set; }
    //public float PhysicalDamage { get; private set; }
    //public float ArtsDamage { get; private set; }
    //public float TrueDamage { get; private set; }
    //public float ElementalDamage { get; private set; }
    public DamageType Type { get; private set; }
    public ElementType ElementType { get; private set; } = ElementType.None;

    public Damage(float physicalDamage, float artsDamage = 0)
    {
        if (artsDamage > 0)
        {
            Type = DamageType.Arts;
            Amount = artsDamage > 0 ? artsDamage : 0;
        }
        else
        {
            Type = DamageType.Physical;
            Amount = physicalDamage > 0 ? physicalDamage : 0;
        }
    }

    /// <summary>
    /// For true damage
    /// </summary>
    public Damage(float amount, DamageType type)
    {
        Amount = amount > 0 ? amount : 0;
        Type = type;
    }

    /// <summary>
    /// For Elemental Damage
    /// </summary>
    public Damage(float elementalDamage, ElementType elementType)
    {
        Amount = elementalDamage > 0 ? elementalDamage : 0;
        Type = DamageType.Elemental;
        ElementType = elementType;
    }

    /// <summary>
    /// Returns a color matching the damageType
    /// </summary>
    /// <returns></returns>
    public Color GetIndicatorColor()
    {
        switch (Type)
        {
            case DamageType.Physical:
                return Color.red;

            case DamageType.Arts:
                return Color.blue;

            case DamageType.True:
                return Color.white;

            case DamageType.Elemental:
                switch (ElementType)
                {
                    case ElementType.None:
                        return Color.white;

                    case ElementType.Poison:
                        return new Color(114, 140, 0); //venom green

                    case ElementType.Fire:
                        return new Color(255, 119, 0); //fire orange

                    default:
                        return Color.white;
                }

            default:
                Debug.LogWarning($"Unexpected damageType: '{Type}'");
                return Color.white;
        }
    }
}


