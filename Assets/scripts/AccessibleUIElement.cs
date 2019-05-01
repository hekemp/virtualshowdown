using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;// Required when using Event data.

/// <summary>
/// Class that is attached to all UI that should be accessible
/// </summary>
public class AccessibleUIElement : MonoBehaviour, IPointerEnterHandler, ISelectHandler {

	public AudioClip hoverAudioClip;

	public void OnPointerEnter(PointerEventData eventData)
	{
		AudioManager.Instance.PlayNarrationImmediate(hoverAudioClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);
	}
	public void OnSelect(BaseEventData eventData)
	{
		AudioManager.Instance.PlayNarrationImmediate(hoverAudioClip, AudioManager.Instance.locationSettings[AudioManager.AudioLocation.Default]);
	
	}
		
}
