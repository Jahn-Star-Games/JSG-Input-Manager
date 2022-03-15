//Developed by Halil Emre Yildiz - @Jahn_Star
//https://github.com/JahnStar
//https://jahnstar.github.io
using UnityEngine;
using UnityEditor;
using UnityEngine.EventSystems;

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
        public float scaleFactor = 1;
        [HideInInspector]
        public Vector2 sizeFactor = Vector2.one;
        [HideInInspector]
        public float axisValue = 2;
        [HideInInspector]
        public bool dynamicBg, circularBg;
        [HideInInspector]
        public RectTransform bg, handle;
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
            if (axisValue > 0.99f) axisValue = 1f; else if (axisValue < -0.99f) axisValue = -1f;
        }
        public void OnPointerDown(PointerEventData ped)
        {
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
        }
        public void OnPointerUp(PointerEventData ped)
        {
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
        }
    }
#if UNITY_EDITOR
    [CustomEditor(typeof(UI_Input))]
    public class UI_Input_Editor : Editor
    {
        private UI_Input _target;
        private GameManager gameManager;
        private void Awake()
        {
            try 
            {
                _target = (UI_Input)target;
                gameManager = GameManager.Instance;
            }
            catch { }
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
                        EditorGUILayout.LabelField("Current Value ", GUILayout.MinWidth(1));
                        _target.axisValue = EditorGUILayout.Slider(_target.axisValue, -1, 1);
                        EditorGUI.EndDisabledGroup();
                        EditorGUILayout.EndHorizontal();

                        EditorUtility.SetDirty(_target);
                    }
                    else _target.axisValue = 2;
                    EditorUtility.SetDirty(target);
                }
            }
            catch { Selection.activeObject = _target; }
            //base.DrawDefaultInspector();
        }
    }
#endif
}