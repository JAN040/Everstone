using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Encapsulates damage data
///     Allows differentiation between Physical and Arts damage types,
///     also supports mixed damage
/// </summary>
public class Damage
{
    public float PhysicalDamage { get; private set; }
    public float ArtsDamage { get; private set; }
    public float TrueDamage { get; private set; }
    public DamageType Type { get; private set; }

    public Damage(float physicalDamage, float artsDamage = 0)
    {
        PhysicalDamage = physicalDamage > 0 ? physicalDamage : 0;
        ArtsDamage = artsDamage > 0 ? artsDamage : 0;

        if (PhysicalDamage > 0 && ArtsDamage > 0)
            Type = DamageType.Mixed;
        else if (ArtsDamage > 0)
            Type = DamageType.Arts;
        else
            Type = DamageType.Physical;
    }

    /// <summary>
    /// For true damage
    /// </summary>
    public Damage(float trueDamage)
    {
        TrueDamage = trueDamage > 0 ? trueDamage : 0;
        Type = DamageType.True;
    }
}


public enum DamageType
{
    Physical,
    Arts,
    Mixed,
    /// <summary>
    /// Ignores armor and resistances
    /// </summary>
    True
}