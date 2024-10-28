//Last Fix 22.5.2023 (Fixed: Distance method is not working issue.)
//Last Edit: 11.11.2023
//Developed by Halil Emre Yildiz - @Jahn_Star
//https://github.com/JahnStar
//https://jahnstar.github.io
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;
using System.IO;

namespace JahnStarGames.InputManager
{
    [CustomEditor(typeof(UI_Input))]
    public class UI_InputEditor : UnityEditor.Editor
    {
        private UI_Input _target;
        private SerializedProperty keyEventHandler, axisEventHandler;
        private void Awake()
        {
            try {  _target = (UI_Input)target; }
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
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Bindings", GUILayout.MinWidth(1));
            string[] guids = AssetDatabase.FindAssets("t:InputBinding");
            string[] names = guids.Select(guid => Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(guid))).ToArray();
            int index = !_target.inputBinding ? -1 : Array.IndexOf(names, _target.inputBinding.name);
            if (guids.Length == 0)
            {
                GUI.backgroundColor = Color.red;
                GUILayout.Box("Add a Input Binding on Game Manager", EditorStyles.textField);
                GUI.backgroundColor = defaultColor;
            }
            else
            {
                if (index < 0) index = 0;
                index = Array.IndexOf(names, names[EditorGUILayout.Popup(index, names)]);
                _target.inputBinding = AssetDatabase.LoadAssetAtPath<InputBinding>(AssetDatabase.GUIDToAssetPath(guids[0]));
                _target.inputBinding.name = names[index];
                GUI.backgroundColor = defaultColor;
            }
            EditorGUILayout.EndHorizontal();
            //
            try
            {
                if (_target.inputBinding)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Input Name ", GUILayout.MinWidth(1));

                    string[] inputs = new string[_target.inputBinding.inputs.Count];
                    for (int i = 0; i < inputs.Length; i++)
                    {
                        inputs[i] = _target.inputBinding.inputs[i].name;
                        if (_target.inputBinding.inputs[i].name == _target.inputName) _target.inputIndex = i;
                    }
                    _target.inputIndex = EditorGUILayout.Popup(_target.inputIndex, inputs);
                    _target.inputName = _target.inputBinding.inputs[_target.inputIndex].name;
                    EditorGUILayout.EndHorizontal();

                    // Axis stick
                    if (_target.inputBinding.inputs[_target.inputIndex].type == InputData.Type.Axes)
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
}