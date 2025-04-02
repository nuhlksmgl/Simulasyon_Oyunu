using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpHeight = 2f;

    [Header("Mouse Look Settings")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float maxLookAngle = 80f;
    [SerializeField] private bool lockCursor = true;

    [Header("Ground Check Settings")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.4f;
    [SerializeField] private LayerMask groundMask;

    [Header("Interaction")]
    [SerializeField] private float interactionRange = 2f;
    [SerializeField] private LayerMask interactionMask;

    [Header("Head Bobbing Settings")]
    [SerializeField] private Transform cameraReference; // Kameranýn referans pozisyonu
    [SerializeField] private float walkBobSpeed = 14f; // Yürüme sýrasýndaki sallanma hýzý
    [SerializeField] private float walkBobAmount = 0.05f; // Yürüme sýrasýndaki sallanma miktarý
    [SerializeField] private float sprintBobSpeed = 18f; // Koþma sýrasýndaki sallanma hýzý
    [SerializeField] private float sprintBobAmount = 0.1f; // Koþma sýrasýndaki sallanma miktarý

    [Header("Footstep Settings")]
    [SerializeField] private AudioClip[] walkFootstepClips; // Yürüme ayak sesleri
    [SerializeField] private AudioClip[] sprintFootstepClips; // Koþma ayak sesleri
    [SerializeField] private float walkFootstepInterval = 0.5f; // Yürüme ayak sesi aralýðý
    [SerializeField] private float sprintFootstepInterval = 0.3f; // Koþma ayak sesi aralýðý

    // Private variables
    private CharacterController controller;
    private Camera playerCamera;
    private AudioSource audioSource; // Ayak sesleri için AudioSource
    private float xRotation = 0f;
    private Vector3 velocity;
    private bool isGrounded;
    private Vector3 defaultCameraLocalPos; // Kameranýn varsayýlan local pozisyonu
    private float bobTimer; // Sallanma için zamanlayýcý
    private float footstepTimer; // Ayak sesi için zamanlayýcý

    private void Start()
    {
        // Get component references
        controller = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();
        audioSource = GetComponent<AudioSource>();

        // AudioSource yoksa ekle
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D ses (konumdan baðýmsýz)
        }

        // Kamerayý kontrol et
        if (playerCamera == null)
        {
            Debug.LogError("Player Camera bulunamadý! Lütfen karakterin child’ý olarak bir Camera ekleyin.");
        }
        else
        {
            Debug.Log($"Player Camera bulundu: {playerCamera.name}, Local Position: {playerCamera.transform.localPosition}");
        }

        // Camera Reference kontrolü
        if (cameraReference == null)
        {
            Debug.LogError("Camera Reference atanmamýþ! Lütfen Inspector’da Camera Reference’ý ayarlayýn.");
        }
        else
        {
            // Kameranýn varsayýlan local pozisyonunu referans noktasýndan al
            defaultCameraLocalPos = cameraReference.localPosition;
            Debug.Log($"Camera Reference bulundu: {cameraReference.name}, Local Position: {defaultCameraLocalPos}");
        }

        // Ground Check kontrolü
        if (groundCheck == null)
        {
            Debug.LogError("Ground Check atanmamýþ! Lütfen Inspector’da Ground Check objesini ayarlayýn.");
        }
        else
        {
            Debug.Log($"Ground Check bulundu: {groundCheck.name}, Local Position: {groundCheck.localPosition}, World Position: {groundCheck.position}");
        }

        // Lock cursor for FPS control
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void Update()
    {
        HandleGroundCheck();
        HandleMovement();
        HandleMouseLook();
        HandleHeadBobbingAndFootsteps();
        HandleInteraction();
    }

    private void HandleGroundCheck()
    {
        // Check if player is grounded using a sphere cast
        if (groundCheck == null)
        {
            Debug.LogWarning("Ground Check null, cannot check if grounded!");
            isGrounded = false;
            return;
        }

        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        Debug.Log($"Is Grounded: {isGrounded}, Ground Check Position: {groundCheck.position}, Ground Distance: {groundDistance}, Ground Mask: {LayerMask.LayerToName(groundMask.value)}");

        // Reset vertical velocity when grounded
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
    }

    private void HandleMovement()
    {
        // Get input axes
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // Debug input
        Debug.Log($"Movement Input - Horizontal: {x}, Vertical: {z}");

        // Create movement vector
        Vector3 move = transform.right * x + transform.forward * z;

        // Determine speed based on sprint input
        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed;

        // Apply movement
        controller.Move(move * currentSpeed * Time.deltaTime);

        // Handle jumping
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleMouseLook()
    {
        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Rotate camera vertically
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);
        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Rotate player horizontally
        transform.Rotate(Vector3.up * mouseX);
    }

    private void HandleHeadBobbingAndFootsteps()
    {
        // Kameranýn null olup olmadýðýný kontrol et
        if (playerCamera == null || cameraReference == null)
        {
            Debug.LogWarning("Head Bobbing çalýþmýyor! playerCamera veya cameraReference null.");
            return;
        }

        // Hareket vektörünü al
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 moveInput = new Vector3(x, 0f, z);
        bool isMoving = moveInput.magnitude > 0.1f && isGrounded;
        bool isSprinting = Input.GetKey(KeyCode.LeftShift);

        Debug.Log($"Is Moving: {isMoving}, Move Input: {moveInput}, Is Sprinting: {isSprinting}");

        if (isMoving)
        {
            // Head bobbing
            float bobSpeed = isSprinting ? sprintBobSpeed : walkBobSpeed;
            float bobAmount = isSprinting ? sprintBobAmount : walkBobAmount;

            bobTimer += Time.deltaTime * bobSpeed;
            float newY = defaultCameraLocalPos.y + Mathf.Sin(bobTimer) * bobAmount;
            float newX = Mathf.Cos(bobTimer * 0.5f) * bobAmount * 0.5f; // Hafif sað-sol sallanma

            Debug.Log($"Head Bobbing - Bob Timer: {bobTimer}, New Y: {newY}, New X: {newX}");

            playerCamera.transform.localPosition = new Vector3(
                defaultCameraLocalPos.x + newX,
                newY,
                defaultCameraLocalPos.z
            );

            Debug.Log($"Camera Local Position Updated: {playerCamera.transform.localPosition}");

            // Footsteps
            footstepTimer += Time.deltaTime;
            float footstepInterval = isSprinting ? sprintFootstepInterval : walkFootstepInterval;

            if (footstepTimer >= footstepInterval)
            {
                PlayFootstepSound(isSprinting);
                footstepTimer = 0f;
            }
        }
        else
        {
            // Hareket yoksa kamerayý varsayýlan pozisyona getir
            bobTimer = 0f;
            footstepTimer = 0f;
            playerCamera.transform.localPosition = Vector3.Lerp(
                playerCamera.transform.localPosition,
                defaultCameraLocalPos,
                Time.deltaTime * 5f
            );
            Debug.Log($"Camera Local Position Reset: {playerCamera.transform.localPosition}");
        }
    }

    private void PlayFootstepSound(bool isSprinting)
    {
        if (audioSource == null) return;

        AudioClip[] footstepClips = isSprinting ? sprintFootstepClips : walkFootstepClips;
        if (footstepClips == null || footstepClips.Length == 0) return;

        // Rastgele bir ayak sesi seç
        AudioClip clip = footstepClips[UnityEngine.Random.Range(0, footstepClips.Length)];
        audioSource.PlayOneShot(clip);
    }

    private void HandleInteraction()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            // Raycast for interactive objects
            Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            if (Physics.Raycast(ray, out RaycastHit hit, interactionRange, interactionMask))
            {
                // Get and execute interaction if object has IInteractable interface
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                interactable?.Interact();
            }
        }
    }

    // Enable/disable cursor lock
    public void SetCursorLock(bool lockCursor)
    {
        Cursor.lockState = lockCursor ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !lockCursor;
    }
}

// Interface for interactive objects
public interface IInteractable
{
    void Interact();
}