// Developed by Halil Emre Yildiz (github: JahnStar)
using System;
using UnityEngine;
using System.Linq;
using UnityEditor;

namespace JahnStarGames.InputManager
{
    [CustomPropertyDrawer(typeof(GameInput), true)]
    public class GameInputDrawer : PropertyDrawer
    {
        private bool drawInputNameProp = false;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!InputManager.Instance.drawInputHandler) return;

            SerializedProperty keyProp = property.FindPropertyRelative("Key");
            SerializedProperty inputNameProp = property.FindPropertyRelative("InputName");
            EditorGUI.BeginProperty(position, label, property);

            // Calculate widths
            float popupWidth = position.width * 0.7f;
            float valueWidth = position.width - popupWidth;
            float singleLineHeight = EditorGUIUtility.singleLineHeight;

            // Rects
            var popupRect = new Rect(position.x, position.y, popupWidth, position.height);
            var valueRect = new Rect(position.x + popupWidth, position.y, valueWidth, position.height);
            var inputNameRect = new Rect(position.x + popupWidth + 2, position.y, valueWidth - singleLineHeight, position.height);
            var drawInputNameRect = new Rect(position.x + popupWidth + valueWidth - singleLineHeight + 2, position.y + 1, singleLineHeight - 2, singleLineHeight - 2);

            // Cache InputManager instance
            var virtualInputs = InputManager.Instance.virtualInputs;

            // Dropdown
            string[] options = virtualInputs.ToArray();
            if (options.Length == 0) options = new string[] { $"Missing: {keyProp.stringValue}" };
            else if (string.IsNullOrEmpty(keyProp.stringValue)) keyProp.stringValue = options[0];
            else if (!options.Contains(keyProp.stringValue) && keyProp.stringValue != "New Input" && !virtualInputs.Contains(keyProp.stringValue)) virtualInputs.Add(keyProp.stringValue);

            int currentIndex = Mathf.Max(0, Array.IndexOf(options, keyProp.stringValue));
            currentIndex = EditorGUI.Popup(popupRect, label.text, currentIndex, options);
            if (currentIndex >= 0 && currentIndex < options.Length) keyProp.stringValue = options[currentIndex];

            // Draw Input Name
            if (keyProp.stringValue != inputNameProp.stringValue) drawInputNameProp = !Application.isPlaying;
            if (!Application.isPlaying && GUI.Button(drawInputNameRect, EditorGUIUtility.IconContent("StandaloneInputModule Icon"), EditorStyles.miniLabel)) drawInputNameProp = !drawInputNameProp;
        
            // Value
            if (!drawInputNameProp) 
            {
                float value = InputManager.InputHandler.ContainsKey(keyProp.stringValue) ? InputManager.InputHandler[keyProp.stringValue] : 0;
                if (value < 0) GUI.color = Color.red + Color.white * 0.6f;
                EditorGUI.BeginDisabledGroup(true);
                EditorGUI.ProgressBar(valueRect, Mathf.Abs(value), value.ToString("0.00"));
                EditorGUI.EndDisabledGroup();
                GUI.color = Color.white;
            }
            else inputNameProp.stringValue = EditorGUI.TextField(inputNameRect, inputNameProp.stringValue);
            
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!InputManager.Instance.drawInputHandler) return 0;
            return base.GetPropertyHeight(property, label);
        }
    }
}