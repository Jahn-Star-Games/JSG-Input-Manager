// Developed by Halil Emre Yildiz (github: JahnStar)
using System;
using UnityEngine;

namespace JahnStarGames.InputManager
{
    [Serializable]
    public struct GameInput
    {
        public string Key;
        public string InputName;

        public GameInput(string key, string inputName = null)
        {
            Key = key;
            InputName = string.IsNullOrEmpty(inputName) ? key : inputName;
        }

        public readonly float Get() => InputManager.GetInput(Key);
        public readonly void Set(float value) => InputManager.SetInput(Key, value);
        public readonly float Fetch(bool updateHandler = true, bool unityInput = false) => InputManager.FetchInput(Key, InputName, updateHandler, unityInput);
        public readonly float Fetch(KeyCode keyCode, int keyState, bool updateHandler = true) => InputManager.FetchInput(Key, keyCode, keyState, updateHandler);

        /// <param name="state"> pressed: 0, pressing: 1, released: 2 </param>
        public readonly bool Fetch(int state)
        {
            float previousValue = Get();
            float newValue = Fetch(true);
            bool result = false;

            switch (state)
            {
                case 0: result = newValue > previousValue; break; // pressed
                case 1: result = newValue > 0; break; // pressing
                case 2: result = newValue < previousValue; break; // released
            }

            return result;
        }
    }
}