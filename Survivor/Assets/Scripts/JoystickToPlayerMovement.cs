using UnityEngine;
using UnityEngine.InputSystem;

public class JoystickToPlayerMovement : MonoBehaviour
{
    [SerializeField] private PlayerMovement targetPlayerMovement;
    [SerializeField] private RectTransform joystickRoot;
    [SerializeField] private RectTransform stickHandle;
    [SerializeField] private Canvas rootCanvas;
    [SerializeField] private float deadzone = 0.05f;
    [SerializeField] private float joystickRadius = 90f;
    [SerializeField] private CanvasGroup joystickCanvasGroup;
    [SerializeField, Range(0f, 1f)] private float hiddenAlpha = 0f;
    [SerializeField, Range(0f, 1f)] private float visibleAlpha = 1f;
    [SerializeField] private bool disableBuiltInVirtualJoystick = true;

    private const int MousePointerId = -999;

    private bool isVisible;
    private bool pointerActive;
    private int activePointerId = -1;
    private Vector2 startScreenPosition;

    private void Reset()
    {
        targetPlayerMovement = FindAnyObjectByType<PlayerMovement>();
        joystickRoot = transform as RectTransform;
        rootCanvas = GetComponentInParent<Canvas>();
        joystickCanvasGroup = GetComponent<CanvasGroup>();
    }

    private void Awake()
    {
        if (joystickRoot == null)
        {
            joystickRoot = transform as RectTransform;
        }

        if (rootCanvas == null)
        {
            rootCanvas = GetComponentInParent<Canvas>();
        }

        if (stickHandle == null)
        {
            Terresquall.VirtualJoystick builtInJoystick = GetComponent<Terresquall.VirtualJoystick>();
            if (builtInJoystick != null && builtInJoystick.controlStick != null)
            {
                stickHandle = builtInJoystick.controlStick.rectTransform;
            }

            if (disableBuiltInVirtualJoystick && builtInJoystick != null)
            {
                builtInJoystick.enabled = false;
            }
        }

        if (joystickCanvasGroup == null)
        {
            joystickCanvasGroup = GetComponent<CanvasGroup>();
        }

        if (joystickCanvasGroup == null)
        {
            joystickCanvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        SetJoystickVisible(false, true);
    }

    private void Update()
    {
        if (targetPlayerMovement == null)
        {
            return;
        }

        if (!pointerActive)
        {
            if (TryGetPointerDown(out Vector2 downPosition, out int pointerId))
            {
                BeginPointer(downPosition, pointerId);
            }
            else
            {
                targetPlayerMovement.SetMoveInput(Vector2.zero);
            }

            return;
        }

        if (TryGetPointerUp(activePointerId))
        {
            EndPointer();
            return;
        }

        if (TryGetPointerPosition(activePointerId, out Vector2 currentPosition))
        {
            Vector2 delta = currentPosition - startScreenPosition;
            Vector2 clampedDelta = Vector2.ClampMagnitude(delta, joystickRadius);
            Vector2 normalizedInput = joystickRadius > 0f ? clampedDelta / joystickRadius : Vector2.zero;

            if (normalizedInput.sqrMagnitude < deadzone * deadzone)
            {
                normalizedInput = Vector2.zero;
            }

            if (stickHandle != null)
            {
                stickHandle.anchoredPosition = clampedDelta;
            }

            targetPlayerMovement.SetMoveInput(normalizedInput);
        }
        else
        {
            EndPointer();
        }
    }

    private void OnDisable()
    {
        EndPointer();
    }

    private void SetJoystickVisible(bool visible, bool force)
    {
        if (!force && visible == isVisible)
        {
            return;
        }

        isVisible = visible;
        if (joystickCanvasGroup != null)
        {
            joystickCanvasGroup.alpha = visible ? visibleAlpha : hiddenAlpha;
        }
    }

    private void BeginPointer(Vector2 screenPosition, int pointerId)
    {
        pointerActive = true;
        activePointerId = pointerId;
        startScreenPosition = screenPosition;

        SetJoystickScreenPosition(screenPosition);
        if (stickHandle != null)
        {
            stickHandle.anchoredPosition = Vector2.zero;
        }

        SetJoystickVisible(true, false);
        targetPlayerMovement.SetMoveInput(Vector2.zero);
    }

    private void EndPointer()
    {
        pointerActive = false;
        activePointerId = -1;

        if (stickHandle != null)
        {
            stickHandle.anchoredPosition = Vector2.zero;
        }

        if (targetPlayerMovement != null)
        {
            targetPlayerMovement.SetMoveInput(Vector2.zero);
        }

        SetJoystickVisible(false, true);
    }

    private void SetJoystickScreenPosition(Vector2 screenPosition)
    {
        if (joystickRoot == null)
        {
            return;
        }

        RectTransform parentRect = joystickRoot.parent as RectTransform;
        if (parentRect == null)
        {
            joystickRoot.position = screenPosition;
            return;
        }

        Camera eventCamera = null;
        if (rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            eventCamera = rootCanvas.worldCamera;
        }

        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(parentRect, screenPosition, eventCamera, out Vector3 worldPoint))
        {
            joystickRoot.position = worldPoint;
        }
    }

    private bool TryGetPointerDown(out Vector2 position, out int pointerId)
    {
        if (Touchscreen.current != null)
        {
            var touches = Touchscreen.current.touches;
            for (int i = 0; i < touches.Count; i++)
            {
                var touch = touches[i];
                if (!touch.press.isPressed)
                {
                    continue;
                }

                var phase = touch.phase.ReadValue();
                if (phase == UnityEngine.InputSystem.TouchPhase.Began)
                {
                    position = touch.position.ReadValue();
                    pointerId = touch.touchId.ReadValue();
                    return true;
                }
            }
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            position = Mouse.current.position.ReadValue();
            pointerId = MousePointerId;
            return true;
        }

        position = Vector2.zero;
        pointerId = -1;
        return false;
    }

    private bool TryGetPointerPosition(int pointerId, out Vector2 position)
    {
        if (pointerId == MousePointerId)
        {
            if (Mouse.current != null && Mouse.current.leftButton.isPressed)
            {
                position = Mouse.current.position.ReadValue();
                return true;
            }

            position = Vector2.zero;
            return false;
        }

        if (Touchscreen.current != null)
        {
            var touches = Touchscreen.current.touches;
            for (int i = 0; i < touches.Count; i++)
            {
                var touch = touches[i];
                if (touch.touchId.ReadValue() == pointerId)
                {
                    var phase = touch.phase.ReadValue();
                    if (phase == UnityEngine.InputSystem.TouchPhase.Ended || phase == UnityEngine.InputSystem.TouchPhase.Canceled)
                    {
                        position = Vector2.zero;
                        return false;
                    }

                    position = touch.position.ReadValue();
                    return true;
                }
            }
        }

        position = Vector2.zero;
        return false;
    }

    private bool TryGetPointerUp(int pointerId)
    {
        if (pointerId == MousePointerId)
        {
            return Mouse.current == null || Mouse.current.leftButton.wasReleasedThisFrame;
        }

        if (Touchscreen.current != null)
        {
            var touches = Touchscreen.current.touches;
            for (int i = 0; i < touches.Count; i++)
            {
                var touch = touches[i];
                if (touch.touchId.ReadValue() == pointerId)
                {
                    var phase = touch.phase.ReadValue();
                    return phase == UnityEngine.InputSystem.TouchPhase.Ended || phase == UnityEngine.InputSystem.TouchPhase.Canceled;
                }
            }
        }

        return true;
    }
}
