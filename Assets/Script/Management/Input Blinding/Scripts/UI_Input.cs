//Last Edit: 03.07.2022
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
        public InputBinding inputManager;
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
        public bool dynamicBg, circularBg;
        [HideInInspector]
        public RectTransform bg, handle;
        [HideInInspector]
        public InputEvent keyEventHandle, axisEventHandle;
        [HideInInspector]
        public bool eventInvoke;
        [Space]
        private bool initialValueExist; 
        private void Start()
        {
            for (int i = 0; i < inputManager.inputs.Count; i++) if (inputManager.inputs[i].name == inputName) inputIndex = i;
            // isAxis
            if (axisValue == 2) return;
            else axisValue = 0;
            scaleFactor = bg.GetComponentInParent<Canvas>().scaleFactor;
            sizeFactor = bg.sizeDelta * 0.5f;
            // Load last value
            try
            {
                float initialValue = dynamicBg ? 0 : inputManager.GetInput(inputIndex);
                if (initialValue != 0) initialValueExist = true;

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
        }
        public void OnDrag(PointerEventData ped)
        {
            keyState = 2;
            if (eventInvoke) keyEventHandle.Invoke(keyState);
            //
            inputManager.SimulateInput(inputIndex, 1, axisValue);
            // isAxis
            if (axisValue == 2) return;
            Vector2 pedPos;
            Vector2 result;
            if (circularBg) pedPos = ped.position;
            else
            { 
                pedPos = handle.position;
                if (axis == 0) pedPos.x = ped.position.x; else pedPos.y = ped.position.y;
            }
            Vector2 distance_normal = (pedPos - (Vector2)bg.position) / scaleFactor / sizeFactor;
            if (circularBg) result = Vector2.ClampMagnitude(distance_normal, 1f);
            else result = new Vector2(Mathf.Clamp(distance_normal.x, -1, 1), Mathf.Clamp(distance_normal.y, -1, 1));
            handle.localPosition = result * sizeFactor;
            // Set input value
            if (axis == 0) axisValue = result.x; else axisValue = result.y;
            axisValue = axisValue > 0.99f ? 1f : axisValue < -0.99f ? -1 : axisValue;
            //
            if (eventInvoke) axisEventHandle.Invoke(axisValue);
        }
        public void OnPointerDown(PointerEventData ped)
        {
            keyState = 1;
            if (eventInvoke) keyEventHandle.Invoke(keyState);
            //
            inputManager.SimulateInput(inputIndex, 1, axisValue);
            // isAxis
            if (axisValue == 2) return;
            if (dynamicBg)
            {
                Vector2 target = (ped.position - (Vector2)bg.transform.parent.position) / scaleFactor;
                bg.localPosition = target;
                bg.gameObject.SetActive(true);
                // Fix
                if (axis == 0) handle.localPosition = new Vector2(0, handle.localPosition.y) * sizeFactor;
                else handle.localPosition = new Vector2(handle.localPosition.x, 0) * sizeFactor;
                axisValue = 0;
            }
            //
            if (eventInvoke) axisEventHandle.Invoke(axisValue);
        }
        /// <summary>
        /// Note: if the camera changes while the key is released, this method doesn't work. 
        /// In this case, delay the camera switching method. 
        /// Make sure it doesn't conflict with the camera switching method.
        /// </summary>
        public void OnPointerUp(PointerEventData ped)
        {
            keyState = 3;
            if (eventInvoke) keyEventHandle.Invoke(keyState);
            keyState = 0;
            if (eventInvoke) keyEventHandle.Invoke(keyState);
            //
            inputManager.SimulateInput(inputIndex, 2);
            // isAxis
            if (axisValue == 2) return;
            if (!initialValueExist)
            {
                if (axis == 0) handle.localPosition = new Vector2(0, handle.localPosition.y);
                else handle.localPosition = new Vector2(handle.localPosition.x, 0);
                axisValue = 0;
            }
            if (dynamicBg) bg.gameObject.SetActive(false);
            //
            if (eventInvoke) axisEventHandle.Invoke(axisValue);
        }
    }
    [System.Serializable] public class InputEvent : UnityEvent<float> { }
#if UNITY_EDITOR
    [CustomEditor(typeof(UI_Input))]
    public class UI_Input_Editor : UnityEditor.Editor
    {
        private UI_Input _target;
        private GameManager gameManager;
        private SerializedProperty keyEventHandle, axisEventHandle;
        private void Awake()
        {
            try 
            {
                _target = (UI_Input)target;
                gameManager = GameManager.Instance;
            }
            catch { }
            keyEventHandle = serializedObject.FindProperty("keyEventHandle");
            axisEventHandle = serializedObject.FindProperty("axisEventHandle");
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
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Bindings", GUILayout.MinWidth(1));
            string[] bindings = new string[gameManager.inputBindings.Count];
            for (int i = 0; i < bindings.Length; i++) bindings[i] = gameManager.inputBindings[i].name;
            if (bindings.Length == 0)
            {
                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("   Add Input Bindings", EditorStyles.textField, GUILayout.Height(EditorGUIUtility.singleLineHeight), GUILayout.Width(130)))
                {
                    Selection.activeObject = gameManager;
                    gameManager.tab = 1;
                }
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
                        _target.axisValue = EditorGUILayout.Slider(_target.axisValue, -1, 1);
                        EditorGUILayout.EndHorizontal();

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

                    EditorGUILayout.Space(5);
                    EditorGUILayout.BeginHorizontal();
                    _target.eventInvoke = EditorGUILayout.Toggle("Event Invoke", _target.eventInvoke);
                    EditorGUILayout.EndHorizontal();

                    if (_target.eventInvoke)
                    {
                        GUI.backgroundColor = Color.black * 0.4f;
                        serializedObject.Update();

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Label("Key State Event");
                        EditorGUILayout.PropertyField(keyEventHandle, GUIContent.none, GUILayout.Height(EditorGUIUtility.singleLineHeight * 5));
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();

                        GUILayout.Label("Axis Value Event");
                        EditorGUILayout.PropertyField(axisEventHandle, GUIContent.none, GUILayout.Height(EditorGUIUtility.singleLineHeight * 5));
                        EditorGUILayout.EndHorizontal();

                        serializedObject.ApplyModifiedProperties();
                    }

                    EditorUtility.SetDirty(target);
                }
            }
            catch { Selection.activeObject = _target; }
            //base.DrawDefaultInspector();
        }
    }
#endif
}