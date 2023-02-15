using System.Collections;
using UnityEngine;
using UnityEngine.Events;
[AddComponentMenu("JahnStar/Utility/Gyroscope"), RequireComponent(typeof(RectTransform))]
public class Gyroscope : MonoBehaviour
{
	public float sensitivityX = 10f;
	[Space] public InputEvent eventHandler;
	[Header("Performance"), SerializeField]
	private float updatePerFrame = 1;
	private float _frameTimer = 0;
	public float DeltaTime() => Time.deltaTime * updatePerFrame;
	public bool FrameOptimization() // Add the { if (FrameOptimization()) return; } into the Update function.
	{
		if (updatePerFrame < 2) return false;
		_frameTimer++;
		_frameTimer %= updatePerFrame;
		return _frameTimer != 0;
	}
	[System.Serializable] public class InputEvent : UnityEvent<float> { }
	private float prev_acceleration;
    private void Update()
    {
		if (FrameOptimization()) return;
    }
    private void HorizontalUpdate()
    {
		try
		{ 
			eventHandler.Invoke(Mathf.Lerp(prev_acceleration, Input.acceleration.x, Time.deltaTime * sensitivityX));
			prev_acceleration = Input.acceleration.x;
		}
		catch { }
	}
}
