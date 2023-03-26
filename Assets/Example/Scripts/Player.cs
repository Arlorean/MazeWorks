using DG.Tweening;
using MazeWorks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] bool animateMovement = true;
    [SerializeField] float moveSpeed = 15f; // Units per second
    [SerializeField] float rotateSpeed = 90f; // Degrees per second

    [Header("Free Look")]
    [SerializeField] bool allowFreeLook = true;
    [SerializeField] float resetLookSpeed = 90f; // Degrees per second
    [SerializeField] float lookPixelsPer90Degrees = 25f;
    [SerializeField] float maxLookAngle = 80f;
    [SerializeField] Ease resetLookEase = Ease.InOutCubic;

    Queue<KeyCode> keyQueue = new Queue<KeyCode>();
    bool moveInProgress;
    bool lookInProgress;

    Dictionary<KeyCode, Action> keyActions = new Dictionary<KeyCode, Action>();

    void Awake() {
        keyActions[KeyCode.Q] = () => Rotate(RelativeDirection.Left);
        keyActions[KeyCode.E] = () => Rotate(RelativeDirection.Right);

        keyActions[KeyCode.W] = () => Translate(RelativeDirection.Forward);
        keyActions[KeyCode.S] = () => Translate(RelativeDirection.Back);
        keyActions[KeyCode.D] = () => Translate(RelativeDirection.Right);
        keyActions[KeyCode.A] = () => Translate(RelativeDirection.Left);

        keyActions[KeyCode.UpArrow] = () => Translate(RelativeDirection.Forward);
        keyActions[KeyCode.DownArrow] = () => Translate(RelativeDirection.Back);
        keyActions[KeyCode.LeftArrow] = () => Rotate(RelativeDirection.Left);
        keyActions[KeyCode.RightArrow] = () => Rotate(RelativeDirection.Right);
    }

    void OnEnable() {
        // Cancel any previous actions
        moveInProgress = false;
        lookInProgress = false;

        // Start in the cell closest to where the player is currently
        var startCell = Util.FindClosestCell(transform.position);
        if (!startCell) {
            Debug.LogWarning("Cannot find the any start cell for the player");
            return;
        }

        // Snap player to starting cell position and rotation
        transform.parent = startCell.transform;
        transform.localPosition = Vector3.zero;
        transform.localEulerAngles = Util.SnapToNearest90Degrees(transform.localEulerAngles);

    }

    public MazeCell CurrentCell => GetComponentInParent<MazeCell>();

    void Update()
    {
        foreach (var key in keyActions.Keys) {
            if (Input.GetKeyDown(key)) {
                keyQueue.Enqueue(key);
            }
            else
            if (Input.GetKey(key) && !moveInProgress && keyQueue.Count == 0) {
                keyQueue.Enqueue(key);
            }
        }

        if (!moveInProgress && keyQueue.Count > 0) {
            var key = keyQueue.Dequeue();
            keyActions[key]();
        }

        if (allowFreeLook && !lookInProgress && Input.GetMouseButtonDown(MouseButton.Right)) {
            StartFreeLook();
        }
    }

    void StartMove() {
        moveInProgress = true;
    }

    void CompleteMove() {
        moveInProgress = false;
        transform.localPosition = Vector3.zero;
    }

    Direction HorizontalDirection => DirectionExtensions.GetHorizontalDirection(transform.localEulerAngles.y);
    Direction VerticalDirection => DirectionExtensions.GetVerticalDirection(transform.localEulerAngles.x);

    void Rotate(RelativeDirection relativeDirection) {
        if (lookInProgress) { return; }
        var direction = HorizontalDirection.Add(relativeDirection);
        var endAngle = new Vector3(0, direction.ToYaw(), 0);
        if (animateMovement) {
            StartMove();
            transform.DOLocalRotate(endAngle, rotateSpeed)
                .SetSpeedBased(true)
                .OnComplete(CompleteMove);
        }
        else {
            transform.localEulerAngles = endAngle;
        }
    }

    void Translate(RelativeDirection translateDirection) {
        var verticalDirection = VerticalDirection;
        if (verticalDirection != Direction.Forward) { // Up or Down
            if (translateDirection == RelativeDirection.Forward) {
                Translate(verticalDirection);
                return;
            }
            if (translateDirection == RelativeDirection.Back) {
                Translate(verticalDirection.Reverse());
                return;
            }
        }

        var direction = HorizontalDirection.Add(translateDirection);
        Translate(direction);
    }
    
    void Translate(Direction direction) {
        var nextCell = CurrentCell.GetNextCell(direction);
        if (nextCell && !nextCell.IsBlocked) {
            if (animateMovement) {
                StartMove();
                transform.parent = nextCell.transform;
                transform.DOBlendableLocalMoveBy(-transform.localPosition, moveSpeed)
                    .SetSpeedBased(true)
                    .OnComplete(CompleteMove);
            }
            else {
                transform.localPosition = Vector3.zero;
            }
        }
    }

    void StartFreeLook() {
        lookInProgress = true;
        StartCoroutine(FreeLookLoop());
    }

    IEnumerator FreeLookLoop() {
    RightMouseButtonDown:
        var rotation = transform.localEulerAngles;
        // Keep pitch in range -180 to +180 degrees
        if (rotation.x > 180) {
            rotation.x -= 360;
        }
        rotation.z = 0;

        Cursor.lockState = CursorLockMode.Locked;

        while (Input.GetMouseButton(MouseButton.Right)) {
            var delta = new Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0);
            rotation += delta / (lookPixelsPer90Degrees/90f);

            // Keep the pitch within range
            rotation = new Vector3(Mathf.Clamp(rotation.x, -maxLookAngle, +maxLookAngle), rotation.y, 0);

            transform.localEulerAngles = rotation;

            yield return null;
        }

        Cursor.lockState = CursorLockMode.None;

        if (true) {
            var resetAngle = new Vector3(0, Util.SnapToNearest90Degrees(transform.localEulerAngles.y), 0);
            
            var tween = transform
                .DOLocalRotate(resetAngle, resetLookSpeed)
                .SetSpeedBased()
                .SetEase(resetLookEase);

            // If right mouse button pressed again while resetting camera, resume free look loop
            while (tween.IsPlaying()) {
                if (Input.GetMouseButtonDown(MouseButton.Right)) {
                    tween.Kill();
                    goto RightMouseButtonDown;
                }
                yield return null;
            }
        }

        lookInProgress = false;
    }
}
