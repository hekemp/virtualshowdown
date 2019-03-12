using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class JoyconController : MonoBehaviour {
    private static List<Joycon> j;

    private void Start()
    {
        // get the public Joycon object attached to the JoyconManager in scene
        if (JoyconManager.Instance != null)
        {
            j = JoyconManager.Instance.j;
        }
    }

    public static void RumbleJoycon(float lowFreq, float higFreq, float amp, int time = 0)
    {
        if (!CheckJoyconAvail() || !BodySourceManager.Instance.bodyFound) return;
        
        foreach (var joycon in j)
        {
            if (joycon.state == Joycon.state_.ATTACHED)
            {
                joycon.SetRumble(lowFreq, higFreq, amp, time);
            }
        }
    }

    public static bool CheckJoyconAvail()
    {
        // make sure the Joycon only gets checked if at least 1 is attached
        return j != null && j.Exists(joycon => joycon.state == Joycon.state_.ATTACHED);
    }
}
