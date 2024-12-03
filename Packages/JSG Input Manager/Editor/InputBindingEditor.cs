using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using static JahnStarGames.InputManager.InputData;

namespace JahnStarGames.InputManager
{
    [CustomEditor(typeof(InputBinding))]
    public class InputBindingEditor : Editor
    {
        InputBinding _target;
        static ReorderableList reorderableList;

        private void Awake()
        {
            _target = (InputBinding)target;
            if (_target) InitializeReorderableList(_target, serializedObject);
        }

        internal static void InitializeReorderableList(InputBinding target, SerializedObject serializedObject, bool disabled = false)
        {
            reorderableList = new(serializedObject, serializedObject.FindProperty("inputs"), true, true, true, true)
            {
                drawHeaderCallback = (Rect rect) => EditorGUI.LabelField(rect, "Inputs")
            };

            reorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = reorderableList.serializedProperty.GetArrayElementAtIndex(index);
                var input = target.inputs[index];

                float singleLineHeight = EditorGUIUtility.singleLineHeight;
                float lineHeight = singleLineHeight + 1;
                rect.y += 2;

                // Name and Value
                EditorGUILayout.BeginHorizontal();
                input.draw = EditorGUI.Foldout(new Rect(rect.x + singleLineHeight - 4, rect.y, rect.width, singleLineHeight), input.draw, GUIContent.none);
                if (disabled) EditorGUI.BeginDisabledGroup(true);
                EditorGUI.PropertyField(new Rect(rect.x + singleLineHeight, rect.y, rect.width / 2 - singleLineHeight - 4, singleLineHeight), element.FindPropertyRelative("name"), GUIContent.none);
                if (disabled) EditorGUI.EndDisabledGroup();
                if (input.currentValue < 0) GUI.color = Color.red + Color.white * 0.6f;
                EditorGUI.ProgressBar(new Rect(rect.x + rect.width / 2, rect.y, rect.width / 2, singleLineHeight), Mathf.Abs(input.currentValue), input.currentValue.ToString("0.000"));
                GUI.color = Color.white;
                EditorGUILayout.EndHorizontal();
                rect.y += lineHeight;

                if (disabled) EditorGUI.BeginDisabledGroup(true);
                if (input.draw)
                {
                    // Remote Unity Input
                    EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width / 2, singleLineHeight), "Use Input System");
                    GUI.backgroundColor = Color.white * 0.7f;
                    if (string.IsNullOrEmpty(input.remote)) input.remote = EditorGUI.TextField(new Rect(rect.x + rect.width - singleLineHeight, rect.y, singleLineHeight, singleLineHeight), input.remote);
                    else input.remote = EditorGUI.TextField(new Rect(rect.x + rect.width / 2, rect.y, rect.width / 2, singleLineHeight), input.remote);
                    GUI.backgroundColor = Color.white;
                    rect.y += lineHeight;

                    if (string.IsNullOrEmpty(input.remote))
                    {
                        // Type
                        EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width / 2, singleLineHeight), "Type");
                        input.type = (InputData.Type)EditorGUI.EnumPopup(new Rect(rect.x + rect.width / 2, rect.y, rect.width / 2, singleLineHeight), input.type);
                        rect.y += lineHeight;

