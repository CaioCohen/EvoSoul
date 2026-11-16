using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(CharacterController))]
public sealed class VirginDropPlayerController : MonoBehaviour
{
    [SerializeField] private Transform visualRoot;
    [SerializeField] private Animator characterAnimator;
    [SerializeField] private AnimationClip walkClip;
    [SerializeField] private AnimationClip attackClip;
    [SerializeField] private float moveSpeed = 3.4f;
    [SerializeField] private float turnSpeed = 720f;
    [SerializeField] private float gravity = -20f;

    private CharacterController controller;
    private float walkTime;
    private float attackTime = -1f;
    private float verticalVelocity;
    private Vector2 touchMoveInput;
    private bool touchAttackQueued;

    public void Configure(Transform model, Animator animator, AnimationClip walk, AnimationClip attack)
    {
        visualRoot = model;
        characterAnimator = animator;
        walkClip = walk;
        attackClip = attack;
    }

    // Called by the mobile overlay. Keeping this on the player avoids a
    // separate input path for the same locomotion and animation state.
    public void SetTouchMoveInput(Vector2 input)
    {
        touchMoveInput = Vector2.ClampMagnitude(input, 1f);
    }

    public void RequestAttack()
    {
        touchAttackQueued = true;
    }

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        controller.height = 1.7f;
        controller.radius = 0.35f;
        controller.center = new Vector3(0f, 0.85f, 0f);
        // The visible stage ground has its own MeshCollider.  The default
        // CharacterController skin (0.08m) left this small creature visibly
        // floating above that surface; a mobile-safe 2cm skin keeps collision
        // stable while letting its base read as touching the terrain.
        controller.skinWidth = 0.02f;

        if (characterAnimator == null)
            characterAnimator = GetComponentInChildren<Animator>();
        if (visualRoot == null && characterAnimator != null)
            visualRoot = characterAnimator.transform;

        if (characterAnimator != null)
        {
            characterAnimator.applyRootMotion = false;
            characterAnimator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        }
    }

    private void Update()
    {
        Vector2 input = ReadMovement();
        bool attackPressed = ReadAttackPressed();
        if (attackPressed && attackTime < 0f && attackClip != null)
        {
            attackTime = 0f;
        }

        bool attacking = attackTime >= 0f;

        Vector3 cameraForward = Camera.main != null ? Camera.main.transform.forward : Vector3.forward;
        cameraForward.y = 0f;
        cameraForward.Normalize();
        Vector3 cameraRight = new Vector3(cameraForward.z, 0f, -cameraForward.x);
        Vector3 move = cameraForward * input.y + cameraRight * input.x;
        if (move.sqrMagnitude > 1f)
            move.Normalize();

        if (!attacking && move.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(move), turnSpeed * Time.deltaTime);
        }

        verticalVelocity = controller.isGrounded && verticalVelocity < 0f ? -2f : verticalVelocity + gravity * Time.deltaTime;
        controller.Move((move * moveSpeed + Vector3.up * verticalVelocity) * Time.deltaTime);

        if (attacking && attackClip != null && visualRoot != null)
        {
            SampleVisualClip(attackClip, attackTime);
            attackTime += Time.deltaTime;
            if (attackTime >= attackClip.length)
            {
                SampleVisualClip(attackClip, 0f);
                attackTime = -1f;
            }
        }
        else if (walkClip != null && visualRoot != null)
        {
            if (move.sqrMagnitude > 0.001f)
                walkTime = Mathf.Repeat(walkTime + Time.deltaTime, walkClip.length);
            else
                walkTime = 0f;
            SampleVisualClip(walkClip, walkTime);
        }
    }

    /// <summary>
    /// The imported Virgin Drop clips contain transform curves. In the stage
    /// scene the visual currently lives on this same root GameObject, so a raw
    /// SampleAnimation call would restore the player to the clip's origin every
    /// frame after CharacterController.Move had already moved it. Preserve the
    /// controller-owned root transform while keeping the animated mesh pose.
    /// </summary>
    private void SampleVisualClip(AnimationClip clip, float time)
    {
        if (clip == null || visualRoot == null)
            return;

        bool visualIsMovementRoot = visualRoot == transform;
        Vector3 worldPosition = transform.position;
        Quaternion worldRotation = transform.rotation;
        clip.SampleAnimation(visualRoot.gameObject, time);
        if (visualIsMovementRoot)
        {
            transform.SetPositionAndRotation(worldPosition, worldRotation);
        }
    }

    private Vector2 ReadMovement()
    {
        Vector2 keyboardInput = Vector2.zero;
#if ENABLE_INPUT_SYSTEM
        var keyboard = Keyboard.current;
        if (keyboard != null)
            keyboardInput = new Vector2((keyboard.dKey.isPressed ? 1f : 0f) - (keyboard.aKey.isPressed ? 1f : 0f),
                (keyboard.wKey.isPressed ? 1f : 0f) - (keyboard.sKey.isPressed ? 1f : 0f));
#elif ENABLE_LEGACY_INPUT_MANAGER
        keyboardInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
#endif
        return touchMoveInput.sqrMagnitude > keyboardInput.sqrMagnitude ? touchMoveInput : keyboardInput;
    }

    private bool ReadAttackPressed()
    {
        bool attackPressed = touchAttackQueued;
        touchAttackQueued = false;
#if ENABLE_INPUT_SYSTEM
        attackPressed |= (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) ||
            (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame);
#elif ENABLE_LEGACY_INPUT_MANAGER
        attackPressed |= Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0);
#endif
        return attackPressed;
    }

}

