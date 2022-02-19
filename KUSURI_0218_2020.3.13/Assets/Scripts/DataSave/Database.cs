[System.Serializable]
public class Database
{
    //position
    public float x;
    public float y;
    public float z;
    
    //LevelObjectsReload
    public string scenePlayed;
}

[System.Serializable]
public class BackPackItem
{
    //material
    public float lightHerb;
    public float timeHerb;
    public float scaleHerb;
    public float fruit;
    public float bigMine;
    public float smallMine;

    //potion
    public float o_lightBig;
    public float o_lightSmall;
    public float o_timeBig;
    public float o_timeSmall;
    public float o_scaleBig;
    public float o_scaleSmall;

    public float p_lightBig;
    public float p_lightSmall;
    public float p_timeBig;
    public float p_timeSmall;
    public float p_scaleBig;
    public float p_scaleSmall;
}

[System.Serializable]
public class BloodCellDatabase
{//White Cell Blood Wall Destoryed Save
    public string[] bloodObjectsName;
    public bool[] bloodObjectsActive;
}