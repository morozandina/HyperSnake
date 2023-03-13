using System.Collections;
using UnityEngine;

namespace Game.Meteor
{
    public class BlastWave : MonoBehaviour
    {
        public int pointsCount;
        public float maxRadius;
        public float speed;
        public float startWidth;
        private LineRenderer _lineRenderer;

        private void Awake()
        {
            _lineRenderer = GetComponent<LineRenderer>();

            _lineRenderer.positionCount = pointsCount + 1;
        }

        private void Start()
        {
            StartCoroutine(Blast());
        }

        private IEnumerator Blast()
        {
            var currentRadius = 0f;
            while (currentRadius < maxRadius)
            {
                currentRadius += Time.deltaTime * speed;
                Draw(currentRadius);
                yield return null;
            }
        }

        private void Draw(float currentRadius)
        {
            var angleBetweenPoints = 360f / pointsCount;

            for (var i = 0; i <= pointsCount; i++)
            {
                var angle = i * angleBetweenPoints * Mathf.Deg2Rad;
                var direction = new Vector3(Mathf.Sin(angle), Mathf.Cos(angle), 0f);
                var position = direction * currentRadius;
                
                _lineRenderer.SetPosition(i, position);
            }

            _lineRenderer.widthMultiplier = Mathf.Lerp(0f, startWidth, 1f - currentRadius / maxRadius);
        }
    }
}
