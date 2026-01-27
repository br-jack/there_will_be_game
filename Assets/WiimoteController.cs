using UnityEngine;
using WiimoteApi;
public class WiimoteController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Wiimote remote;
    private void InitWiimotes()
    {
        WiimoteManager.FindWiimotes(); // Poll native bluetooth drivers to find Wiimotes

            remote = WiimoteManager.Wiimotes[0];
            remote.SendPlayerLED(true, false, false, true);
        
    }

    private void FinishedWithWiimotes()
    {
        foreach (Wiimote remote in WiimoteManager.Wiimotes)
        {
            WiimoteManager.Cleanup(remote);
        }
    }
    void Start()
    {
        InitWiimotes();
    }

    // Update is called once per frame
    void Update()
    {
        // remote.RumbleOn = true; // Enabled Rumble
        // remote.SendStatusInfoRequest(); // Requests Status Report, encodes Rumble into input report

        // Thread.Sleep(500); // Wait 0.5s

        // remtote.RumbleOn = false; // Disabled Rumble
        // remote.SendStatusInfoRequest(); // Requests Status Report, encodes Rumble into input report
    }
}
