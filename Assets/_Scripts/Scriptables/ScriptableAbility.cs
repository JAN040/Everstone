using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScriptableAbility : ScriptableObject
{
    public Sprite MenuImage;

    public string Name;

    [TextArea(3, 5)]
    public string Description;

    //0 => locked
    //when unlocked starts at level 1
    public int Level = 0;

    //TODO
    //[SerializeField] List<UnlockCondition> UnlockConditions;

    //TODO
    //public List<Effect> Effects;

    /// <summary>
    /// The amount (in seconds) it will take for the skill to become available after activation
    /// </summary>
    public float Cooldown = 0;

    private float currentCooldown = 0;
    /// <summary>
    /// Amount of time (in seconds) left till skill becomes available
    /// </summary>
    public float CurrentCooldown
    {
        get => currentCooldown;
        set
        {
            currentCooldown = value;
         
            if (currentCooldown < 0)
                currentCooldown = 0;
        }
    }
    //TODO
    //public ToggleMode ToggleMode = ToggleMode.None;



    public float GetCooldownNormalized()
    {
        return Cooldown <= 0 ? 0 : CurrentCooldown / Cooldown;
    }
}
