using UnityEngine;

public class HorseRamController : MonoBehaviour
{
    public bool buildingRamUnlocked;

    public void UnlockBuildingRam()
    {
        buildingRamUnlocked = true;
    }
}