                        if (input.type == InputData.Type.Axes)
                        {
                            // Positive Key
                            EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width / 2, singleLineHeight), "Positive Key");
                            input.positiveKey = (KeyCodes)EditorGUI.EnumPopup(new Rect(rect.x + rect.width / 2, rect.y, rect.width / 2, singleLineHeight), input.positiveKey);
                            rect.y += lineHeight;

                            // Negative Key
                            EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width / 2, singleLineHeight), "Negative Key");
                            input.negativeKey = (KeyCodes)EditorGUI.EnumPopup(new Rect(rect.x + rect.width / 2, rect.y, rect.width / 2, singleLineHeight), input.negativeKey);
                            rect.y += lineHeight;

                            // Sensitivity
                            EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width / 2, singleLineHeight), "Sensitivity");
                            input.sensitivity = EditorGUI.FloatField(new Rect(rect.x + rect.width / 2, rect.y, rect.width / 2, singleLineHeight), input.sensitivity);
                            rect.y += lineHeight;

                            // Gravity
                            EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width / 2, singleLineHeight), "Gravity");
                            input.gravity = EditorGUI.FloatField(new Rect(rect.x + rect.width / 2, rect.y, rect.width / 2, singleLineHeight), input.gravity);
                            rect.y += lineHeight;

                            // Dead
                            EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width / 2, singleLineHeight), "Dead");
                            input.deadZone = EditorGUI.FloatField(new Rect(rect.x + rect.width / 2, rect.y, rect.width / 2, singleLineHeight), input.deadZone);
                            rect.y += lineHeight;
                        }
                        else
                        {
                            // Key
                            EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width / 2, singleLineHeight), "Key");
                            input.key = (KeyCodes)EditorGUI.EnumPopup(new Rect(rect.x + rect.width / 2, rect.y, rect.width / 2, singleLineHeight), input.key);
                            rect.y += lineHeight;
                        }
                    }
                }
                if (disabled) EditorGUI.EndDisabledGroup();
            };

            reorderableList.elementHeightCallback = (int index) =>
            {
                var input = target.inputs[index];
                float lineHeight = EditorGUIUtility.singleLineHeight + 1;
                if (!input.draw) return lineHeight;
                else if (string.IsNullOrEmpty(input.remote))
                {
                    if (input.type == InputData.Type.Axes) return lineHeight * 8;
                    else return lineHeight * 4;
                }
                else return lineHeight * 2;
            };

            reorderableList.onAddCallback = (ReorderableList list) =>
            {
                target.inputs.Add(new InputData("New Input"));
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
            };

            reorderableList.onRemoveCallback = (ReorderableList list) =>
            {
                if (EditorUtility.DisplayDialog("Warning!", "Are you sure you want to delete this input?", "Yes", "No"))
                {
                    ReorderableList.defaultBehaviours.DoRemoveButton(list);
                    serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(target);
                }
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (reorderableList == null) InitializeReorderableList(_target, serializedObject);
            DrawTitle("Input Binding");

            reorderableList.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }

        static GUIStyle headerStyle;
        public static void DrawTitle(string title)
        {
            headerStyle ??= new GUIStyle(EditorStyles.helpBox)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(5, 5, 5, 5)
            };
            GUI.backgroundColor = Color.black;
            headerStyle.normal.textColor = Color.white;
            EditorGUILayout.BeginVertical(headerStyle);
            EditorGUILayout.LabelField("--- Input System v1.0.5 ---\nJahn Star Games", headerStyle);
            //
            GUI.backgroundColor = Color.white;
            GUILayout.Space(3);
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.BeginHorizontal();
            // if (GUILayout.Button("Open Manager", EditorStyles.miniButton, GUILayout.ExpandWidth(false))) InputManagerEditor.SelectInputManager();
            GUILayout.FlexibleSpace();
            GUILayout.Label(title, EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            // if (GUILayout.Button("Set As Default", EditorStyles.miniButton, GUILayout.ExpandWidth(false)))
            // {
            //     InputManager inputManager = InputManager.Instance;
            //     inputManager.inputBinding = (InputBinding)Selection.activeObject;
            //     Selection.activeObject = inputManager;
            // }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            EditorGUILayout.EndVertical();
        }
        
        public static void DrawInputBinding(InputBinding inputBinding, bool disabled = false)
        {
            if (inputBinding == null || inputBinding.inputs.Count == 0) return;
            var inputHandlerObj = new SerializedObject(inputBinding);

            if (reorderableList == null) InitializeReorderableList(inputBinding, inputHandlerObj, disabled);
            else 
            {
                inputHandlerObj.Update();
                reorderableList.DoLayoutList();

                inputHandlerObj.ApplyModifiedProperties();
                inputHandlerObj.Update();
                EditorUtility.SetDirty(inputBinding);
            }
        }

        public static void Clear() => reorderableList = null;
    }
}