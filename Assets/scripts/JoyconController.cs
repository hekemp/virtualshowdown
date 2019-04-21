using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class JoyconController : MonoBehaviour {
    private static List<Joycon> j;

    /// <summary>
    /// ButtonPressed is the bool that sets calibration.
    /// Originally it was when the player presses any button for the first time,
    /// they calibrated the system. This is used in FreePlay, but not in ExpMode
    /// </summary>
    public static bool ButtonPressed;

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

    private void Update()
    {
        // Use keyboard presses when joycon isn't available, otherwise just return
       
        if (!CheckJoyconAvail())
        {
            //DEBUG Simulate button press on Keyboard
            if (Input.GetKeyUp(KeyCode.Space))
            {
                ButtonPressed = false;
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ButtonPressed = true;
            }
            //END DEBUG

            return;
        }

        foreach (var joycon in j)
        {
            if (joycon.state == Joycon.state_.ATTACHED)
            {
                // GetButtonDown checks if a button has been released
                if (joycon.GetButtonUp(Joycon.Button.SHOULDER_2) ||
                    joycon.GetButtonUp(Joycon.Button.DPAD_UP) ||
                    joycon.GetButtonUp(Joycon.Button.DPAD_DOWN) ||
                    joycon.GetButtonUp(Joycon.Button.DPAD_RIGHT) ||
                    joycon.GetButtonUp(Joycon.Button.DPAD_LEFT) ||
                    joycon.GetButtonUp(Joycon.Button.SHOULDER_1) ||
                    joycon.GetButtonUp(Joycon.Button.PLUS) ||
                    joycon.GetButtonUp(Joycon.Button.MINUS))
                {
                    ButtonPressed = false;
                }
                // GetButtonDown checks if a button is currently down (pressed or held)
                if (joycon.GetButton(Joycon.Button.SHOULDER_2) ||
                    joycon.GetButton(Joycon.Button.DPAD_UP) ||
                    joycon.GetButton(Joycon.Button.DPAD_DOWN) ||
                    joycon.GetButton(Joycon.Button.DPAD_RIGHT) ||
                    joycon.GetButton(Joycon.Button.DPAD_LEFT) ||
                    joycon.GetButton(Joycon.Button.SHOULDER_1) ||
                    joycon.GetButton(Joycon.Button.PLUS) ||
                    joycon.GetButton(Joycon.Button.MINUS))
                {
                    ButtonPressed = true;
                }
            }
        }
    }
}
