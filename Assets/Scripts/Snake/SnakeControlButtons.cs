using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Snake
{
    public class SnakeControlButtons : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        private bool _isHandle = false;

        private void FixedUpdate()
        {
            if (!_isHandle)
                return;

            var halfScreen = Screen.width / 2;
            var xPos = (Input.mousePosition.x - halfScreen) / halfScreen;
            var val = Mathf.Clamp(xPos * 3.2f, -1f, 1f);
            
            if (val != 0)
                StyledSnakeControl.rotationValue?.Invoke(val);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!_isHandle)
                StyledSnakeControl.jumpSnake?.Invoke();
        }
        // Drag with move
        public void OnBeginDrag(PointerEventData eventData) => _isHandle = true;
        public void OnDrag(PointerEventData eventData) { }
        public void OnEndDrag(PointerEventData eventData) => _isHandle = false;
    }
}
