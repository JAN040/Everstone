using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// A helper class for grouping weapon bonus stats by type
/// </summary>
public class WeaponProficiency
{
    private readonly float ACCURACY_GROWTH = 0.01f;
    private readonly float DAMAGE_GROWTH = 1f;



    public WeaponType WeaponType { get; private set; }
    public Stat DamageBonus { get; private set; }
    public Stat AccuracyBonus { get; private set; }
   
    public WeaponProficiency(WeaponType weaponType, float damageBonus, float accuracyBonus)
    {
        WeaponType = weaponType;
        DamageBonus = new Stat(damageBonus, StatType.WeaponProficiency, false);
        AccuracyBonus = new Stat(accuracyBonus, StatType.WeaponProficiency, false); ;
    }

    public void Grow(int level)
    {
        DamageBonus.SetBaseValue(DamageBonus.GetBaseValue() + DAMAGE_GROWTH);
        AccuracyBonus.SetBaseValue(AccuracyBonus.GetBaseValue() + ACCURACY_GROWTH);
    }
}
