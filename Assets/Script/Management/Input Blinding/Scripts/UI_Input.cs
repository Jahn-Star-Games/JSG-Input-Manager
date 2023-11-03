//Last Fix 22.5.2023 (Fixed: Distance method is not working issue.)
//Last Edit: 27.5.2023 (Added: Smooth Touching)
//Developed by Halil Emre Yildiz - @Jahn_Star
//https://github.com/JahnStar
//https://jahnstar.github.io
using UnityEngine;
using UnityEditor;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace JahnStar.CoreThreeD
{
    [AddComponentMenu("JahnStar/Core/UI Input"), RequireComponent(typeof(RectTransform), typeof(UnityEngine.UI.Image))]
    public class UI_Input : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [HideInInspector]
        public int inputManagerIndex;
        [HideInInspector]
        public HeyInputBinding inputManager;
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
            for (int i = 0; i < inputManager.inputs.Count; i++) if (inputManager.inputs[i].name == inputName) inputIndex = i;
            // isAxis
            if (axisValue == 2) return;
            else axisValue = 0;
            scaleFactor = bg.GetComponentInParent<Canvas>().scaleFactor;
            sizeFactor = bg.sizeDelta * 0.5f;
            // Load last value
            try
            {
                float initialValue = inputManager.GetInput(inputIndex);

                if (axis == 0) handle.localPosition = new Vector2(initialValue, handle.localPosition.y) * sizeFactor;
                else handle.localPosition = new Vector2(handle.localPosition.x, initialValue) * sizeFactor;
                axisValue = initialValue;
            }
            catch 
            {
            #if UNITY_EDITOR
                Selection.activeObject = gameObject;
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
                else if (simulateInput) inputManager.SimulateInput(inputIndex, 1, axisValueDistance);
            }

        }
        public void OnDrag(PointerEventData ped)
        {
            keyState = 2;
            if (eventInvoke) keyEventHandler.Invoke(keyState);
            //
            if (!calculateDistance && simulateInput) inputManager.SimulateInput(inputIndex, 1, axisValue);
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
            if (simulateInput) inputManager.SimulateInput(inputIndex, 1, axisValue);
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
                if (simulateInput) inputManager.SimulateInput(inputIndex, 1, axisValue);
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
            if (simulateInput) inputManager.SimulateInput(inputIndex, 2);
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
    [System.Serializable] public class InputEvent : UnityEvent<float> { }
#if UNITY_EDITOR
    [CustomEditor(typeof(UI_Input))]
    public class UI_Input_Editor : UnityEditor.Editor
    {
        private UI_Input _target;
        private GameManager gameManager;
        private SerializedProperty keyEventHandler, axisEventHandler;
        private void Awake()
        {
            try 
            { 
                _target = (UI_Input)target;
                gameManager = GameManager.Instance;
            }
            catch { }
            keyEventHandler = serializedObject.FindProperty("keyEventHandler");
            axisEventHandler = serializedObject.FindProperty("axisEventHandler");
        }
        public override void OnInspectorGUI()
        {
            GUIStyle title = GUIStyle.none;
            title.alignment = TextAnchor.MiddleCenter;
            title.fontStyle = FontStyle.Bold;
            title.normal.textColor = Color.white;
            title.fontSize = 14;
            GUI.contentColor = new Color(0, 0.9f, 1, 1);
            GUI.backgroundColor = Color.black * 0.4f;
            Color defaultColor = GUI.backgroundColor;
            GUILayout.Space(5);
            GUILayout.Box("Developed by Halil Emre Yildiz \n ! Canvas Mode set to Overlay ! ", GUILayout.MinWidth(Screen.width - 37));
            GUILayout.Space(5);
            GUILayout.Label("Simulate Input", title);
            //
            GUILayout.Space(5);
            if (!gameManager)
            {
                GUI.backgroundColor = Color.red;
                GUILayout.Box("Create a Game Manager in Scene", EditorStyles.textField);
                GUI.backgroundColor = defaultColor;
                GUILayout.Box("if you have a Game Manager, try reset Unity", EditorStyles.textField);
                return;
            }
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Bindings", GUILayout.MinWidth(1));
            string[] bindings = new string[gameManager.inputBindings.Count];
            for (int i = 0; i < bindings.Length; i++) bindings[i] = gameManager.inputBindings[i].name;
            if (bindings.Length == 0)
            {
                GUI.backgroundColor = Color.red;
                GUILayout.Box("Add a Input Binding on Game Manager", EditorStyles.textField);
                GUI.backgroundColor = defaultColor;
            }
            else
            {
                _target.inputManagerIndex = EditorGUILayout.Popup(_target.inputManagerIndex, bindings);
                _target.inputManager = gameManager.inputBindings[_target.inputManagerIndex];
                GUI.backgroundColor = defaultColor;
            }
            EditorGUILayout.EndHorizontal();
            //
            try
            {
                if (_target.inputManager)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Input Name ", GUILayout.MinWidth(1));

                    string[] inputs = new string[_target.inputManager.inputs.Count];
                    for (int i = 0; i < inputs.Length; i++)
                    {
                        inputs[i] = _target.inputManager.inputs[i].name;
                        if (_target.inputManager.inputs[i].name == _target.inputName) _target.inputIndex = i;
                    }
                    _target.inputIndex = EditorGUILayout.Popup(_target.inputIndex, inputs);
                    _target.inputName = _target.inputManager.inputs[_target.inputIndex].name;
                    EditorGUILayout.EndHorizontal();

                    // Axis stick
                    if (_target.inputManager.inputs[_target.inputIndex].type == InputData.Type.Axes)
                    {
                        title.alignment = TextAnchor.MiddleLeft;
                        GUIStyle header = GUIStyle.none;
                        header.alignment = TextAnchor.MiddleCenter;
                        header.fontStyle = FontStyle.Bold;
                        header.normal.textColor = Color.white;
                        header.fontSize = 12;

                        GUI.contentColor = new Color(0, 0.9f, 1, 1);
                        GUI.backgroundColor = Color.black * 0.4f;

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Axis", GUILayout.MinWidth(1));
                        _target.axis = EditorGUILayout.Popup(_target.axis, new string[] { "X", "Y" });
                        EditorGUILayout.EndHorizontal();
                        GUILayout.Space(5);
                        GUI.backgroundColor = Color.white * 0.8f;

                        defaultColor = GUI.backgroundColor;
                        if (!_target.bg || _target.bg == _target.handle) GUI.backgroundColor = Color.red;
                        else GUI.backgroundColor = Color.white;

                        GUILayout.BeginVertical("GroupBox");
                        GUILayout.Label("UI Triggers", header);
                        GUILayout.Space(5);
                        _target.bg = (RectTransform)EditorGUILayout.ObjectField(_target.bg, typeof(RectTransform), true);
                        GUILayout.BeginHorizontal();
                        if (!_target.handle) GUI.backgroundColor = Color.red;
                        else GUI.backgroundColor = defaultColor;
                        GUILayout.BeginVertical("GroupBox");
                        _target.handle = (RectTransform)EditorGUILayout.ObjectField(_target.handle, typeof(RectTransform), true);
                        GUILayout.EndVertical();
                        GUILayout.EndHorizontal();
                        try
                        {
                            int axisInputCount = 0;
                            foreach (UI_Input input in _target.GetComponents<UI_Input>()) if (input.axisValue != 2) axisInputCount++;
                            if (axisInputCount > 1)
                            {
                                if (_target.bg.gameObject != _target.gameObject)
                                {
                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.LabelField("Dynamic");
                                    GUI.backgroundColor = Color.white * 0.8f;
                                    _target.dynamicBg = EditorGUILayout.Toggle(_target.dynamicBg);
                                    EditorGUILayout.EndHorizontal();
                                }
                                else _target.dynamicBg = false;

                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField("Circular");
                                GUI.backgroundColor = Color.white * 0.8f;
                                _target.circularBg = EditorGUILayout.Toggle(_target.circularBg);
                                EditorGUILayout.EndHorizontal();
                            }
                            else
                            {
                                _target.circularBg = false;
                                _target.dynamicBg = false;
                            }
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField("Fixed Origin");
                            GUI.backgroundColor = Color.white * 0.8f;
                            _target.fixedOriginHandle = EditorGUILayout.Toggle(_target.fixedOriginHandle);
                            EditorGUILayout.EndHorizontal();

                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField("Touching (distance and smoothing)");
                            _target.calculateDistance = EditorGUILayout.Toggle(_target.calculateDistance);
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.BeginHorizontal();
                            if (_target.calculateDistance)
                            {
                                _target.distanceMultiplier = EditorGUILayout.FloatField((_target.distanceMultiplier == 0) ? 100f : _target.distanceMultiplier);
                                _target.touching_smoothTime = EditorGUILayout.FloatField(_target.touching_smoothTime);
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                        catch { }
                        GUILayout.EndVertical();
                        GUILayout.Space(5);
                        GUI.backgroundColor = defaultColor;

                        EditorGUI.BeginDisabledGroup(true);
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Keep Value ", GUILayout.MinWidth(1));
                        EditorGUILayout.TextField("input > gravity => 0");
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Axis Value ", GUILayout.MinWidth(1));
                        EditorGUILayout.Slider(_target.axisValue, -1, 1);
                        EditorGUILayout.EndHorizontal();

                        if (_target.calculateDistance)
                        {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField("Distance Value", GUILayout.MinWidth(1));
                            EditorGUILayout.Slider(_target.axisValueDistance, -1, 1);
                            EditorGUILayout.EndHorizontal();
                        }

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Key State ", GUILayout.MinWidth(1));
                        EditorGUILayout.TextField(_target.keyState + "");
                        EditorGUILayout.EndHorizontal();
                        EditorGUI.EndDisabledGroup();
                    }
                    else
                    {
                        _target.axisValue = 2;
                        GUI.backgroundColor = Color.white * 0.8f;

                        EditorGUI.BeginDisabledGroup(true);
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Key State ", GUILayout.MinWidth(1));
                        EditorGUILayout.TextField(_target.keyState + "");
                        EditorGUILayout.EndHorizontal();
                        EditorGUI.EndDisabledGroup();
                    }
                    EditorGUILayout.Space();
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Simulate Input Binding");
                    _target.simulateInput = EditorGUILayout.Toggle(_target.simulateInput);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    _target.eventInvoke = EditorGUILayout.Toggle("Event Invoke", _target.eventInvoke);
                    EditorGUILayout.EndHorizontal();

                    if (_target.eventInvoke)
                    {
                        GUI.backgroundColor = Color.black * 0.4f;
                        serializedObject.Update();

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Label("Key State Event");
                        EditorGUILayout.PropertyField(keyEventHandler, GUIContent.none, GUILayout.Height(EditorGUI.GetPropertyHeight(keyEventHandler)));
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Label("Axis Value Event");
                        EditorGUILayout.PropertyField(axisEventHandler, GUIContent.none, GUILayout.Height(EditorGUI.GetPropertyHeight(axisEventHandler)));
                        EditorGUILayout.EndHorizontal();

                        serializedObject.ApplyModifiedProperties();
                    }

                    EditorUtility.SetDirty(target);
                }
            }
            catch { Selection.activeObject = _target; }
            //base.DrawDefaultInspector(); // Debug
        }
    }
#endif
}