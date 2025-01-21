using UnityEngine;
using UnityEngine.Events;

namespace JahnStarGames.InputBindings
{
	[AddComponentMenu("Jahn Star Games/Input Manager/Utility/Gyroscope"), RequireComponent(typeof(RectTransform))]
	public class Gyroscope : MonoBehaviour
	{
		public float sensitivityX = 10f;
		[Space] public InputEvent eventHandler;
		[Header("Performance"), SerializeField]
		private int updatePerFrame = 1;
		public int UpdatePerFrame => updatePerFrame;
		[System.Serializable] public class InputEvent : UnityEvent<float> { }
		private float prev_acceleration;

		public void Update() => HorizontalUpdate();
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
}