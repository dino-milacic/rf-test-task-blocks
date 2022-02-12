using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RFTestTaskBlocks
{
    public class BlockController : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        private Transform _transform;
        [SerializeField] private BoxCollider2D _collider;
        [SerializeField] private Rigidbody2D _rigidbody;

        private BlockColor _color;
        private bool _isProcessing;
        private Transform _destination;
        private Action<BlockController> _onProcessingComplete;
        private float _processingTime;
        private Vector3 _startPosition;

        private bool _isTargeted;

        private const float PROCESSING_DURATION = 5f;

        public bool IsTargeted
        {
            get => _isTargeted;
            set => _isTargeted = value;
        }

        public BlockColor Color
        {
            get => _color;
            set
            {
                _color = value;
                _spriteRenderer.color = value.BlockColorToColor();
            }
        }

        public Transform Transform => _transform;

        private void Awake()
        {
            _transform = transform;
            if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_collider == null) _collider = GetComponent<BoxCollider2D>();
            if (_rigidbody == null) _rigidbody = GetComponent<Rigidbody2D>();
        }

        private BlockConfiguration _config;
        
        public void Initialize(BlockConfiguration config)
        {
            _config = config;
            Reinitialize();
        }

        public void Reinitialize()
        {
            if (_config == null)
            {
                Debug.LogWarning("Cannot reinitialize block. Config is missing.");
                return;
            }
            
            SetSpriteOrder(0);
            Vector2 randomPosition = new Vector2(Random.Range(0, _config.AllowedBounds.size.x),
                Random.Range(_config.AllowedBounds.size.y / 2f, _config.AllowedBounds.size.y));
            Transform.position = Services.Get<ISceneManager>().GetPositionInScene(randomPosition);

            Color = _config.AllowedColors[Random.Range(0, _config.AllowedColors.Count)];
            EnablePhysics();
        }

        private void Update()
        {
            if (_isProcessing)
            {
                _processingTime += Time.deltaTime;
                float t = _processingTime / PROCESSING_DURATION;
                Vector3 newPosition = Vector3.Lerp(_startPosition, _destination.position, t);

                if (_processingTime > PROCESSING_DURATION)
                {
                    Transform.position = _destination.position;
                    _isProcessing = false;
                    _onProcessingComplete?.Invoke(this);
                }
                else
                {
                    Transform.position = newPosition;
                }
            }
        }

        private void EnablePhysics()
        {
            _collider.enabled = true;
            _rigidbody.simulated = true;
        }

        public void DisablePhysics()
        {
            _collider.enabled = false;
            _rigidbody.simulated = false;
        }

        public void StartProcessing(Transform destination, Action<BlockController> onProcessingComplete)
        {
            DisablePhysics();
            _processingTime = 0f;
            _destination = destination;
            _startPosition = Transform.position;
            _onProcessingComplete = onProcessingComplete;
            _isProcessing = true;
        }

        public void SetSpriteOrder(int sortingOrder)
        {
            _spriteRenderer.sortingOrder = sortingOrder;
        }
    }
}