using System;
using System.Collections;
using System.Collections.Generic;
using Game;
using Game.Spawner;
using Main;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace Snake
{
    public class StyledSnakeControl : MonoBehaviour
    {
        public static Action<float> rotationValue;
        public static Action jumpSnake;
        public static Action<int> shiedSnake;
        
        public AudioClip[] appleEat;
        public AudioClip starCollect;
        public AudioClip abilityCollect;
        public AudioClip hit;
        public AudioClip explosion;
        public AudioClip jump;
        
        [Range(3, 13)] public int startLenght;
        [Header("Snake Settings: ")]
        public float walkSpeed = 5;
        private float _tempSpeed;
        public float jumpForce;
        public float rotationSpeed;
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
            shiedSnake += Shield;

            if (SceneManager.GetActiveScene().buildIndex != 0)
                Shield(3);
        }

        private void OnDestroy()
        {
            rotationValue -= Rotate;
            jumpSnake -= Jump;
            shiedSnake -= Shield;
        }

        private void Start()
        {
            var pl = SettingsPrefs.GetUpdatePrefs(SettingsPrefs.Snake);
            rotationSpeed += pl * 5;
            walkSpeed += pl / 4;
            bodySpeed += pl / 4;
            if (gap > 1)
                gap -= pl / 4;
            
            for (var i = 0; i < startLenght + SettingsPrefs.GetUpdatePrefs(SettingsPrefs.Snake); i++)
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

        private void Rotate(float val)
        {
            if (_isDead)
                return;
            
            snakeHead.Rotate(Vector3.up * val * rotationSpeed * Time.deltaTime);
        }

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
            {
                Sound.Instance.PlaySound(jump);
                _rigidbody.AddForce(transform.up * jumpForce);
            }
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
                GrowSnake();
                GetScore(100);
                StarManager.AddApple(1);
                Spawner.AppleSpawn?.Invoke(collision.gameObject);
                Sound.Instance.PlaySound(appleEat[Random.Range(0, appleEat.Length)]);
                walkSpeed += .01f;
                bodySpeed += .01f;
                if (gap > 1)
                    gap -= .01f;
            }
            if (collision.gameObject.CompareTag("ObstacleDestroy"))
            {
                Instantiate(objectDestroyFx, collision.transform.position, collision.transform.rotation);
                Sound.Instance.PlaySound(hit);
                GetScore(50);
                GetRandomFromProps();
                Spawner.PropsSpawn?.Invoke(collision.gameObject);
            }
            if (collision.gameObject.CompareTag("Obstacle") || collision.gameObject.CompareTag("Meteor"))
            {
                if (_isCanBreak)
                {
                    Instantiate(objectDestroyFx, collision.transform.position, collision.transform.rotation);
                    Destroy(collision.gameObject);
                    Sound.Instance.PlaySound(hit);
                    GetScore(500);
                }
                else
                {
                    Sound.Instance.PlaySound(explosion);
                    _tempSpeed = walkSpeed;
                    walkSpeed = 0;
                    _isDead = true;
                    DestroySnake();
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (SceneManager.GetActiveScene().buildIndex == 0)
                return;
            
            if (other.CompareTag("Tail"))
            {
                if (_isCanBreak)
                    return;
                
                Sound.Instance.PlaySound(hit);

                _tempSpeed = walkSpeed;
                walkSpeed = 0;
                _isDead = true;
                DestroySnake();
            }
                
        }

        private void DestroySnake()
        {
            SettingsPrefs.SavePrefs(SettingsPrefs.Death, SettingsPrefs.GetPrefs(SettingsPrefs.Death) + 1);
            GameUIManager.Instance?.Lose(callback =>
            {
                StartCoroutine(DestroySnakeCoroutine(callback));
            }, () =>
            {
                walkSpeed = _tempSpeed;
                _isDead = false;
                Shield(3);
            });
        }

        private IEnumerator DestroySnakeCoroutine(Action callback)
        {
            float waitBetween = 2 / _bodyParts.Count;
            foreach (var body in _bodyParts)
            {
                Instantiate(snakeDestroyFx, body.transform.position, body.transform.rotation);
                Sound.Instance.PlaySound(explosion);
                Destroy(body);
                yield return new WaitForSeconds(waitBetween);
            }

            yield return new WaitForSeconds(1);
            
            callback.Invoke();
        }

        #endregion

        #region Status

        private void GetScore(int value)
        {
            GameUIManager.Instance.UpdateScore(value);
        }

        #endregion

        #region Super

        private Coroutine _shieldCoroutine, _speedCoroutine;
        private void Shield(int time) => _shieldCoroutine ??= StartCoroutine(StartShield(time));
        private void Speed(int time) => _speedCoroutine ??= StartCoroutine(StartSpeed(time));

        private IEnumerator StartShield(int time)
        {
            var plus = SettingsPrefs.GetUpdatePrefs(SettingsPrefs.Powers);
            GameUIManager.Instance.ShieldTimer(time + plus);
            shieldFx.Play();
            _isCanBreak = true;
            yield return new WaitForSeconds(time + plus);
            _isCanBreak = false;
            shieldFx.Stop();
            _shieldCoroutine = null;
        }
        
        private IEnumerator StartSpeed(int time)
        {
            var plus = SettingsPrefs.GetUpdatePrefs(SettingsPrefs.Powers);
            GameUIManager.Instance.SuperTimer(time + plus);
            rotationSpeed += plus * 2;
            walkSpeed += plus;
            bodySpeed += plus;
            if (gap - plus > 1)
                gap -= plus;
            yield return new WaitForSeconds(time + plus);
            rotationSpeed -= plus * 2;
            walkSpeed -= plus;
            bodySpeed -= plus;
            if (gap - plus > 1)
                gap += plus;
            _speedCoroutine = null;
        }

        #endregion

        private void GetRandomFromProps()
        {
            var x = Random.Range(0, 100);
            if (x > 80)
            {
                Sound.Instance.PlaySound(abilityCollect);
                switch (x)
                {
                    case < 90:
                        Shield(5);
                        break;
                    case >= 90 and < 97:
                        Speed(5);
                        break;
                    case > 97:
                        GetScore(5000);
                        GrowSnake();
                        GrowSnake();
                        GrowSnake();
                        GrowSnake();
                        GrowSnake();
                        break;
                }
            }
            else
            {
                Sound.Instance.PlaySound(starCollect);
                GameUIManager.Instance.UpdateStars(SettingsPrefs.GetUpdatePrefs(SettingsPrefs.Seeker));
            }
        }
    }
}
