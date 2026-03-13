using Hammer;
using UnityEngine;

public enum GameState
{
    Menu,
    Calibration,
    BeforePlay,
    Playing,
    Paused,
    GameOver

}

public class GlobalManager : MonoBehaviour
{
    public static GlobalManager Instance { get; private set; }

    public Quaternion CalibrationQuaternion = new Quaternion(1, 1, 1, 1);
    public static HammerBehaviour HammerBehaviour { get; private set; }
    public GameState curState { get; private set; }
    public float playTimeSoFar { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (curState == GameState.Playing)
        {
            ElapsedRunTime += Time.deltaTime;
        }
    }
}
