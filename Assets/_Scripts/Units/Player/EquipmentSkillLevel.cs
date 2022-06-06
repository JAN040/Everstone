using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class EquipmentSkillLevel : SkillLevel
{
    public WeaponType Weapon_SkillType { get; private set; }

    public EquipmentSkillLevel(Skill type, CharacterStats stats, int level = 1, int experience = 0,
                                    float proficiency = 0, WeaponType weapon_SkillType = WeaponType.None) 
        : base(type, stats, level, experience, proficiency, true)
    {
        Weapon_SkillType = weapon_SkillType;

        Level = Math.Clamp(level, 1, MAX_LEVEL);
        ExpToNextLevel = GetRequiredExpToNextLevel(Level);
        Experience = Math.Clamp(experience, 0, ExpToNextLevel - 1);
    }

    public override string GetSkillName()
    {
        return $"{Weapon_SkillType} Mastery";
    }

    protected override void ModifyStatsOnLevelUp()
    {
        if(Weapon_SkillType == WeaponType.Shield)
        {
            this._statsReference.BlockChance.Grow();
        }
        this._statsReference.WeaponProficiencies[this.Weapon_SkillType].Grow(Level);
    }
}
