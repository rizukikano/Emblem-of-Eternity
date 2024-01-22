[System.Serializable]
public class Item
{
    public string itemName;
    public int bonusHP;
    public int bonusATK;
    public int bonusDEF;
    public int bonusRES;

    public Item(string name,int hp, int atk, int def, int res)
    {
        itemName = name;
        bonusHP = hp;
        bonusATK = atk;
        bonusDEF = def;
        bonusRES = res;
    }
}
