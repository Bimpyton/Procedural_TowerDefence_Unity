using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    // Player stats
    public int Score { get; private set; }
    public int Gold { get; private set; }
    public int Level { get; private set; }
    public int SkillPoints { get; private set; }

    // Leveling system (customize as needed)
    public int scoreToNextLevel = 100;

    void Start()
    {
        Score = 0;
        Gold = 0;
        Level = 1;
        SkillPoints = 0;
    }

    public void AddScore(int amount)
    {
        Score += amount;
        if (Score >= scoreToNextLevel)
        {
            LevelUp();
        }
    }

    public void AddGold(int amount)
    {
        Gold += amount;
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
        Score -= scoreToNextLevel;
        // 100 score to get to level 2, 200 score to get to level 3, etc.
        scoreToNextLevel += 100 * Level;
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
