using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace UnityStandardAssets.Characters.FirstPerson
{
    [Serializable]
    public class MouseLook
    {
        public float XSensitivity = 2f;
        public float YSensitivity = 2f;
        public bool clampVerticalRotation = true;
        public float MinimumX = -90F;
        public float MaximumX = 90F;
        public bool smooth;
        public float smoothTime = 5f;

		private Quaternion m_CameraTargetRot;

        public void Init(Transform camera)
        {
            m_CameraTargetRot = camera.localRotation;
        }

        public void LookRotation(Transform camera)
        {
            float yRot = CrossPlatformInputManager.GetAxis("Mouse X") * XSensitivity;
            float xRot = CrossPlatformInputManager.GetAxis("Mouse Y") * YSensitivity;

            m_CameraTargetRot *= Quaternion.Euler(-xRot, yRot, 0f);
            m_CameraTargetRot = Quaternion.Euler(new Vector3(m_CameraTargetRot.eulerAngles.x, m_CameraTargetRot.eulerAngles.y, camera.localEulerAngles.z));

            if (smooth)
            {
                camera.localRotation = Quaternion.Slerp (camera.localRotation, m_CameraTargetRot,
                    smoothTime * Time.deltaTime);
            }
            else
            {
                camera.localRotation = m_CameraTargetRot;
            }
        }

    }
}
