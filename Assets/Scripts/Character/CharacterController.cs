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
    [SerializeField] private Transform cameraReference;
    [SerializeField] private float walkBobSpeed = 14f;
    [SerializeField] private float walkBobAmount = 0.05f;
    [SerializeField] private float sprintBobSpeed = 18f;
    [SerializeField] private float sprintBobAmount = 0.1f;

    [Header("Footstep Settings")]
    [SerializeField] private AudioClip[] walkFootstepClips;
    [SerializeField] private AudioClip[] sprintFootstepClips;
    [SerializeField] private float walkFootstepInterval = 0.5f;
    [SerializeField] private float sprintFootstepInterval = 0.3f;

    // Private variables
    private CharacterController controller;
    private Camera playerCamera;
    private AudioSource audioSource;
    private float xRotation = 0f;
    private Vector3 velocity;
    private bool isGrounded;
    private Vector3 defaultCameraLocalPos;
    private float bobTimer;
    private float footstepTimer;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
        }

        if (cameraReference != null)
        {
            defaultCameraLocalPos = cameraReference.localPosition;
        }

        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void Update()
    {
        // Sadece UI açýk DEÐÝLSE hareket ve kamera kontrolünü çalýþtýr.
        if (CanvasControl.IsUiOpen == false)
        {
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            HandleGroundCheck();
            HandleMovement();
            HandleMouseLook();
            HandleHeadBobbingAndFootsteps();
            HandleInteraction();
        }
    }

    private void HandleGroundCheck()
    {
        if (groundCheck == null)
        {
            isGrounded = false;
            return;
        }
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
    }

    private void HandleMovement()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;
        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed;
        controller.Move(move * currentSpeed * Time.deltaTime);
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);

        // DÜZELTÝLEN SATIR: 'xrotation' -> 'xRotation' yapýldý.
        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);
    }

    private void HandleHeadBobbingAndFootsteps()
    {
        if (playerCamera == null || cameraReference == null) return;

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        bool isMoving = new Vector3(x, 0f, z).magnitude > 0.1f && isGrounded;
        bool isSprinting = Input.GetKey(KeyCode.LeftShift);

        if (isMoving)
        {
            float bobSpeed = isSprinting ? sprintBobSpeed : walkBobSpeed;
            float bobAmount = isSprinting ? sprintBobAmount : walkBobAmount;
            bobTimer += Time.deltaTime * bobSpeed;
            float newY = defaultCameraLocalPos.y + Mathf.Sin(bobTimer) * bobAmount;
            float newX = Mathf.Cos(bobTimer * 0.5f) * bobAmount * 0.5f;
            playerCamera.transform.localPosition = new Vector3(defaultCameraLocalPos.x + newX, newY, defaultCameraLocalPos.z);

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
            bobTimer = 0f;
            footstepTimer = 0f;
            playerCamera.transform.localPosition = Vector3.Lerp(playerCamera.transform.localPosition, defaultCameraLocalPos, Time.deltaTime * 5f);
        }
    }

    private void PlayFootstepSound(bool isSprinting)
    {
        if (audioSource == null) return;
        AudioClip[] footstepClips = isSprinting ? sprintFootstepClips : walkFootstepClips;
        if (footstepClips == null || footstepClips.Length == 0) return;
        AudioClip clip = footstepClips[UnityEngine.Random.Range(0, footstepClips.Length)];
        audioSource.PlayOneShot(clip);
    }

    private void HandleInteraction()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            if (Physics.Raycast(ray, out RaycastHit hit, interactionRange, interactionMask))
            {
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                interactable?.Interact();
            }
        }
    }

    public void SetCursorLock(bool lockCursor)
    {
        Cursor.lockState = lockCursor ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !lockCursor;
    }
}

public interface IInteractable
{
    void Interact();
}