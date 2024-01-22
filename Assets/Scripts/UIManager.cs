using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    public TextMeshProUGUI hpTxt,atkTxt,defTxt,resTxt;

    void Awake()
    {
        instance = this;
    }
    public void UpdateDetailUI(int hp, int atk, int def, int res){
        hpTxt.text = "HP: "  + hp;
        atkTxt.text = "ATK: "  + atk;
        defTxt.text = "DEF: "  + def;
        resTxt.text = "RES: "  + res;
    }
}
