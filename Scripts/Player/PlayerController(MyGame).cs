using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Player Moving")]
    public float moveSpeed;
    public float gravityModifer;
    public float runSpeed;
    public float jumpPower;
    public CharacterController charCon;
    private bool canJump;
    private bool manyJump;

    public Transform groundCheckPoint;
    public LayerMask whatIsGround;
    private Vector3 moveInput;

    [Header("Camera Controll")]
    [SerializeField] private bool goToMousePosition;
    private Camera mainCamera;
    public float airSpeedControll;
    [SerializeField] private bool ignoreHeight;
    [SerializeField] private Transform aimedTransform;

    [Header("Weapon Holder")]
    [SerializeField] private WeaponSwitching weaponSwitching;

    [Header("Gizmos")]
    [SerializeField] private bool gizmo_cameraRay = false;
    [SerializeField] private bool gizmo_ground = false;
    [SerializeField] private bool gizmo_target = false;
    [SerializeField] private bool gizmo_ignoredHeightTarget = false;


    private void Awake()
    {
   
    }
    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        HandleMovementInput();

        if (!goToMousePosition)
            Aim();
        else HandleRotationInput();
        
    }
    private void HandleMovementInput()
    {

        float yStore = moveInput.y;

        Vector3 vertMove = transform.forward * Input.GetAxis("Vertical");
        Vector3 horiMove = transform.right * Input.GetAxis("Horizontal");

        moveInput = horiMove + vertMove;

        if (moveInput.magnitude > 1f)
        {
            moveInput.Normalize();
        }

        if (Input.GetAxis("Vertical") == 0 && Input.GetAxis("Horizontal") == 0)
        {
            moveInput.x = 0f;
            moveInput.z = 0f;
        }

        #region Running
        if (Input.GetKey(KeyCode.LeftShift))
        {
            moveInput = moveInput * runSpeed;
        }
        else
        {
            moveInput = moveInput * moveSpeed;
        }
        moveInput.y = yStore;
        #endregion

        #region Gravity

        Gravity();

        if (!canJump)
            Fly();

        #endregion

        #region Jump
        Jump();

        #endregion

        charCon.Move(moveInput * Time.deltaTime);

    }

    private void Gravity()
    {
        if (charCon.isGrounded)
        {
            moveInput.y = Physics.gravity.y * gravityModifer * Time.deltaTime;
        }
        else
        {
            moveInput.y += Physics.gravity.y * gravityModifer * Time.deltaTime;
        }
    }

    private void Jump()
    {
        canJump = Physics.OverlapSphere(groundCheckPoint.position, .25f, whatIsGround).Length > 0;

        if (canJump)
        {
            manyJump = false;
        }


        //Handle Jumping
        if (Input.GetKeyDown(KeyCode.Space) && canJump)
        {
            moveInput.y = jumpPower;

            manyJump = true;
        }


        if (manyJump && Input.GetKeyDown(KeyCode.Space))
        {
            moveInput.y = jumpPower;

            manyJump = false;
        }
    }

    private void HandleRotationInput()
    {
        RaycastHit _hit;
        Ray _ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(_ray, out _hit))
        {
            transform.LookAt(new Vector3(_hit.point.x, transform.position.y, _hit.point.z));
        }
    }

    private (bool success, Vector3 position) GetMousePosition()
    {
        var ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out var hitInfo, Mathf.Infinity, whatIsGround))
        {
            return (success: true, position: hitInfo.point);
        }
        else
        {
            return (success: false, position: Vector3.zero);
        }
    }

    private void Fly()      //TODO!!! in state!
    {
        {
            moveInput.x *= airSpeedControll;
            moveInput.z *= airSpeedControll;
        }
    }

    private void Aim()
    {

        var (success, position) = GetMousePosition();
        if (success)
        {
            // Direction is usually normalized, 
            // but it does not matter in this case.
            var direction = position - aimedTransform.position;

            if (ignoreHeight)
            {
                // Ignore the height difference.
                direction.y = 0;
            }

            // Make the transform look at the mouse position.
            aimedTransform.forward = direction;
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheckPoint.position, 0.25f);

        #region Gimos for checking position of mouse AFTER Delete
        if (Application.isPlaying == false) return;

        var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hitInfo, float.MaxValue, whatIsGround))
        {
            if (gizmo_cameraRay)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(ray.origin, hitInfo.point);
                Gizmos.DrawWireSphere(ray.origin, 0.5f);
            }

            var hitPosition = hitInfo.point;
            var hitGroundHeight = Vector3.Scale(hitInfo.point, new Vector3(1, 0, 1)); ;
            var hitPositionIngoredHeight = new Vector3(hitInfo.point.x, aimedTransform.position.y, hitInfo.point.z);

            if (gizmo_ground)
            {
                Gizmos.color = Color.gray;
                Gizmos.DrawWireSphere(hitGroundHeight, 0.5f);
                Gizmos.DrawLine(hitGroundHeight, hitPosition);
            }

            if (gizmo_target)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(hitInfo.point, 0.5f);
                Gizmos.DrawLine(aimedTransform.position, hitPosition);
            }

            if (gizmo_ignoredHeightTarget)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(hitPositionIngoredHeight, 0.5f);
                Gizmos.DrawLine(aimedTransform.position, hitPositionIngoredHeight);
            }
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheckPoint.position, 0.25f);
        #endregion 
    }
    private void GizmoSettings()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            gizmo_cameraRay = !gizmo_cameraRay;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            gizmo_ground = !gizmo_ground;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            gizmo_target = !gizmo_target;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            gizmo_ignoredHeightTarget = !gizmo_ignoredHeightTarget;
        }
    }
}
