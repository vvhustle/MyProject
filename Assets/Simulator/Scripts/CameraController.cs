using DG.Tweening;
using UnityEngine;

namespace Simulator
{
    public class CameraController
    {
        private Vector2 hit_position = Vector2.zero;
        private Vector2 current_position = Vector2.zero;
        private Vector2 camera_position = Vector2.zero;
        private Transform canvas;
        public CameraController(Transform mainCanvas)
        {
            canvas = mainCanvas;
        }
        public void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                hit_position = Input.mousePosition;
                camera_position = canvas.position;

            }
            if (Input.GetMouseButton(0))
            {
                current_position = Input.mousePosition;
                LeftMouseDrag();
            }
        }
        private void LeftMouseDrag()
        {
            Vector2 direction = current_position - hit_position;
            Vector2 position = camera_position + direction;
            canvas.position = position;
        }
        public void MoveToCharacter(Vector2 position)
        {
            var rect = canvas.gameObject.GetComponent<RectTransform>();
            var pos = new Vector2(-position.x, -position.y);
            rect.DOAnchorPos(pos, 0.4f).SetEase(Ease.OutBack);
        }
    }
}
