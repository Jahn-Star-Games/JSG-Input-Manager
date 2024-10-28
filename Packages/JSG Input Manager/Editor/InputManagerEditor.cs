// Developed by Halil Emre Yildiz (github: JahnStar)
using System;
using System.Collections;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace JahnStarGames.InputManager
{
    [CustomEditor(typeof(InputManager))]
    public class InputManagerEditor : Editor
    {
        private InputManager _target;
        private ReorderableList _inputHandlerList;

        private void Awake()
        {
            _target = (InputManager)target;
            InputBindingEditor.Clear();
        }

        private static bool drawInputCounts = false;
        public override void OnInspectorGUI()
        {
            if (_target == null) return;
            else if (InputManager.refleshEditor)
            {
                InputManager.refleshEditor = false;
                InputManager.Instance.StartCoroutine(SelectInputManager(1));
            }

            try
            {
                void titleCallback() 
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(15);
                    _target.drawInputBinding = EditorGUILayout.Foldout(_target.drawInputBinding, " API");
                    GUILayout.Space(-22);
                    _target.inputBinding = (InputBinding)EditorGUILayout.ObjectField(_target.inputBinding, typeof(InputBinding), false);
                    EditorGUILayout.EndHorizontal();
                    GUILayout.Space(2);
                }
                void contentCallback() 
                {
                    GUILayout.Space(3);
                    if (_target.drawInputBinding) InputBindingEditor.DrawInputBinding(_target.inputBinding, true);
                }
                DrawGUI("Input Manager", titleCallback, contentCallback);

                GUILayout.Space(6);
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                bool newDrawInputHandler = GUILayout.Toggle(_target.drawInputHandler, EditorGUIUtility.IconContent(_target.drawInputHandler ? "animationvisibilitytoggleon" : "animationvisibilitytoggleoff"), EditorStyles.label);
                _target.drawInputHandler = newDrawInputHandler;
                GUILayout.Label(" Input Handler", EditorStyles.boldLabel);
                drawInputCounts = GUILayout.Toggle(drawInputCounts, EditorGUIUtility.IconContent(drawInputCounts ? "d_UnityEditor.ConsoleWindow" : "d_UnityEditor.ConsoleWindow"), EditorStyles.label); 
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.Space(5);

                if (drawInputCounts)
                {
                    GUILayout.Space(-EditorGUIUtility.singleLineHeight);
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.Label($"Inputs: {InputManager.InputHandler.Count} / {InputManager.Instance.virtualInputs.Count}", EditorStyles.miniLabel);
                    GUILayout.EndHorizontal();
                    GUILayout.Space(4);
                }

                if (_target.drawInputHandler) 
                {
                    if (_inputHandlerList == null) InitializeVirtualInputs();
                    else _inputHandlerList.DoLayoutList();
                }

                serializedObject.ApplyModifiedProperties();
                serializedObject.Update();
                Repaint();
            }
            catch (ArgumentException)
            {
                EditorGUILayout.HelpBox("Please drag and drop an InputBinding asset.", MessageType.Warning);
                try { InputBindingEditor.Clear(); } catch { } 
            }
        }

        public void DrawGUI(string title, Action titleCallback, Action contentCallback)
        {
            var headerStyle = new GUIStyle(EditorStyles.helpBox)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(5, 5, 5, 5)
            };
            GUI.backgroundColor = Color.black;
            headerStyle.normal.textColor = Color.white;
            EditorGUILayout.BeginVertical(headerStyle);
            EditorGUILayout.LabelField("--- Input System ---\nJahn Star Games", headerStyle);
            //
            GUI.backgroundColor = Color.white;
            GUILayout.Space(3);
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Space(2);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(title, EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(2);
            GUILayout.BeginHorizontal();
            titleCallback?.Invoke();
            GUILayout.EndHorizontal();
            GUILayout.Space(3);
            contentCallback?.Invoke();
            GUILayout.EndVertical();
            EditorGUILayout.EndVertical();
        }

        public void InitializeVirtualInputs()
        {
            _target = InputManager.Instance;
            if (InputManager.InputHandler.Count != _target.virtualInputs.Count) _target.UpdateInputHandler();
            // Reorderable List
            _inputHandlerList = new(serializedObject, serializedObject.FindProperty("virtualInputs"), true, true, true, true)
            {
                drawHeaderCallback = (Rect rect) => EditorGUI.LabelField(rect, "Virtual Inputs"),
                drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    var element = _inputHandlerList.serializedProperty.GetArrayElementAtIndex(index);
                    var inputHandler = _target.virtualInputs;
                    string name = inputHandler[index];
                    float value = InputManager.InputHandler.ContainsKey(name) ? InputManager.InputHandler[name] : 0;

                    float singleLineHeight = EditorGUIUtility.singleLineHeight;
                    float valueWidth = singleLineHeight * 3;
                    float lineHeight = singleLineHeight;
                    rect.y += 2;

                    // Name and Value
                    inputHandler[index] = EditorGUI.TextField(new Rect(rect.x, rect.y, rect.width / 2 - valueWidth - 4, singleLineHeight), name);

                    EditorGUI.BeginChangeCheck();
                    float newValue = EditorGUI.FloatField(new Rect(rect.x + rect.width / 2 - valueWidth - 2, rect.y, valueWidth, singleLineHeight), value);
                    if (EditorGUI.EndChangeCheck()) 
                    {
                        _target.UpdateInputHandler();
                        InputManager.SetInput(name, newValue);
                        serializedObject.ApplyModifiedProperties();
                        serializedObject.Update();
                    }

                    if (value < 0) GUI.color = Color.red + Color.white * 0.6f;
                    EditorGUI.ProgressBar(new Rect(rect.x + rect.width / 2, rect.y, rect.width / 2, singleLineHeight), Mathf.Abs(value), "");
                    GUI.color = Color.white;
                    rect.y += lineHeight;
                },
                elementHeightCallback = (int index) => EditorGUIUtility.singleLineHeight,
                onAddCallback = (ReorderableList list) =>
                {
                    if (_target.virtualInputs.Contains("New Input"))
                    {
                        int i = 1;
                        while (_target.virtualInputs.Contains($"New Input {i}")) i++;
                        _target.virtualInputs.Add($"New Input {i}");
                    }
                    else _target.virtualInputs.Add("New Input");
                    serializedObject.ApplyModifiedProperties();
                    serializedObject.Update();
                },
                onRemoveCallback = (ReorderableList list) =>
                {
                    _target.virtualInputs.RemoveAt(list.index);
                    _target.UpdateInputHandler();
                    serializedObject.ApplyModifiedProperties();
                    serializedObject.Update();
                }
            };
        }
        public static void SelectInputManager() => Selection.activeObject = InputManager.Instance;
        public static IEnumerator SelectInputManager(float delay) 
        {
            Selection.activeObject = null;
            yield return new WaitForSeconds(delay);
            SelectInputManager();
        }
    }
}