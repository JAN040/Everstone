using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class PlayerUnit : UnitBase
{
    public LevelSystem LevelSystem { get; set; }
    
    public PlayerUnit(LevelSystem levelSystem, CharacterStats stats) : base(stats)
    {
        LevelSystem = levelSystem;
    }


}
