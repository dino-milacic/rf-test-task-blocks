using System.Collections;
using UnityEngine;

namespace RFTestTaskBlocks
{
    public partial class RobotController
    {
        private const string LAYER_BLOCKS = "Blocks";
        
        public abstract class RobotState
        {
            protected readonly RobotController Robot;

            protected RobotState(RobotController robot)
            {
                Robot = robot;
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
            private const float TIME_DELAY_MIN = 1f;
            private const float TIME_DELAY_MAX = 3f;

            public BootingUpState(RobotController robot) : base(robot)
            {
                _timeDelay = Random.Range(TIME_DELAY_MIN, TIME_DELAY_MAX);
            }

            public override void DoUpdate()
            {
                _time += Time.deltaTime;
                if (_time >= _timeDelay)
                {
                    Robot.ChangeState(new SearchForBlockState(Robot));   
                }
            }
        }

        public sealed class SearchForBlockState : RobotState
        {
            public SearchForBlockState(RobotController robot) : base(robot) {}

            public override void DoFixedUpdate()
            {
                if (LookForBlockInDirection(Robot.Direction, out BlockController block))
                {
                    Robot.SetTargetBlock(block);
                    Robot.ChangeState(new GoToTargetBlockState(Robot));
                }
                else
                {
                    Robot.ChangeDirection();
                }
            }

            private bool LookForBlockInDirection(RobotDirection direction, out BlockController block)
            {
                Debug.DrawRay(Robot.Eye.position, Vector3.right * (int) direction * Robot._visionRange, Color.red);
                RaycastHit2D hit = Physics2D.Raycast(Robot.Eye.position, Vector3.right * (int) direction, Robot._visionRange, LayerMask.GetMask(LAYER_BLOCKS));
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
                if (Robot.MoveToPosition(Robot._targetBlock.Transform.position.x))
                {
                    Robot._targetBlock.DisablePhysics();
                    Robot.ChangeState(new PickUpBlockState(Robot));
                    
                }
                
                Debug.DrawLine(Robot.Eye.position, Robot._targetBlock.Transform.position, Color.green);
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
                if (Robot._pickUpAnimationInProgress) return;
            
                Robot._pickUpAnimationInProgress = true;
                Robot.StartCoroutine(PickUpAnimation());
            }

            private IEnumerator PickUpAnimation()
            {
                AttachTargetBlock();
                Robot._animator.SetBool(Carrying, true);
                Services.Get<ISoundManager>().PlaySFX(SoundAddress.RobotBlip01);
                
                yield return new WaitForSeconds(1.2f);
                
                Robot._pickUpAnimationInProgress = false;
                Robot.ChangeState(new FindContainerState(Robot));
            }

            private void AttachTargetBlock()
            {
                Robot._targetBlock.IsTargeted = false;
                Robot._targetBlock.Transform.SetParent(Robot._pickUpAnchor, false);
                Robot._targetBlock.SetSpriteOrder(1);
                Robot._targetBlock.Transform.localPosition = Vector3.zero;
            }
        }

        public sealed class FindContainerState : RobotState
        {
            public FindContainerState(RobotController robot) : base(robot) {}

            public override void OnSwitched()
            {
                ContainerController targetContainer =
                    Services.Get<ISceneManager>().GetContainerForColor(Robot._targetBlock.Color);
                if (targetContainer != null)
                {
                    Debug.DrawLine(Robot.Eye.position, targetContainer.DropOffPoint.position, Color.green);
                    Robot.SetTargetContainer(targetContainer);
                    if (IsBehindContainer())
                    {
                        Robot.ChangeState(new PrepareToDeliverBlockState(Robot));
                    }
                    else
                    {
                        Robot.ChangeState(new DeliverBlockState(Robot));
                    }
                }
            }

            private bool IsBehindContainer()
            {
                if (Robot._targetContainer == null) return false;

                float distance = Robot._targetContainer.DropOffPoint.position.x -Robot.Transform.position.x;
                return Mathf.Abs(Mathf.Sign(distance) - (int) Robot._targetContainer.ReceptacleSide) < 1e-4;
            }
        }

        public sealed class PrepareToDeliverBlockState : RobotState
        {
            public PrepareToDeliverBlockState(RobotController robot) : base(robot)
            {
            }

            public override void DoUpdate()
            {
                Debug.DrawLine(Robot.Eye.position, Robot._targetContainer.PrepareToDropOffPoint.position, Color.green);

                if (Robot.MoveToPosition(Robot._targetContainer.PrepareToDropOffPoint.position.x))
                {
                    Robot.ChangeState(new DeliverBlockState(Robot));
                }
            }
        }

        public sealed class DeliverBlockState : RobotState
        {
            public DeliverBlockState(RobotController robot) : base(robot) {}

            public override void DoUpdate()
            {
                Debug.DrawLine(Robot.Eye.position, Robot._targetContainer.DropOffPoint.position, Color.green);

                if (Robot.MoveToPosition(Robot._targetContainer.DropOffPoint.position.x))
                {
                    Deliver();
                }
            }

            private void Deliver()
            {
                if (Robot._droppingOffAnimationInProgress) return;
            
                Robot._droppingOffAnimationInProgress = true;
                Robot.StartCoroutine(DeliverAnimation());
            }

            private IEnumerator DeliverAnimation()
            {
                Robot._targetBlock.SetSpriteOrder(-2);
                Robot._animator.SetBool(Carrying, false);
                Services.Get<ISoundManager>().PlaySFX(SoundAddress.RobotBlip02);
                Robot._score++;
                
                yield return new WaitForSeconds(0.5f);
                
                Robot._droppingOffAnimationInProgress = false;
                DetachTargetBlock();
            
                Robot._targetContainer.ReceiveBlock(Robot._targetBlock);
                Robot.SetTargetBlock(null);
                Robot.SetTargetContainer(null);
                Robot.ChangeState(new SearchForBlockState(Robot));
            }

            private void DetachTargetBlock()
            {
                Robot._targetBlock.Transform.SetParent(null);
            }
        }
    }
}