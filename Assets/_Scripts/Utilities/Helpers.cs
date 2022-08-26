using UnityEngine;

/// <summary>
/// A static class for general helpful methods
/// </summary>
public static class Helpers 
{
    /// <summary>
    /// Destroy all child objects of this transform (Unintentionally evil sounding).
    /// Use it like so:
    /// <code>
    /// transform.DestroyChildren();
    /// </code>
    /// </summary>
    public static void DestroyChildren(this Transform t) {
        foreach (Transform child in t) Object.Destroy(child.gameObject);
    }

    /// <summary>
    /// Returns a random float between 0 and 1, representing a percentage
    /// </summary>
    public static float RandomPercent()
    {
        return Random.Range(0f, 1f);
    }

    /// <summary>
    /// <para>
    /// Rolls a random float between 0 and 1. If the rolled amount is below or equal to
    ///     provided param chanceToSucceed, returns true, otherwise false.
    /// </para>
    /// 
    /// <para>
    /// If chanceToSucceed is below or equal to 0 always returns false
    /// </para>
    /// 
    /// <para>
    /// If chanceToSucceed is above 1 always returns true
    /// </para>
    /// </summary>
    public static bool DiceRoll(float chanceToSucceed)
    {
        if (chanceToSucceed <= 0)
            return false;
        
        if (chanceToSucceed > 1)
            return true;

        return RandomPercent() <= chanceToSucceed;
    }
}
