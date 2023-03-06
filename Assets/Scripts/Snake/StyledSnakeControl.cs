using System;
using System.Collections;
using System.Collections.Generic;
using Game.Spawner;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Snake
{
    public class StyledSnakeControl : MonoBehaviour
    {
        public static Action<float> rotationValue;
        public static Action jumpSnake;
        [Range(3, 13)] public int startLenght;
        [Header("Snake Settings: ")]
        public float walkSpeed = 5;
        public float jumpForce = 10;
        public float rotationSpeed = 180;
        public Transform snakeHead;
        public LayerMask groundedMask;
        
        private bool _grounded;
        private Vector3 _moveAmount;
        private Vector3 _smoothMoveVelocity;
        private float _verticalLookRotation;
        private Rigidbody _rigidbody;

        [Header("SNAKE: ")]
        public GameObject bodyPrefab;
        public float gap;
        public float bodySpeed = 4;
        private List<GameObject> _bodyParts = new List<GameObject>();
        private List<Vector3> _positionHistory = new List<Vector3>();

        [Header("Visual")]
        [SerializeField] private ParticleSystem eatFx;
        [SerializeField] private ParticleSystem snakeDestroyFx;
        [SerializeField] private ParticleSystem objectDestroyFx;
        [SerializeField] private ParticleSystem shieldFx;

        private bool _isDead;
        private bool _isCanBreak;


        private void Awake() {
            _rigidbody = GetComponent<Rigidbody>();
            rotationValue += Rotate;
            jumpSnake += Jump;

            StartCoroutine(Shield(3));
        }

        private void OnDestroy()
        {
            rotationValue -= Rotate;
            jumpSnake -= Jump;
        }

        private void Start()
        {
            for (var i = 0; i < startLenght; i++)
            {
                GrowSnake();
            }
        }

        #region Snake Control Movemente

        private void Update()
        {
            Movement();
            
            // Grounded check
            var ray = new Ray(transform.position, -transform.up);
            _grounded = Physics.Raycast(ray, out _, 1 + .1f, groundedMask);
        }
        
        private void FixedUpdate()
        {
            // Apply movement to rigidbody
            var localMove = snakeHead.TransformDirection(_moveAmount) * Time.fixedDeltaTime;
            _positionHistory.Insert(0, transform.position);
            _rigidbody.MovePosition(_rigidbody.position + localMove);

            MoveSnakeBody();
        }

        private void Rotate(float val) => snakeHead.Rotate(Vector3.up * val * rotationSpeed * Time.deltaTime);

        private void Movement()
        {
            // Calculate movement:
            var moveDir = new Vector3(0,0, 1).normalized;
            var targetMoveAmount = moveDir * walkSpeed;
            _moveAmount = Vector3.SmoothDamp(_moveAmount,targetMoveAmount,ref _smoothMoveVelocity,.15f);
        }

        private void Jump()
        {
            if (_isDead)
                return;
            // Jump
            if (_grounded)
                _rigidbody.AddForce(transform.up * jumpForce);
        }

        #endregion

        #region SNAKE

        private void GrowSnake()
        {
            var body = Instantiate(bodyPrefab);
            _bodyParts.Add(body);
        }

        private void MoveSnakeBody()
        {
            if (_isDead)
                return;
            
            var index = 0;
            foreach (var body in _bodyParts)
            {
                var point = _positionHistory[Mathf.Min(index * (int)gap, _positionHistory.Count - 1)];
                var moveDirection = point - body.transform.position;
                body.transform.position += moveDirection * bodySpeed * Time.deltaTime;
                body.transform.LookAt(point);
                index++;
            }
        }

        #endregion

        #region Growing

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Apple"))
            {
                Instantiate(eatFx, collision.transform.position, collision.transform.rotation);
                Destroy(collision.gameObject);
                GrowSnake();
                Spawner.AppleSpawn?.Invoke();
                walkSpeed += .01f;
                bodySpeed += .01f;
                gap += .01f;
            }
            if (collision.gameObject.CompareTag("ObstacleDestroy"))
            {
                Instantiate(objectDestroyFx, collision.transform.position, collision.transform.rotation);
                Destroy(collision.gameObject);
            }
            if (collision.gameObject.CompareTag("Obstacle") || collision.gameObject.CompareTag("Tail"))
            {
                if (_isCanBreak)
                {
                    Instantiate(objectDestroyFx, collision.transform.position, collision.transform.rotation);
                    Destroy(collision.gameObject);
                }
                else
                {
                    walkSpeed = 0;
                    _isDead = true;
                    StartCoroutine(DestroySnake());
                }
            }
        }

        private IEnumerator DestroySnake()
        {
            foreach (var body in _bodyParts)
            {
                Instantiate(snakeDestroyFx, body.transform.position, body.transform.rotation);
                Destroy(body);
                yield return new WaitForSeconds(.2f);
            }

            SceneManager.LoadScene(0);
        }

        private IEnumerator Shield(float time)
        {
            shieldFx.Play();
            _isCanBreak = true;
            yield return new WaitForSeconds(time);
            _isCanBreak = false;
            shieldFx.Stop();
        }

        #endregion
    }
}
