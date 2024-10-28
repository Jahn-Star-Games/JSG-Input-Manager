// Developed by Halil Emre Yildiz (github: JahnStar)
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace JahnStarGames.InputManager
{
    /// <summary>
    /// The InputManager class acts as a mediator to receive data from the InputBinding class.
    /// It handles adding, holding, and reading virtual inputs.
    /// </summary>
    [Serializable]
    public class InputManager : MonoBehaviour
    {
        private static InputManager _instance;
        public static InputManager Instance
        {
            get
            {
                if (_instance == null) 
                {
                    _instance = FindObjectOfType<InputManager>();
                    if (_instance == null) _instance = new GameObject("Input Manager").AddComponent<InputManager>();
                }
                return _instance;
            }
        }
        //
        [HideInInspector] public bool drawInputBinding = false, drawInputHandler = true;
        [HideInInspector] public static bool refleshEditor = false;
        public InputBinding inputBinding;
        public List<string> virtualInputs = new();
        protected internal  Dictionary<string, float> inputHandler = new();
        public static Dictionary<string, float> InputHandler => Instance.inputHandler;
        //
        private void Awake()
        {
            if (_instance == null) _instance = this;
            else if (_instance != this) Destroy(gameObject);
            DontDestroyOnLoad(gameObject);
            inputHandler = new();
            UpdateInputHandler();
        }
        private void Start() => refleshEditor = true;
        //
        public void UpdateInputHandler()
        {
            foreach (string key in virtualInputs) if (!inputHandler.ContainsKey(key)) inputHandler.Add(key, 0);
            foreach (string key in new List<string>(inputHandler.Keys)) if (!virtualInputs.Contains(key)) inputHandler.Remove(key);
        }

        public static float GetRawInput(string key) => Instance.inputBinding.GetInput(key);
        public static float GetInput(string key)
        {
            if (InputHandler.ContainsKey(key)) return InputHandler[key];
            try { return GetRawInput(key); }
            catch (KeyNotFoundException) 
            {
                if (!Instance.virtualInputs.Contains(key)) Instance.virtualInputs.Add(key);
                Instance.UpdateInputHandler();
                Debug.LogWarning($"Get input '{key}' failed because it does not exist. Virtual input '{key}' has been created and set to 0.");
                return 0;
            }            
        }
        public static void SetInput(string key, float value)
        {
            if (!InputHandler.ContainsKey(key)) 
            {
                if (!Instance.virtualInputs.Contains(key)) Instance.virtualInputs.Add(key);
                Instance.UpdateInputHandler();
                Debug.LogWarning($"Set input '{key}' does not exist. Virtual input '{key}' has been created and set to '{value}'.");
            }
            InputHandler[key] = value;
        }

        public static float FetchInput(string key, string inputName, bool updateHandler = true, bool unityInput = false)
        {
            inputName ??= key;
            float value;
            try { value = !unityInput ? GetRawInput(inputName) : Input.GetAxis(inputName); }
            catch (KeyNotFoundException) { throw new KeyNotFoundException($"Fetch input '{key}' failed because key does not exist in the selected Input Binding."); }
            if (updateHandler) SetInput(key, value);
            return value;
        }
        
        public static float FetchInput(string key, KeyCode keycode, int keyState = 0, bool updateHandler = true)
        {
            float value;
            try { value = GetInput(keycode) ? 1 : 0; }
            catch (KeyNotFoundException) { throw new KeyNotFoundException($"Fetch input '{key}' failed because key does not exist in the selected Input Binding."); }
            if (updateHandler) SetInput(key, value);
            return value;
            bool GetInput(KeyCode keyCode) => keyState == 0 ? Input.GetKeyDown(keyCode) : keyState == 1 ? Input.GetKey(keyCode) : keyState == 2 && Input.GetKeyUp(keyCode);
        }

        public static void SetInputBinding(InputBinding inputBinding)
        {
            Instance.inputBinding = inputBinding;
            refleshEditor = true;
        }
    }
}