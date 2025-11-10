using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    // Player stats
    public int xp { get; private set; }
    public int Gold { get; private set; }
    public int Level { get; private set; }
    public int SkillPoints { get; private set; }

    // Leveling system
    public int xpToNextLevel = 100;
    [SerializeField] UIManager uiManager;

    void Start()
    {
        xp = 0;
        Gold = 100;
        Level = 1;
        SkillPoints = 0;
    }

    public void AddScore(int amount)
    {
        xp += amount;
        if (xp >= xpToNextLevel)
        {
            LevelUp();
        }
    }

    public void AddGold(int amount)
    {
        Gold += amount;
        uiManager.PulseGoldText();
    }

    public bool SpendGold(int amount)
    {
        if (Gold >= amount)
        {
            Gold -= amount;
            return true;
        }
        return false;
    }

    // Level up logic
    public void LevelUp()
    {
        Level++;
        SkillPoints++;
        xp -= xpToNextLevel;
        // 100 xp to get to level 2, 200 xp to get to level 3, etc.
        xpToNextLevel += 100 * Level;
    }

    public bool SpendSkillPoint()
    {
        if (SkillPoints > 0)
        {
            SkillPoints--;
            return true;
        }
        return false;
    }
}
