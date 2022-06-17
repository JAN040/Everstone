using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class UnitData
{
    public static CharacterStats GetBaseStats(EnemyClass enemyClass, EnemyType enemyType, LocationDifficulty locDiff, Difficulty gameDiff)
    {
        return new CharacterStats(1, 1, 0, 0, 
                                  50, 5, 1, 2, 0);
        //TODO: class specific stats, these are test only
    }

    private static CharacterStats GetEnemyStatsForClass_Marksman(float statModifierPercentage)
    {
        return null;
    }



}
