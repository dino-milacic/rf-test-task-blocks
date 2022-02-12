using System.Collections.Generic;
using UnityEngine;

namespace RFTestTaskBlocks
{
    public class ContainerController : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        private Transform _transform;

        public Transform Transform => _transform;

        [SerializeField] private ContainerOrientation _receptacleSide = ContainerOrientation.Right;
        [SerializeField] private BlockColor _color;
        [SerializeField] private Transform _dropOffPoint;
        [SerializeField] private Transform _recyclePoint;
        [SerializeField] private Transform _prepareToDropOffPoint;
        [SerializeField] private SpriteRenderer _processingCompleteIndicator;
        [SerializeField] private SpriteRenderer _processingInProgressIndicator;

        [SerializeField] private Queue<BlockController> _processingQueue;
        private bool _isProcessingBlock;
        
        public Transform DropOffPoint => _dropOffPoint;
        public Transform PrepareToDropOffPoint => _prepareToDropOffPoint;
        
        public enum ContainerOrientation
        {
            Left = -1,
            Right = 1
        }
        
        public ContainerOrientation ReceptacleSide
        {
            get => _receptacleSide;
            set
            {
                _receptacleSide = value;
                _transform.localScale = new Vector3((float) _receptacleSide, 1f, 1f);
            }
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

        private void Awake()
        {
            _transform = transform;
            if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();
            _processingQueue = new Queue<BlockController>();
            SetIndicatorAlpha(_processingCompleteIndicator, 0f);
            SetIndicatorAlpha(_processingInProgressIndicator, 0f);
        }
        
        private void OnValidate()
        {
            if (_transform == null) _transform = transform;
            if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();
            
            ReceptacleSide = ReceptacleSide;
            Color = Color;
        }
        
        public void Initialize(ContainerConfiguration config)
        {
            var sceneManager = Services.Get<ISceneManager>();
            Transform.position = sceneManager.GetGroundedPositionInScene(config.SpawnPosition);
            Vector2 scenePos = sceneManager.GetSceneGridPosition(config.SpawnPosition);
            if (scenePos.x < 2)
            {
                ReceptacleSide = ContainerOrientation.Right;
            }
            else if (scenePos.x > sceneManager.SceneSize.max.x - 2)
            {
                ReceptacleSide = ContainerOrientation.Left;
            }
            else
            {
                ReceptacleSide = config.Orientation;
            }
            Color = config.Color;
        }

        public void ReceiveBlock(BlockController block)
        {
            block.Transform.SetPositionAndRotation(_dropOffPoint.position, _dropOffPoint.rotation);
            _processingQueue.Enqueue(block);
        }

        private void Update()
        {
            if (_processingQueue.Count > 0)
            {
                ProcessBlock();
            }

            if (_processingCompleteIndicator.color.a > 0f)
            {
                SetIndicatorAlpha(_processingCompleteIndicator, _processingCompleteIndicator.color.a - 0.5f * Time.deltaTime);
            }

            if (_processingInProgressIndicator.color.a > 0f && !_isProcessingBlock)
            {
                SetIndicatorAlpha(_processingInProgressIndicator, _processingInProgressIndicator.color.a - 0.5f * Time.deltaTime);
            }
        }

        private void ProcessBlock()
        {
            if (_isProcessingBlock) return;

            _isProcessingBlock = true;
            BlockController block = _processingQueue.Dequeue();
            SetIndicatorAlpha(_processingInProgressIndicator, 1f);
            block.StartProcessing(_recyclePoint, BlockProcessed);
        }

        public void BlockProcessed(BlockController block)
        {
            block.Reinitialize();
            Services.Get<ISoundManager>().PlaySFX(SoundAddress.ScannerBeep);
            _isProcessingBlock = false;
            SetIndicatorAlpha(_processingCompleteIndicator, 1f);
        }

        private void SetIndicatorAlpha(SpriteRenderer renderer, float newAlpha)
        {
            Color newColor = renderer.color;
            newColor.a = newAlpha;
            renderer.color = newColor;
        }
    }
}