#if UN_PhotonBolt

using UnityEngine;
using System.Collections;

using uNature.Core.Networking;

namespace uNature.Extensions.PhotonBolt
{
    public class PhotonBoltPlayerController : UNNetworkPlayerController
    {
        protected override bool hasControl
        {
            get
            {
                return entity.isAttached && entity.hasControl;
            }
        }

        struct State
        {
            public bool forward;
            public bool backwards;
            public bool right;
            public bool left;

            public bool jump;

            public float yaw;
            public float pitch;
        }
        State input;

        public PlayerMotor motor;

        public override void Attached()
        {
            state.transform.SetTransforms(this.transform);
        }

        public override void ControlGained()
        {
            base.OnAttached();
        }

        void Update()
        {
            GetInputs(false);
        }

        void GetInputs(bool simulated)
        {
            input.forward = Input.GetKey(KeyCode.W);
            input.backwards = Input.GetKey(KeyCode.S);
            input.right = Input.GetKey(KeyCode.D);
            input.left = Input.GetKey(KeyCode.A);
            input.jump = Input.GetKey(KeyCode.Space);

            if (!simulated)
            {
                input.yaw += (Input.GetAxisRaw("Mouse X") * 2);
                input.yaw %= 360;

                input.pitch += (-Input.GetAxisRaw("Mouse Y") * 2);
                input.pitch = Mathf.Clamp(input.pitch, -85, +85);
            }
        }

        public override void SimulateController()
        {
            GetInputs(true);

            IPlayerCommandInput cmd = PlayerCommand.Create();

            cmd.forward = input.forward;
            cmd.backward = input.backwards;
            cmd.right = input.right;
            cmd.left = input.left;

            cmd.jump = input.jump;

            cmd.yaw = input.yaw;
            cmd.pitch = input.pitch;

            entity.QueueInput(cmd);
        }

        public override void ExecuteCommand(Bolt.Command command, bool resetState)
        {
            var cmd = (PlayerCommand)command;

            if (resetState)
            {
                motor.SetState(cmd.Result.Position, cmd.Result.Velocity, cmd.Result.IsGrounded, cmd.Result.JumpFrames);
            }
            else
            {
                PlayerMotor.State Result = motor.Move(cmd.Input.forward, cmd.Input.backward, cmd.Input.left, cmd.Input.right, cmd.Input.jump, cmd.Input.yaw);

                //Send Results
                cmd.Result.Position = Result.position;
                cmd.Result.Velocity = Result.velocity;
                cmd.Result.JumpFrames = Result.jumpFrames;
                cmd.Result.IsGrounded = Result.isGrounded;

                if (cmd.IsFirstExecution)
                {
                    base.Camera.transform.localEulerAngles = new Vector3(cmd.Input.pitch, 0, 0);
                }
            }
        }
    }
}

#endif