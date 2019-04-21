using UnityEngine;
using UnityEngine.UI;

public class AIColliderScript : MonoBehaviour
{
    internal static bool ballInZone;

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Ball")
        {
            ballInZone = false;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Ball")
        {
            ballInZone = true;
        }
    }
}
