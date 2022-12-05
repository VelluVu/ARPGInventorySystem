using UnityEngine;

[System.Serializable]
public class Stat
{
    public string name;
    public string description;
    public StatType type;
    public StatValue value;
}

public enum StatType
{
    Armor,
    Health,
}