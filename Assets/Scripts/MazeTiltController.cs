using UnityEngine;
using UnityEngine.InputSystem;

namespace MarbleMazePan3D
{
    /// <summary>
    /// Tilts the maze board like a physical marble labyrinth pan.
    /// Supports mouse drag, keyboard, and device accelerometer.
    /// </summary>
    public class MazeTiltController : MonoBehaviour
    {
        [Header("Tilt Limits")]
        [SerializeField] private float maxTiltAngle = 18f;
        [SerializeField] private float tiltSmoothing = 8f;

        [Header("Input Sensitivity")]
        [SerializeField] private float mouseSensitivity = 0.35f;
        [SerializeField] private float keyboardTiltSpeed = 45f;
        [SerializeField] private float accelerometerSensitivity = 2.5f;

        [Header("Input Mode")]
        [SerializeField] private bool useAccelerometerOnMobile = true;

        private Vector2 _targetTilt;
        private Vector2 _currentTilt;
        private bool _dragging;
        private Vector2 _lastPointerPosition;

        private void Update()
        {
            ReadInput();
            _currentTilt = Vector2.Lerp(
                _currentTilt,
                _targetTilt,
                1f - Mathf.Exp(-tiltSmoothing * Time.deltaTime));

            transform.localRotation = Quaternion.Euler(-_currentTilt.y, 0f, _currentTilt.x);
        }

        private void ReadInput()
        {
            if (TryReadAccelerometer())
            {
                return;
            }

            if (TryReadMouseDrag())
            {
                return;
            }

            ReadKeyboard();
        }

        private bool TryReadAccelerometer()
        {
#if UNITY_ANDROID || UNITY_IOS
            if (!useAccelerometerOnMobile)
            {
                return false;
            }

            Vector3 accel = Input.acceleration;
            _targetTilt = new Vector2(
                Mathf.Clamp(accel.x * accelerometerSensitivity * maxTiltAngle, -maxTiltAngle, maxTiltAngle),
                Mathf.Clamp(accel.y * accelerometerSensitivity * maxTiltAngle, -maxTiltAngle, maxTiltAngle));
            return true;
#else
            return false;
#endif
        }

        private bool TryReadMouseDrag()
        {
            if (Mouse.current == null)
            {
                return false;
            }

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                _dragging = true;
                _lastPointerPosition = Mouse.current.position.ReadValue();
                return true;
            }

            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                _dragging = false;
            }

            if (_dragging && Mouse.current.leftButton.isPressed)
            {
                Vector2 pointer = Mouse.current.position.ReadValue();
                Vector2 delta = pointer - _lastPointerPosition;
                _lastPointerPosition = pointer;

                _targetTilt += new Vector2(delta.x, -delta.y) * mouseSensitivity;
                _targetTilt = ClampTilt(_targetTilt);
                return true;
            }

            return false;
        }

        private void ReadKeyboard()
        {
            Vector2 input = Vector2.zero;

            if (Keyboard.current == null)
            {
                return;
            }

            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            {
                input.x -= 1f;
            }

            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            {
                input.x += 1f;
            }

            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
            {
                input.y -= 1f;
            }

            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
            {
                input.y += 1f;
            }

            if (input.sqrMagnitude > 0f)
            {
                input = input.normalized;
                _targetTilt += input * keyboardTiltSpeed * Time.deltaTime;
                _targetTilt = ClampTilt(_targetTilt);
            }
        }

        private Vector2 ClampTilt(Vector2 tilt)
        {
            return new Vector2(
                Mathf.Clamp(tilt.x, -maxTiltAngle, maxTiltAngle),
                Mathf.Clamp(tilt.y, -maxTiltAngle, maxTiltAngle));
        }

        public void ResetTilt()
        {
            _targetTilt = Vector2.zero;
            _currentTilt = Vector2.zero;
            transform.localRotation = Quaternion.identity;
        }
    }
}
