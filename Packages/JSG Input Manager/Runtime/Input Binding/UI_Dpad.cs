using System;
using UnityEngine;
using UnityEngine.Events;

namespace JahnStarGames.InputManager
{
    [AddComponentMenu("Jahn Star Games/Input Manager/UI Dpad")]
    public class UI_Dpad : MonoBehaviour
    {
        [Header("UI Inputs")]
        [SerializeField] private UI_Input xAxis;
        [SerializeField] private UI_Input yAxis;
        [Header("Snapping")]
        [SerializeField] private RectTransform snapHandle;
        [SerializeField] private float snapingNormal = 1f;
        [Header("Conditions"), Range(0f, 1f)]
        [SerializeField] private float thresholdValue = 0.1f;
        [SerializeField] private KeyState keyState;
        [Header("Actions")]
        [SerializeField] private PadEvents padEvents;
        private float _xAxis, _yAxis;
        public enum KeyState { PRESS_DOWN = 1, PRESSING = 2, PRESS_UP = 3 }
        private readonly Vector2[] _snapPositions = new Vector2[] { new(0, 0), new(0, 1), new(0, -1), new(1, 0), new(-1, 0), new(1, 1), new(-1, 1), new(1, -1), new(-1, -1) };
        internal Action resetHandle;
        private void Start() => Invoke(nameof(Init), 0.01f);
        public void Init()
        {
            xAxis.axisEventHandler.AddListener((axisValue) => _xAxis = axisValue);
            yAxis.axisEventHandler.AddListener((axisValue) => _yAxis = axisValue);
            xAxis.keyEventHandler.AddListener((keyState) => InputHandle((int)keyState));
            //
            if (snapHandle)
            {
                UI_Input dominantAxis = xAxis;
                for (int i = 0; i < _snapPositions.Length; i++)
                {
                    Vector2 GetHandlePositionFromAxis(Vector2 axisNormal) => dominantAxis.scaleFactor * snapingNormal * (dominantAxis.circularBg ? Vector2.ClampMagnitude(axisNormal, 1f) : axisNormal) * dominantAxis.sizeFactor;
                    Vector2 targetHandlePos = GetHandlePositionFromAxis(_snapPositions[i]);
                    switch (i)
                    {
                        case 1: padEvents.up.AddListener(() => snapHandle.position = (Vector2)dominantAxis.bg.position + targetHandlePos); break;
                        case 2: padEvents.down.AddListener(() => snapHandle.position = (Vector2)dominantAxis.bg.position + targetHandlePos); break;
                        case 3: padEvents.right.AddListener(() => snapHandle.position = (Vector2)dominantAxis.bg.position + targetHandlePos); break;
                        case 4: padEvents.left.AddListener(() => snapHandle.position = (Vector2)dominantAxis.bg.position + targetHandlePos); break;
                        case 5: padEvents.up_right.AddListener(() => snapHandle.position = (Vector2)dominantAxis.bg.position + targetHandlePos); break;
                        case 6: padEvents.up_left.AddListener(() => snapHandle.position = (Vector2)dominantAxis.bg.position + targetHandlePos); break;
                        case 7: padEvents.down_right.AddListener(() => snapHandle.position = (Vector2)dominantAxis.bg.position + targetHandlePos); break;
                        case 8: padEvents.down_left.AddListener(() => snapHandle.position = (Vector2)dominantAxis.bg.position + targetHandlePos); break;
                        default: padEvents.center.AddListener(() => snapHandle.position = (Vector2)dominantAxis.bg.position); break;
                    }
                }
                if (padEvents.centerEventResets) resetHandle = () => padEvents.center?.Invoke();
                snapHandle.gameObject.SetActive(true);
            }
        }
        public void InputHandle(int state)
        {
            if (keyState == KeyState.PRESSING) resetHandle?.Invoke();
            if ((int)keyState != state) return;
            padEvents.InputHandle(_xAxis, _yAxis, thresholdValue);
        }
        private void Awake()
        {
            padEvents.Init(transform);
            if (snapHandle) snapHandle.gameObject.SetActive(false);
        }
        private void OnEnable() => resetHandle?.Invoke();
        private void OnDisable() => resetHandle?.Invoke();
        public void SetKeyStateCondition(int index) => keyState = (KeyState)index;
        #region Fake Press
        public void InvokeDPadCenter() => padEvents.center?.Invoke();
        public void InvokeDPadUp() => padEvents.up?.Invoke();
        public void InvokeDPadDown() => padEvents.down?.Invoke();
        public void InvokeDPadRight() => padEvents.right?.Invoke();
        public void InvokeDPadLeft() => padEvents.left?.Invoke();
        #endregion
        [Serializable]
        public class PadEvents
        {
            public bool centerEventResets = true;
            public UnityEvent center;
            [Space]
            public GameObject upImage;
            public UnityEvent up;
            public GameObject downImage;
            public UnityEvent down;
            public GameObject rightImage;
            public UnityEvent right;
            public GameObject leftImage;
            public UnityEvent left;
            [Space]
            public GameObject upRightImage;
            public UnityEvent up_right;
            public GameObject upLeftImage;
            public UnityEvent up_left;
            public GameObject downRightImage;
            public UnityEvent down_right;
            public GameObject downLeftImage;
            public UnityEvent down_left;

            internal void Init(Transform parent)
            {
                GameObject tempObject = new GameObject("_temp_dpad_image");
                tempObject.transform.SetParent(parent);
                tempObject.SetActive(false);
                if (!upImage) upImage = tempObject;
                if (!downImage) downImage = tempObject;
                if (!rightImage) rightImage = tempObject;
                if (!leftImage) leftImage = tempObject;
                if (!upRightImage) upRightImage = tempObject;
                if (!upLeftImage) upLeftImage = tempObject;
                if (!downRightImage) downRightImage = tempObject;
                if (!downLeftImage) downLeftImage = tempObject;
                //
            }
            internal void InputHandle(float _xAxis, float _yAxis, float thresholdValue)
            {
                switch (_yAxis >= thresholdValue)
                {
                    case true when upRightImage.activeInHierarchy && _xAxis >= thresholdValue: up_right?.Invoke(); break;
                    case true when upLeftImage.activeInHierarchy && _xAxis <= -thresholdValue: up_left?.Invoke(); break;
                    case true when upImage.activeInHierarchy: up.Invoke(); break;
                    case false when downRightImage.activeInHierarchy && _yAxis <= -thresholdValue && _xAxis >= thresholdValue: down_right?.Invoke(); break;
                    case false when downLeftImage.activeInHierarchy && _yAxis <= -thresholdValue && _xAxis <= -thresholdValue: down_left?.Invoke(); break;
                    case false when downImage.activeInHierarchy && _yAxis <= -thresholdValue: down?.Invoke(); break;
                    case false when rightImage.activeInHierarchy && _xAxis >= thresholdValue: right?.Invoke(); break;
                    case false when leftImage.activeInHierarchy && _xAxis <= -thresholdValue: left?.Invoke(); break;
                    default: center?.Invoke(); break;
                }
            }
        }
    }
}