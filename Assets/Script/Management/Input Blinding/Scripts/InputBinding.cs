// Developed by Halil Emre Yildiz - </> 2021
//https://github.com/JahnStar
//https://jahnstar.github.io/donate/
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

using KeyCodes = JahnStar.CoreThreeD.InputData.KeyCodes;
namespace JahnStar.CoreThreeD
{
    [SerializeField]
    [CreateAssetMenu(menuName = "JahnStar/Core/Input Bindings", fileName = "Input Bindings")]
    public class InputBinding : ScriptableObject
    {
        [SerializeField] internal List<InputData> inputs = new List<InputData>();
        internal Dictionary<string, int> inputIndex;
        /// <summary>
        /// <para>Eger 'hierarchy' penceresinde 'GameManager.cs > inputBindings' listesi var ise</para>
        /// <para>Girdi listesindeki adi ile girdi oku = GetInput(name, type)</para>
        /// <para>Girdi listesindeki sirasi ile girdi oku = GetInput(index, type)</para>
        /// </summary>
        /// <returns> Eger girdi 'axis' ise -1 = Min ve 1 = Max arasinda deger dondurur. Eger 'button' ise 1 = true, 0 = false olarak dondurur.</returns>
        /// <param name="name">inputs listesindeki adi</param>
        /// <param name="type"> 0 = GetKeyDown, 1 = GetKey, 2 = GetKeyUp</param>
        internal float GetInput(string name, int type = 1)
        {
            if (inputIndex == null)
            {
                inputIndex = new Dictionary<string, int>();
                for (int i = 0; i < inputs.Count; i++) inputIndex.Add(inputs[i].name, i);
            }
            return GetInput(inputIndex[name], type: type);
        }
        /// <summary>
        /// <para>Eger 'hierarchy' penceresinde 'GameManager.cs > inputBindings' listesi var ise</para>
        /// <para>Girdi listesindeki adi ile girdi oku = GetInput(name, type)</para>
        /// <para>Girdi listesindeki sirasi ile girdi oku = GetInput(index, type)</para>
        /// </summary>
        /// <returns> Eger girdi 'axis' ise -1 = Min ve 1 = Max arasinda deger dondurur. Eger 'button' ise 1 = true, 0 = false olarak dondurur.</returns>
        /// <param name="index">inputs listesindeki sirasi</param>
        /// <param name="type"> 0 = GetKeyDown, 1 = GetKey, 2 = GetKeyUp</param>
        public float GetInput(int index, int type = 1)
        {
            try
            {
                if (index > -1)
                {
                    InputData input = inputs[index];

                    if (input.remote)
                    {
                        input.currentValue = Input.GetAxis(input.name);
                        return input.currentValue;
                    }
                    else
                    {
                        if (input.type == InputData.Type.Axes)
                        {
                            if (input.uiButtonState > 0) // Mobile, UI
                            {
                                if (input.uiButtonState == 2) input.uiButtonState = 0;
                                input.currentValue = input.uiAxisState;
                            }
                            else // Keyboard, Mouse, Gamepad
                            {
                                float target = 0;
                                if (input.negativeKey != KeyCodes.__ && Input.GetKey(KeyCodeToString(input.negativeKey))) target = -1;
                                if (input.positiveKey != KeyCodes.__ && Input.GetKey(KeyCodeToString(input.positiveKey))) target = 1;

                                input.currentValue = Mathf.MoveTowards(input.currentValue, target, ((Mathf.Abs(target) > Mathf.Epsilon) ? Time.deltaTime / input.sensitivity : Time.deltaTime * input.gravity));
                            }
                            return (Mathf.Abs(input.currentValue) < input.deadZone) ? 0 : input.currentValue;
                        }
                        else // Button - Key
                        {
                            if (input.uiButtonState > 0) // Mobile, UI
                            {
                                if (type == 0) // press down
                                {
                                    if (input.uiButtonState == 1) // pressing
                                    {
                                        input.currentValue = 1;
                                        input.uiButtonState = 0;
                                    }
                                    else input.currentValue = input.uiButtonState = 0; // not press
                                }
                                else if (type == 2) // press up
                                {
                                    if (input.uiButtonState == 2) // press up
                                    {
                                        input.currentValue = 1;
                                        input.uiButtonState = 0;
                                    }
                                    else input.currentValue = input.uiButtonState = 0; // not press
                                }
                                else // pressing
                                {
                                    if (input.uiButtonState == 1) input.currentValue = 1; // pressing
                                    else input.currentValue = input.uiButtonState = 0; // not press
                                }
                            }
                            else // Keyboard, Mouse, Gamepad
                            {
                                if (input.key == KeyCodes.__ || Mathf.Abs(Time.timeScale) < float.Epsilon) return input.currentValue = 0;

                                if (type == 0) input.currentValue = Input.GetKeyDown(KeyCodeToString(input.key)) ? 1 : 0;
                                else if (type == 2) input.currentValue = Input.GetKeyUp(KeyCodeToString(input.key)) ? 1 : 0;
                                else input.currentValue = Input.GetKey(KeyCodeToString(input.key)) ? 1 : 0;
                            }
                            return input.currentValue;
                        }
                    }
                }
                else throw new Exception();
            }
            catch (Exception e) { throw new Exception("index: " + index + ", 'InputBinding > inputs' not found! (" + e.Message + ")"); }
        }
        public string KeyCodeToString(KeyCodes key)
        {
            string keyCode = key + "";
            if (keyCode[0] == '_') keyCode = keyCode.Substring(1);
            return keyCode.Replace('_', ' ');
        }
        /// <param name="uiPressState"> 0 = NotPressed, 1 = KeyPress, 2 = KeyUp</param>
        /// <param name="uiAxisState"> Between -1 and 1 </param>
        internal void SimulateInput(int index, int uiPressState, float uiAxisState = 2)
        {
            inputs[index].uiButtonState = uiPressState;
            if (uiAxisState < 2) inputs[index].uiAxisState = uiAxisState;
        }
    }
    #if UNITY_EDITOR
    [CustomEditor(typeof(InputBinding))]
    public class InputBinding_Editor : UnityEditor.Editor
    {
        InputBinding _target;
        private void Awake()
        {
            try { _target = (InputBinding)target; }
            catch { }
        }
        public override void OnInspectorGUI()
        {
            try { for (int i = 0; i < _target.inputs.Count; i++) _target.GetInput(i); }
            catch { }
            GUIStyle title = GUIStyle.none;
            title.fontSize = 15;
            title.alignment = TextAnchor.MiddleCenter;
            title.fontStyle = FontStyle.Bold;
            title.normal.textColor = Color.white;

            GUIStyle text = EditorStyles.label;
            text.normal.textColor = Color.white;
            text.alignment = TextAnchor.MiddleLeft;

            GUIStyle field = EditorStyles.textField;
            field.normal.textColor = Color.white;
            field.fontSize = 12;
            field.alignment = TextAnchor.MiddleLeft;

            GUIStyle currentValue = EditorStyles.miniLabel;
            currentValue.fontSize = 12;
            currentValue.fontStyle = FontStyle.Bold;
            currentValue.normal.textColor = Color.white;
            currentValue.alignment = TextAnchor.MiddleRight;

            Color contentColor = GUI.contentColor = new Color(0, 0.9f, 1, 1);
            Color backgroundColor = GUI.backgroundColor = Color.black * 0.45f;

            GUILayout.Space(5);
            GUILayout.Box("Developed by Halil Emre Yildiz", GUILayout.MinWidth(Screen.width - 37));
            GUILayout.Space(5);
            GUILayout.Label(_target.name, title);
            GUILayout.Space(7.5f);

            Rect rect2 = EditorGUILayout.GetControlRect(false, 1f);
            rect2.xMin = 0f;
            rect2.width = Screen.width;
            EditorGUI.DrawRect(rect2, Color.black * 0.75f);

            int remove = -1;
            for (int i = 0; i < _target.inputs.Count; i++)
            {
                InputData input = _target.inputs[i];

                GUI.Box(new Rect(0, GUILayoutUtility.GetLastRect().y, Screen.width, 30f), "");
                GUILayout.Space(2f);
                EditorGUILayout.BeginHorizontal();
                input.draw = EditorGUILayout.Foldout(input.draw, input.name + " - index: " + i, EditorStyles.foldout);
                if (input.currentValue != 0) EditorGUILayout.LabelField(input.currentValue.ToString("0.00"), currentValue);

                GUI.contentColor = Color.white;
                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("x", GUILayout.Height(EditorGUIUtility.singleLineHeight), GUILayout.Width(EditorGUIUtility.singleLineHeight))) remove = i;
                GUI.contentColor = contentColor;
                GUI.backgroundColor = backgroundColor;
                EditorGUILayout.EndHorizontal();

                if (input.draw)
                {
                    GUILayout.Space(10);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Name ", GUILayout.MinWidth(1));
                    input.name = EditorGUILayout.TextField(input.name);
                    EditorGUILayout.EndHorizontal();

                    try
                    {
                        Input.GetAxis(input.name);

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Remote Unity Input");
                        GUI.backgroundColor = Color.white * 0.8f;
                        input.remote = EditorGUILayout.Toggle(input.remote);
                        GUI.backgroundColor = backgroundColor;
                        EditorGUILayout.EndHorizontal();
                    }
                    catch { input.remote = false; }

                    GUILayout.Space(5);
                    if (!input.remote)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Type ", GUILayout.MinWidth(1));
                        input.type = (InputData.Type)EditorGUILayout.EnumPopup(input.type);
                        EditorGUILayout.EndHorizontal();

                        if (input.type == InputData.Type.Axes)
                        {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField("Positive Key ", GUILayout.MinWidth(1));
                            input.positiveKey = (KeyCodes)EditorGUILayout.EnumPopup(input.positiveKey);
                            EditorGUILayout.EndHorizontal();

                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField("Negative Key ", GUILayout.MinWidth(1));
                            input.negativeKey = (KeyCodes)EditorGUILayout.EnumPopup(input.negativeKey);
                            EditorGUILayout.EndHorizontal();

                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField("Sensitivity ", GUILayout.MinWidth(1));
                            input.sensitivity = EditorGUILayout.FloatField(input.sensitivity);
                            EditorGUILayout.EndHorizontal();

                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField("Gravity ", GUILayout.MinWidth(1));
                            input.gravity = EditorGUILayout.FloatField(input.gravity);
                            EditorGUILayout.EndHorizontal();

                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField("Dead ", GUILayout.MinWidth(1));
                            input.deadZone = EditorGUILayout.FloatField(input.deadZone);
                            EditorGUILayout.EndHorizontal();
                        }
                        else
                        {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField("Key ", GUILayout.MinWidth(1));
                            input.key = (KeyCodes)EditorGUILayout.EnumPopup(input.key);
                            EditorGUILayout.EndHorizontal();
                        }

                        EditorUtility.SetDirty(_target);
                    }
                }
                GUILayout.Space(5);

