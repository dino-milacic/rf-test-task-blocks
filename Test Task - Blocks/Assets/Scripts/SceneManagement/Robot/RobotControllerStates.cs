using System.Collections;
using System.Linq;
using UnityEngine;

namespace RFTestTaskBlocks
{
    public partial class RobotController : MonoBehaviour
    {
        private const string LAYER_BLOCKS = "Blocks";
        
        public abstract class RobotState
        {
            protected RobotController _robot;

            public RobotState(RobotController robot)
            {
                _robot = robot;
            }

            public virtual void OnSwitched() {}
            
            public virtual void DoUpdate() {}
            public virtual void DoLateUpdate() {}
            public virtual void DoFixedUpdate() {}
        }

        public sealed class BootingUpState : RobotState
        {
            private float _time;
            private float _timeDelay;
            private const float TIME_DELAY_MIN = 2f;
            private const float TIME_DELAY_MAX = 5f;

            public BootingUpState(RobotController robot) : base(robot)
            {
                _timeDelay = Random.Range(TIME_DELAY_MIN, TIME_DELAY_MAX);
            }

            public override void DoUpdate()
            {
                _time += Time.deltaTime;
                if (_time >= _timeDelay)
                {
                    _robot.ChangeState(new SearchForBlockState(_robot));   
                }
            }
        }

        public sealed class SearchForBlockState : RobotState
        {
            public SearchForBlockState(RobotController robot) : base(robot) {}

            public override void DoFixedUpdate()
            {
                if (LookForBlockInDirection(_robot.Direction, out BlockController block))
                {
                    _robot.SetTargetBlock(block);
                    _robot.ChangeState(new GoToTargetBlockState(_robot));
                }
                else
                {
                    _robot.ChangeDirection();
                }
            }

            private bool LookForBlockInDirection(RobotDirection direction, out BlockController block)
            {
                Debug.DrawRay(_robot.Eye.position, Vector3.right * (int) direction * _robot._visionRange, Color.red);
                RaycastHit2D hit = Physics2D.Raycast(_robot.Eye.position, Vector3.right * (int) direction, _robot._visionRange, LayerMask.GetMask(LAYER_BLOCKS));
                bool foundBlock = hit.collider != null;
                block = foundBlock ? hit.collider.GetComponent<BlockController>() : null;
                
                if (block != null && block.IsTargeted) return false;
                return foundBlock;
            }
        }

        public sealed class GoToTargetBlockState : RobotState
        {
            public GoToTargetBlockState(RobotController robot) : base(robot) {}

            public override void DoUpdate()
            {
                if (_robot.MoveToPosition(_robot._targetBlock.Transform.position.x))
                {
                    _robot._targetBlock.DisablePhysics();
                    _robot.ChangeState(new PickUpBlockState(_robot));
                    
                }
                
                Debug.DrawLine(_robot.Eye.position, _robot._targetBlock.Transform.position, Color.green);
            }
        }

        public sealed class PickUpBlockState : RobotState
        {
            public PickUpBlockState(RobotController robot) : base(robot) {}

            public override void OnSwitched()
            {
                Pickup();
            }

            private void Pickup()
            {
                if (_robot._pickUpAnimationInProgress) return;
            
                _robot._pickUpAnimationInProgress = true;
                _robot.StartCoroutine(PickUpAnimation());
            }

            private IEnumerator PickUpAnimation()
            {
                AttachTargetBlock();
                _robot._animator.SetBool(Carrying, true);
                Services.Get<ISoundManager>().PlaySFX(SoundAddress.RobotBlip01);
                yield return new WaitForSeconds(1.2f);
                _robot._pickUpAnimationInProgress = false;
                _robot.ChangeState(new FindContainerState(_robot));
            }

            private void AttachTargetBlock()
            {
                _robot._targetBlock.IsTargeted = false;
                _robot._targetBlock.Transform.SetParent(_robot._pickUpAnchor, false);
                _robot._targetBlock.SetSpriteOrder(1);
                _robot._targetBlock.Transform.localPosition = Vector3.zero;
            }
        }

        public sealed class FindContainerState : RobotState
        {
            public FindContainerState(RobotController robot) : base(robot)
            {
            }

            public override void OnSwitched()
            {
                ContainerController targetContainer =
                    Services.Get<ISceneManager>().GetContainerForColor(_robot._targetBlock.Color);
                if (targetContainer != null)
                {
                    Debug.DrawLine(_robot.Eye.position, targetContainer.DropOffPoint.position, Color.green);
                    _robot.SetTargetContainer(targetContainer);
                    if (IsBehindContainer())
                    {
                        _robot.ChangeState(new PrepareToDeliverBlockState(_robot));
                    }
                    else
                    {
                        _robot.ChangeState(new DeliverBlockState(_robot));
                    }
                }
            }

            private bool IsBehindContainer()
            {
                if (_robot._targetContainer == null) return false;

                float distance = _robot._targetContainer.DropOffPoint.position.x -_robot.Transform.position.x;
                return Mathf.Abs(Mathf.Sign(distance) - (int) _robot._targetContainer.ReceptacleSide) < 1e-4;
            }
        }

        public sealed class PrepareToDeliverBlockState : RobotState
        {
            public PrepareToDeliverBlockState(RobotController robot) : base(robot)
            {
            }

            public override void DoUpdate()
            {
                Debug.DrawLine(_robot.Eye.position, _robot._targetContainer.PrepareToDropOffPoint.position, Color.green);

                if (_robot.MoveToPosition(_robot._targetContainer.PrepareToDropOffPoint.position.x))
                {
                    _robot.ChangeState(new DeliverBlockState(_robot));
                }
            }
        }

        public sealed class DeliverBlockState : RobotState
        {
            public DeliverBlockState(RobotController robot) : base(robot) {}

            public override void DoUpdate()
            {
                Debug.DrawLine(_robot.Eye.position, _robot._targetContainer.DropOffPoint.position, Color.green);

                if (_robot.MoveToPosition(_robot._targetContainer.DropOffPoint.position.x))
                {
                    Deliver();
                }
            }

            private void Deliver()
            {
                if (_robot._droppingOffAnimationInProgress) return;
            
                _robot._droppingOffAnimationInProgress = true;
                _robot.StartCoroutine(DeliverAnimation());
            }

            private IEnumerator DeliverAnimation()
            {
                _robot._targetBlock.SetSpriteOrder(-2);
                _robot._animator.SetBool(Carrying, false);
                Services.Get<ISoundManager>().PlaySFX(SoundAddress.RobotBlip02);
                _robot._score++;
                
                yield return new WaitForSeconds(0.5f);
                
                _robot._droppingOffAnimationInProgress = false;
                DetachTargetBlock();
            
                _robot._targetContainer.ReceiveBlock(_robot._targetBlock);
                _robot.SetTargetBlock(null);
                _robot.SetTargetContainer(null);
                _robot.ChangeState(new SearchForBlockState(_robot));
            }

            private void DetachTargetBlock()
            {
                _robot._targetBlock.Transform.SetParent(null);
            }
        }
    }
}