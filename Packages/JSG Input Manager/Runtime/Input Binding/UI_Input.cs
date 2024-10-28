//Last Fix 22.5.2023 (Fixed: Distance method not works.)
//Last Edit: 11.11.2023
//Developed by Halil Emre Yildiz - @Jahn_Star
//https://github.com/JahnStar
//https://jahnstar.github.io
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System;

namespace JahnStarGames.InputManager
{
    [AddComponentMenu("Jahn Star Games/Input Manager/UI Input"), RequireComponent(typeof(RectTransform), typeof(UnityEngine.UI.Image))]
    public class UI_Input : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [HideInInspector]
        public InputBinding inputBinding;
        [HideInInspector]
        public int inputIndex;
        [HideInInspector]
        public string inputName;
        [HideInInspector]
        public int axis; // 0 = X, 1 = Y
        [HideInInspector]
        public int keyState; // not press = 0, press down = 1, pressing = 2, press up = 3
        [HideInInspector]
        public float scaleFactor = 1;
        [HideInInspector]
        public Vector2 sizeFactor = Vector2.one;
        [HideInInspector]
        public float axisValue = 2;
        [HideInInspector]
        public bool dynamicBg, circularBg, fixedOriginHandle, calculateDistance, simulateInput;
        [HideInInspector]
        public RectTransform bg, handle;
        [HideInInspector]
        public InputEvent keyEventHandler, axisEventHandler;
        [HideInInspector]
        public bool eventInvoke;
        private bool _init;
        private void Start()
        {
            _init = true;
            for (int i = 0; i < inputBinding.inputs.Count; i++) if (inputBinding.inputs[i].name == inputName) inputIndex = i;
            // isAxis
            if (axisValue == 2) return;
            else axisValue = 0;
            scaleFactor = bg.GetComponentInParent<Canvas>().scaleFactor;
            sizeFactor = bg.sizeDelta * 0.5f;
            // Load last value
            try
            {
                float initialValue = inputBinding.GetInput(inputIndex);

                if (axis == 0) handle.localPosition = new Vector2(initialValue, handle.localPosition.y) * sizeFactor;
                else handle.localPosition = new Vector2(handle.localPosition.x, initialValue) * sizeFactor;
                axisValue = initialValue;
            }
            catch 
            {
            #if UNITY_EDITOR
                UnityEditor.Selection.activeObject = gameObject;
            #endif
            }
            if (GetComponentsInChildren<UnityEngine.UI.Button>().Length > 1) Debug.Log("UI_Input (" + gameObject.name + ") may not work properly.");
        }
        public float axisValueDistance, _prevValue, distanceMultiplier = 100f;
        private bool _pressed;
        private float touching_velocity = 0;
        public float touching_smoothTime = 0f;
        private void Update()
        {
            // Touchpad mode (distance method)
            if (calculateDistance)
            {
                if (_pressed)
                {
                    if (touching_smoothTime > 0) axisValueDistance = Mathf.SmoothDamp(axisValueDistance, axisValue - _prevValue, ref touching_velocity, touching_smoothTime);
                    else axisValueDistance = axisValue - _prevValue;
                    _prevValue = axisValue;
                }
                else
                {
                    axisValueDistance = 0;
                }

                if (eventInvoke) axisEventHandler.Invoke(axisValueDistance * distanceMultiplier);
                else if (simulateInput) inputBinding.SimulateInput(inputIndex, 1, axisValueDistance);
            }

        }
        public void OnDrag(PointerEventData ped)
        {
            keyState = 2;
            if (eventInvoke) keyEventHandler.Invoke(keyState);
            //
            if (!calculateDistance && simulateInput) inputBinding.SimulateInput(inputIndex, 1, axisValue);
            // isAxis
            if (axisValue == 2) return;
            axisValue = GetHandleLocalAxis(ped.position);
            //
            if (!calculateDistance && eventInvoke) axisEventHandler.Invoke(axisValue);
        }
        public float GetHandleLocalAxis(Vector2 pedPosition)
        {
            float _axisValue;
            Vector2 pedPos;
            Vector2 result;
            if (circularBg) pedPos = pedPosition;
            else
            { 
                pedPos = handle.position;
                if (axis == 0) pedPos.x = pedPosition.x; else pedPos.y = pedPosition.y;
            }
            Vector2 distance_normal = (pedPos - (Vector2)bg.position) / scaleFactor / sizeFactor;
            if (circularBg) result = Vector2.ClampMagnitude(distance_normal, 1f);
            else result = new Vector2(Mathf.Clamp(distance_normal.x, -1, 1), Mathf.Clamp(distance_normal.y, -1, 1));
            handle.localPosition = result * sizeFactor;
            // Set input value
            if (axis == 0) _axisValue = result.x; else _axisValue = result.y;
            return _axisValue > 0.99f ? 1f : _axisValue < -0.99f ? -1 : _axisValue;
        }
        public void OnPointerDown(PointerEventData ped)
        {
            keyState = 1;
            if (eventInvoke) keyEventHandler.Invoke(keyState);
            //
            if (simulateInput) inputBinding.SimulateInput(inputIndex, 1, axisValue);
            // isAxis
            if (axisValue == 2) return;
            if (dynamicBg) // the first axisValue is always zero (dynamic joystick) - fixed origin
            {
                Vector2 target = (ped.position - (Vector2)bg.transform.parent.position) / scaleFactor;
                bg.localPosition = target;
                bg.gameObject.SetActive(true);
                // Fix
                if (axis == 0) handle.localPosition = new Vector2(0, handle.localPosition.y) * sizeFactor;
                else handle.localPosition = new Vector2(handle.localPosition.x, 0) * sizeFactor;
                axisValue = 0;
            }
            else // the first axisValue can be a non-zero value (static stick) - not fixed origin
            {
                axisValue = GetHandleLocalAxis(ped.position);
                if (simulateInput) inputBinding.SimulateInput(inputIndex, 1, axisValue);
            }
            //
            if (eventInvoke) axisEventHandler.Invoke(axisValue);
            //
            _pressed = true;
            _prevValue = axisValue;
        }
        /// <summary>
        /// IF ISN'T WORKING: if the camera changes while the key is released, this event is not called. 
        /// In this case, delay the camera switching method. 
        /// Make sure it doesn't conflict with the camera switching method.
        /// IF ISN'T WORKING: if you use button component in child object, this event is not called. 
        /// </summary>
        public void OnPointerUp(PointerEventData ped)
        {
            keyState = 3;
            if (eventInvoke) keyEventHandler.Invoke(keyState);
            keyState = 0;
            if (eventInvoke) keyEventHandler.Invoke(keyState);
            //
            if (simulateInput) inputBinding.SimulateInput(inputIndex, 2);
            // isAxis
            if (axisValue == 2) return;
            if (fixedOriginHandle)
            {
                if (axis == 0) handle.localPosition = new Vector2(0, handle.localPosition.y);
                else handle.localPosition = new Vector2(handle.localPosition.x, 0);
                axisValue = 0;
            }
            if (dynamicBg) bg.gameObject.SetActive(false);
            //
            if (eventInvoke) axisEventHandler.Invoke(axisValue);
            _pressed = false;
        }
        private void OnDisable() { if (_init) OnPointerUp(null); }
    }
    [Serializable] public class InputEvent : UnityEvent<float> { }
}