                rect2 = EditorGUILayout.GetControlRect(false, 1f);
                rect2.xMin = 0f;
                rect2.width = Screen.width;
                EditorGUI.DrawRect(rect2, Color.black * 0.75f);
            }

            if (remove != -1)
            {
                if (EditorUtility.DisplayDialog("Are you sure?", "Do you want to delete '" + _target.inputs[remove].name + "' ?", "Yes", "No"))
                {
                    Undo.RecordObject(_target, "Undo Inspector");
                    _target.inputs.RemoveAt(remove);
                    EditorUtility.SetDirty(_target);
                }
            }
            GUILayout.Space(5);
            GUILayout.FlexibleSpace();
            EditorGUI.BeginChangeCheck();
            if (GUILayout.Button("Add", GUILayout.Height(25), GUILayout.Width(Screen.width - 40)))
            {
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(_target, "Undo Inspector");
                    _target.inputs.Add(new InputData("Axis" + (_target.inputs.Count + 1)));

                    EditorUtility.SetDirty(_target);
                    return;
                }
            }
        }
    }
    #endif
    [Serializable]
    public class InputData
    {
        internal bool draw;
        public string name;
        public bool remote;
        public Type type;
        [Header("Keys")]
        public KeyCodes key;
        [Header("Axes")]
        public KeyCodes positiveKey;
        public KeyCodes negativeKey;
        public float sensitivity = 0.4f, gravity = 4f, deadZone = 0.01f;
        [Header("State")]
        public float currentValue;
        [Space]
        /// <param name="uiButtonState"> 0 = NotPressed, 1 = KeyPress, 2 = KeyUp</param>
        public float uiButtonState = 0;
        /// <param name="uiAxisState"> Between -1 and 1 </param>
        public float uiAxisState = 0;
        public enum Type { Axes, Key }
        public enum KeyCodes
        {
            __, mouse_0, mouse_1, mouse_2,
            up, right, left, down,
            q, w, e, r, t, y, u, i, o, p, a, s, d, f, g, h, j, k, l, z, x, c, v, b, n, m, _1, _2, _3, _4, _5, _6, _7, _8, _9, _0,
            space, backspace,
            left_shift, right_shift,
            left_ctrl, right_ctrl,
            left_alt, right_alt,
            tab, escape,
            f1, f2, f3, f4, f5, f6, f7, f8, f9, f10, f11, f12, f13, f14, f15,
            joystick_button_0, joystick_button_1, joystick_button_2, joystick_button_3, joystick_button_4,
            joystick_button_5, joystick_button_6, joystick_button_7, joystick_button_8, joystick_button_9,
            joystick_button_10, joystick_button_11, joystick_button_12, joystick_button_13, joystick_button_14,
            joystick_button_15, joystick_button_16, joystick_button_17, joystick_button_18, joystick_button_19,
        }
        public InputData(string inputName)
        {
            this.name = inputName;
        }
    }
}