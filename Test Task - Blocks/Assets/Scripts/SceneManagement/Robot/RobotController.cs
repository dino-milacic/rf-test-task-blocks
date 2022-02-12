using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RFTestTaskBlocks
{
    public partial class RobotController : MonoBehaviour
    {
        // private SpriteRenderer _spriteRenderer;
        private Animator _animator;
        private Transform _transform;
        [SerializeField] private Transform _pickUpAnchor;
        [SerializeField] private Animator _trackAnimator;
        [SerializeField] private SpriteRenderer _eye;

        private RobotState _state;
        private BlockController _targetBlock;
        private ContainerController _targetContainer;

        // [SerializeField] private ContainerController[] _containers;

        [SerializeField] private float _maxVelocity = 3f;
        [SerializeField] private float _baseAcceleration = 1f;
        [SerializeField] private float _visionRange = 18f;

        private float _velocity = 0f;
        private float _accelerationSign = 1f;
        private float _v0stop = 0f;
        [SerializeField] private RobotDirection _direction = RobotDirection.Right;
        private bool _slowingDown;
        private bool _changingDirectionInProgress;
        private bool _droppingOffAnimationInProgress;
        private bool _pickUpAnimationInProgress;
        private static readonly int Speed = Animator.StringToHash("Speed");
        private static readonly int Carrying = Animator.StringToHash("Carrying");

        [SerializeField] private int _score = 0;

        public enum RobotDirection
        {
            Left = -1,
            Right = 1
        }

        public Transform Transform => _transform;
        private Transform Eye => _eye.transform;

        public RobotDirection Direction
        {
            get => _direction;
            set
            {
                _direction = value;
                _transform.localScale = new Vector3((float) _direction, 1f, 1f);
            }
        }

        public RobotDirection OppositeDirection => (RobotDirection) ((int)_direction * -1);

        public static RobotDirection RandomDirection
        {
            get
            {
                RobotDirection[] directions = (RobotDirection[]) Enum.GetValues(typeof(RobotDirection));
                return directions[Random.Range(0, directions.Length)];
            }
        }

        public void Initialize(RobotConfiguration config)
        {
            Transform.position = Services.Get<ISceneManager>().GetGroundedPositionInScene(config.SpawnPosition);
            Direction = config.StartingDirection;
            _maxVelocity *= config.SpeedMultiplier;
            _baseAcceleration *= config.SpeedMultiplier;
            _visionRange = config.VisionRange;

            _score = 0;
        }
        
        private void Awake()
        {
            _transform = transform;
            _animator = GetComponent<Animator>();
            ChangeState(new BootingUpState(this));
        }

        private void OnValidate()
        {
            if (_transform == null) _transform = transform;
            
            Direction = Direction;
        }

        private void Update()
        {
            _animator.SetFloat(Speed, _velocity / _maxVelocity);
            _trackAnimator.speed = 2f * _velocity / _maxVelocity;
            _state.DoUpdate();
        }

        private void LateUpdate()
        {
            _state.DoLateUpdate();
        }

        private void FixedUpdate()
        {
            _state.DoFixedUpdate();
        }

        private void ChangeDirection()
        {
            if (_changingDirectionInProgress) return;
            
            _changingDirectionInProgress = true;
            StartCoroutine(ChangeDirectionAnimation());
        }

        private IEnumerator ChangeDirectionAnimation()
        {
            RobotDirection newDirection = OppositeDirection;

            yield return new WaitForSeconds(1f); // simulate robot turn animation

            _changingDirectionInProgress = false;
            Direction = newDirection;
        }

        private bool MoveToPosition(float destinationX)
        {
            Vector3 currentPos = Transform.position;
            float path = destinationX - currentPos.x;
            float distance = Mathf.Abs(path);
            if (_velocity > 0) Direction = (RobotDirection) Mathf.Sign(path);

            float a = _baseAcceleration * _accelerationSign;
            if (!_slowingDown)
            {
                float timeToDecelerate = _velocity / _baseAcceleration;
                float distanceTillStopped = 0.5f * -_velocity * timeToDecelerate + _velocity * timeToDecelerate;
                
                if (distanceTillStopped >= distance - STOP_DISTANCE)
                {
                    _slowingDown = true;
                    _accelerationSign = -1;
                }
            }

            if (distance >= STOP_DISTANCE)
            {
                _velocity += a * Time.deltaTime;
                _velocity = Mathf.Clamp(_velocity, 0.1f, _maxVelocity);
                
                currentPos = new Vector3(currentPos.x + _velocity * Mathf.Sign(path) * Time.deltaTime, currentPos.y, currentPos.z);
                Transform.position = currentPos;
                return false;
            }

            _accelerationSign = 1f;
            _slowingDown = false;
            return true;
        }

        private const float STOP_DISTANCE = 0.5f;

        private void ChangeState(RobotState newState)
        {
            Debug.LogFormat("State {0} -> {1}", _state?.GetType().Name, newState.GetType().Name);
            _state = newState;
            _state.OnSwitched();
        }

        private void SetTargetBlock(BlockController block)
        {
            Debug.LogFormat("Block {0} -> {1}", _targetBlock, block);
            _targetBlock = block;
            if (_targetBlock != null)
            {
                _targetBlock.IsTargeted = true;
                _eye.color = _targetBlock.Color.BlockColorToColor();
            }
            else
            {
                _eye.color = Color.white;;
            }
        }

        private void SetTargetContainer(ContainerController container)
        {
            Debug.LogFormat("Container {0} -> {1}", _targetContainer, container);
            _targetContainer = container;
        }
        
    }
}