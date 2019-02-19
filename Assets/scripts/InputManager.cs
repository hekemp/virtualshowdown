using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InputManager : MonoBehaviour
{
	public static InputManager Instance;
	
	public abstract Vector3 GetHandPosition();
}
