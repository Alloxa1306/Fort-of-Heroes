using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerMoveComponent))]
public class PlayerInputHandler : NetworkBehaviour
{
    private PlayerMoveComponent playerMove;
    private PlayerWeaponComponent playerWeapon;
    private PlayerComponent playerData;

    private InputAction moveAction;
    private InputAction aimAction;
    private InputAction dashAction;
    private InputAction shootAction;

    [Header("Ссылки на animation")]
    public Animator animator;
    public float turnSpeed = 10f;

    [Tooltip("Эталонная скорость движения, для расчёта коэффициента скорости анимации")]
    public float referenceMoveSpeed = 5f;

    private Camera mainCamera;
    private bool isPointerOverUI;

    void Awake()
    {
        playerMove = GetComponent<PlayerMoveComponent>();
        playerWeapon = GetComponent<PlayerWeaponComponent>();
        playerData = GetComponent<PlayerComponent>();
        mainCamera = Camera.main;

        moveAction = new InputAction("Move", binding: "<Gamepad>/leftStick");
        moveAction.AddCompositeBinding("Dpad")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");

        aimAction = new InputAction("Aim", binding: "<Gamepad>/rightStick");
        dashAction = new InputAction("Dash", binding: "<Keyboard>/space");
        dashAction.AddBinding("<Gamepad>/buttonSouth");

        shootAction = new InputAction("Shoot", binding: "<Mouse>/leftButton");
        shootAction.AddBinding("<Gamepad>/buttonEast");

        dashAction.performed += context =>
        {
            if (!IsOwner) return;
            playerMove.wantsToDash = true;
        };
    }

    private void Start()
    {
        if (playerData != null)
        {
            turnSpeed = playerData.turnSpeed;
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            moveAction?.Enable();
            aimAction?.Enable();
            dashAction?.Enable();
            shootAction?.Enable();
        }
    }

    public override void OnNetworkDespawn()
    {
        moveAction?.Disable();
        aimAction?.Disable();
        dashAction?.Disable();
        shootAction?.Disable();
    }

    void Update()
    {
        if (!IsOwner) return;

        if (EventSystem.current != null)
        {
            isPointerOverUI = EventSystem.current.IsPointerOverGameObject();
        }

        Vector2 aimInput = aimAction.ReadValue<Vector2>();
        bool isAimingWithStick = aimInput.magnitude > 0.15f;

        if (isAimingWithStick)
        {
            if (playerWeapon != null) playerWeapon.wantsToShoot = true;
        }
        else if (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            if (playerWeapon != null) playerWeapon.wantsToShoot = !isPointerOverUI;
        }
        else
        {
            if (playerWeapon != null) playerWeapon.wantsToShoot = false;
        }

        if (playerWeapon != null && animator != null)
        {
            animator.SetBool("IsShooting", playerWeapon.wantsToShoot);
        }

        Vector2 inputVal = moveAction.ReadValue<Vector2>();
        Vector3 moveDirection = new Vector3(inputVal.x, 0, inputVal.y);

        playerMove.direction = moveDirection;

        if (animator != null)
        {
            animator.SetFloat("Speed", inputVal.magnitude);

            float currentActualSpeed = playerMove.moveSpeed * inputVal.magnitude;

            float speedRatio = currentActualSpeed / referenceMoveSpeed;

            animator.SetFloat("AnimSpeed", speedRatio);
        }

        if (playerWeapon != null && playerWeapon.wantsToShoot)
        {
            if (isAimingWithStick)
            {
                Vector3 aimDirection = new Vector3(aimInput.x, 0, aimInput.y);
                if (aimDirection.magnitude > 0.1f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(aimDirection);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
                }
            }
            else
            {
                RotateTowardsMouse();
            }
        }
        else if (inputVal.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }
    }

    private void RotateTowardsMouse()
    {
        if (Mouse.current == null || mainCamera == null) return;

        Vector2 mouseScreenPosition = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mouseScreenPosition);
        Plane groundPlane = new Plane(Vector3.up, transform.position);

        if (groundPlane.Raycast(ray, out float rayDistance))
        {
            Vector3 hitPoint = ray.GetPoint(rayDistance);
            Vector3 lookDirection = hitPoint - transform.position;
            lookDirection.y = 0;

            if (lookDirection.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
            }
        }
    }
}
