using UnityEngine;
using System.Collections;

namespace uNature.Demo
{
    public class UN_CameraFly : MonoBehaviour
    {
        public static UN_CameraFly _instance;
        public static UN_CameraFly instance
        {
            get
            {
                return _instance;
            }
        }

        public static bool canMove = true;

        [SerializeField]
        public new Camera camera;

        public float normalspeed = 10;
        public float speed
        {
            get { return normalspeed * (Input.GetKey(KeyCode.LeftShift) ? 2 : 1); }
        }

        public float sensitivity = 2;

        float yaw;
        float pitch;

        long lastCheckedSaveBytesLength;

        private void Awake()
        {
            _instance = this;
        }

        public virtual void Update()
        {
            if (canMove)
            {
                #region HandleMovement
                Vector3 movement = Vector3.zero;

                movement += (transform.forward * Input.GetAxis("Vertical") * speed);
                movement += (transform.right * Input.GetAxis("Horizontal") * speed);

                transform.position += movement * Time.deltaTime;
                #endregion

                #region Mouse
                if (Input.GetMouseButton(1))
                {
                    yaw += (Input.GetAxisRaw("Mouse X") * sensitivity);
                    yaw %= 360;

                    pitch += (-Input.GetAxisRaw("Mouse Y") * sensitivity);
                    pitch = Mathf.Clamp(pitch, -85, +85);

                    transform.eulerAngles = new Vector3(pitch, yaw, 0);
                }
                #endregion
            }
        }
    }
}