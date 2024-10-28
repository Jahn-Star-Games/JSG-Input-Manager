// Devloped by Halil Emre Yildiz (github: JahnStar)
using System;
using System.Collections.Generic;
using UnityEngine;

using KeyCodes = JahnStarGames.InputManager.InputData.KeyCodes;
namespace JahnStarGames.InputManager
{
    [SerializeField]
    [CreateAssetMenu(menuName = "Jahn Star Games/Input Manager/Create Input Binding", fileName = "Input Binding")]
    public class InputBinding : ScriptableObject
    {
        [SerializeField] public List<InputData> inputs = new List<InputData>();
        internal Dictionary<string, int> inputIndex;
        private Dictionary<KeyCodes, string> keyCodeToStringCache = new Dictionary<KeyCodes, string>();

        private void InitializeInputIndex()
        {
            if (inputIndex == null)
            {
                inputIndex = new Dictionary<string, int>();
                for (int i = 0; i < inputs.Count; i++) inputIndex.Add(inputs[i].name, i);
            }
        }

        public Dictionary<string, float> GetKeyValues()
        {
            Dictionary<string, float> keyValues = new();
            for (int i = 0; i < inputs.Count; i++) keyValues.Add(inputs[i].name, inputs[i].currentValue);
            return keyValues;
        }

        /// <param name="name">The name in the inputs list</param>
        /// <param name="type">0 = GetKeyDown, 1 = GetKey, 2 = GetKeyUp</param>
        public float GetInput(string name, int type = 1)
        {
            InitializeInputIndex();
            return GetInput(inputIndex[name], type: type);
        }

        public float GetInput(int index, int type = 1)
        {
            try
            {
                if (index > -1)
                {
                    InputData input = inputs[index];

                    if (!string.IsNullOrEmpty(input.remote))
                    {
                        try { input.currentValue = Input.GetAxis(input.remote); }
                        catch { }
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
            if (!keyCodeToStringCache.TryGetValue(key, out string keyCode))
            {
                keyCode = key.ToString();
                if (keyCode[0] == '_') keyCode = keyCode.Substring(1);
                keyCode = keyCode.Replace('_', ' ');
                keyCodeToStringCache[key] = keyCode;
            }
            return keyCode;
        }

        /// <param name="uiButtonState"> 0 = NotPressed, 1 = KeyPress, 2 = KeyUp </param>
        /// <param name="uiAxisState"> Between -1 and 1 </param>
        internal void SimulateInput(int index, int uiPressState, float uiAxisState = 2)
        {
            inputs[index].uiButtonState = uiPressState;
            if (uiAxisState < 2) inputs[index].uiAxisState = uiAxisState;
        }

        internal void SimulateInput(string name, int uiPressState = 2, float uiAxisState = 2) => SimulateInput(inputIndex[name], uiPressState, uiAxisState);
    }

    [Serializable]
    public class InputData
    {
        public bool draw = false;
        public string name;
        public string remote;
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
        public float uiButtonState = 0; // 0 = NotPressed, 1 = KeyPress, 2 = KeyUp
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