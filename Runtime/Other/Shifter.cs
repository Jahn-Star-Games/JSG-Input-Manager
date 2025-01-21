using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JahnStarGames.InputBindings
{
    [AddComponentMenu("Jahn Star Games/Input Manager/Utility/Shifter"), RequireComponent(typeof(RectTransform), typeof(Image))]
    public class Shifter : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        public RectTransform levelKnob, levelN_0, levelD_1, levelR_2;
        public Text levelKnobText;
        [Space] public InputEvent eventHandler;
        [System.Serializable] public class InputEvent : UnityEvent<float> { }
        private float firstPosY;
        private int currentLevel = 0, targetLevel = 1;
        private void Start() => SetKnobPosition(targetLevel);
        private void SetKnobPosition(int level)
        {
            if (level == 0)
            {
                levelKnob.position = levelN_0.position;
                levelKnobText.text = "N";
            }
            else if (level == 1)
            {
                levelKnob.position = levelD_1.position;
                levelKnobText.text = "D";
            }
            else if (level == 2)
            {
                levelKnob.position = levelR_2.position;
                levelKnobText.text = "R";
            }
        }
        public void OnDrag(PointerEventData ped)
        {
            if (ped.position.y - firstPosY > 0) targetLevel = Mathf.Clamp(currentLevel + 1, 0, 2);
            else if (ped.position.y - firstPosY < 0) targetLevel = Mathf.Clamp(currentLevel - 1, 0, 2);
            //
            SetKnobPosition(targetLevel);
        }
        public void OnPointerDown(PointerEventData ped) => firstPosY = ped.position.y;
        public void OnPointerUp(PointerEventData ped) => eventHandler.Invoke(currentLevel = targetLevel);
    }
}