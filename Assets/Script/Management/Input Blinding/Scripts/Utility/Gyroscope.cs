using UnityEngine;
using UnityEngine.Events;
using JahnStar.Optimization;
[AddComponentMenu("JahnStar/Utility/Gyroscope"), RequireComponent(typeof(RectTransform))]
public class Gyroscope : MonoBehaviour, IHeyUpdate
{
	public float sensitivityX = 10f;
	[Space] public InputEvent eventHandler;
	[Header("Performance"), SerializeField]
	private int updatePerFrame = 1;
    public int UpdatePerFrame => updatePerFrame;
	[System.Serializable] public class InputEvent : UnityEvent<float> { }
	private float prev_acceleration;

    public void HeyUpdate(float deltaTime)
    {
		HorizontalUpdate();
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
