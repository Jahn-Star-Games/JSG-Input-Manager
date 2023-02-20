// SteeringWheel.cs original version: yasirkula/UnitySimpleInput/SteeringWheel.cs
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using JahnStar.Optimization;

[AddComponentMenu("JahnStar/Utility/Steering Wheel"), RequireComponent(typeof(RectTransform), typeof(UnityEngine.UI.Image))]
public class SteeringWheel : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IHeyUpdate
{
	private RectTransform wheelTransform;
	private Vector2 centerPoint;
	//
	public float maximumSteeringAngle = 270f;
	public float wheelReleasedSpeed = 800f;
	public float valueMultiplier = 1f;
	//
	private float wheelAngle = 0f;
	private float wheelPrevAngle = 0f;
	//
	private bool wheelBeingHeld = false;
	//
	[SerializeField] private float m_value;
	[Space] public InputEvent eventHandler;
	[Header("Performance"), SerializeField]
	private int updatePerFrame = 1;
    public int UpdatePerFrame => updatePerFrame;
	[System.Serializable] public class InputEvent : UnityEvent<float> { }
	public float Value { get { return m_value; } }
	public float Angle { get { return wheelAngle; } }


    private void Awake() => wheelTransform = GetComponent<RectTransform>();
	private void OnDisable()
	{
		wheelBeingHeld = false;
		wheelAngle = wheelPrevAngle = m_value = 0f;
		wheelTransform.localEulerAngles = Vector3.zero;
	}
	public void HeyUpdate(float deltaTime)
	{
		// If the wheel is released, reset the rotation
		// to initial (zero) rotation by wheelReleasedSpeed degrees per second
		if (!wheelBeingHeld && wheelAngle != 0f)
		{
			float deltaAngle = wheelReleasedSpeed * deltaTime;
			if (Mathf.Abs(deltaAngle) > Mathf.Abs(wheelAngle)) wheelAngle = 0f;
			else if (wheelAngle > 0f) wheelAngle -= deltaAngle;
			else wheelAngle += deltaAngle;
		}
		// Rotate the wheel image
		wheelTransform.localEulerAngles = new Vector3(0f, 0f, -wheelAngle);
		m_value = wheelAngle * valueMultiplier / maximumSteeringAngle;
		//
        try { eventHandler.Invoke(m_value); }
        catch { }
	}
	public void OnPointerDown(PointerEventData eventData)
	{
		// Executed when mouse/finger starts touching the steering wheel
		wheelBeingHeld = true;
		centerPoint = RectTransformUtility.WorldToScreenPoint(eventData.pressEventCamera, wheelTransform.position);
		wheelPrevAngle = Vector2.Angle(Vector2.up, eventData.position - centerPoint);
	}
	public void OnDrag(PointerEventData eventData)
	{
		// Executed when mouse/finger is dragged over the steering wheel
		Vector2 pointerPos = eventData.position;
		float wheelNewAngle = Vector2.Angle(Vector2.up, pointerPos - centerPoint);
		// Do nothing if the pointer is too close to the center of the wheel
		if ((pointerPos - centerPoint).sqrMagnitude >= 400f)
		{
			if (pointerPos.x > centerPoint.x) wheelAngle += wheelNewAngle - wheelPrevAngle;
			else wheelAngle -= wheelNewAngle - wheelPrevAngle;
		}
		// Make sure wheel angle never exceeds maximumSteeringAngle
		wheelAngle = Mathf.Clamp(wheelAngle, -maximumSteeringAngle, maximumSteeringAngle);
		wheelPrevAngle = wheelNewAngle;
	}
	public void OnPointerUp(PointerEventData eventData)
	{
		// Executed when mouse/finger stops touching the steering wheel
		// Performs one last OnDrag calculation, just in case
		OnDrag(eventData);
		wheelBeingHeld = false;
	}
}