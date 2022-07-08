using System.Collections;
using UnityEngine;
using UnityEngine.Events;
[AddComponentMenu("JahnStar/Utility/Gyroscope"), RequireComponent(typeof(RectTransform))]
public class Gyroscope : MonoBehaviour
{
	public float sensitivityX = 10f;
	[Space] public InputEvent eventHandler;
    [Header("Performance"), SerializeField] private float framePerSecond = 30;
    [System.Serializable] public class InputEvent : UnityEvent<float> { }
    private IEnumerator routine;
    private WaitForSeconds wait;
	private void OnEnable()
	{
		if (routine == null)
		{
			routine = HorizontalUpdate();
			wait = new WaitForSeconds(1f / framePerSecond);
		}
		StartCoroutine(routine);
	}
	private void OnDisable() => StopCoroutine(routine);
	private float prev_acceleration;
	private IEnumerator HorizontalUpdate()
    {
		while (true)
		{
			try
			{ 
				eventHandler.Invoke(Mathf.Lerp(prev_acceleration, Input.acceleration.x, Time.deltaTime * sensitivityX));
				prev_acceleration = Input.acceleration.x;
			}
			catch { }
			yield return wait;
		}
	}
}
