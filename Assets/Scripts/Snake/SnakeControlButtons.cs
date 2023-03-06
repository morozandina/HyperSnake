using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Snake
{
    [Serializable]
    public enum MoveType
    {
        Left,
        Jump,
        Right
    }
    public class SnakeControlButtons : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public MoveType moveType;
        private bool _isHandle = false;

        private void FixedUpdate()
        {
            if (!_isHandle)
                return;
            
            switch (moveType)
            {
                case MoveType.Left:
                    StyledSnakeControl.rotationValue?.Invoke(-1);
                    break;
                case MoveType.Jump:
                    StyledSnakeControl.jumpSnake?.Invoke();
                    _isHandle = false;
                    break;
                case MoveType.Right:
                    StyledSnakeControl.rotationValue?.Invoke(1);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        public void OnPointerDown(PointerEventData eventData) => _isHandle = true;
        public void OnPointerUp(PointerEventData eventData) => _isHandle = false;
    }
}
