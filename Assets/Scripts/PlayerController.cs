using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerController : MonoBehaviour
{
    [SerializeField] float _moveSpeed = 10f;
    [SerializeField] float _jumpPower = 500f;
    [SerializeField] bool _canClimb = false;
    [SerializeField] GameObject _bulletPrefab;
    [SerializeField] Transform _bulletSpawnLocation;

    private PlayerAnimations _anims;

    private PlayerControl playerControl = null;
    private Rigidbody2D rb = null;
    private new BoxCollider2D collider2D;
    private float moveAxisX = 0f;
    private float moveAxisY = 0f;
    private bool isRunning;
    private bool isFlipped;
    private bool doubleJump;
    private void Awake()
    {
        playerControl = new PlayerControl();
        _anims = GetComponentInChildren<PlayerAnimations>();
        collider2D = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();
    }
    private void OnEnable()
    {
        playerControl.Enable();
        playerControl.Player.Movement.performed += OnMovementPerformed;
        playerControl.Player.Movement.canceled += OnMovementCanceled;
        playerControl.Player.Jump.performed += OnJumpPerformed;
        playerControl.Player.Climb.performed += OnClimbPerformed;
        playerControl.Player.Climb.canceled += OnClimbCanceled;
        playerControl.Player.Shoot.performed += OnShootPerformed;
    }
    private void OnDisable()
    {
        playerControl.Disable();
        playerControl.Player.Movement.performed -= OnMovementPerformed;
        playerControl.Player.Movement.canceled -= OnMovementCanceled;
        playerControl.Player.Jump.performed -= OnJumpPerformed;
        playerControl.Player.Climb.performed -= OnClimbPerformed;
        playerControl.Player.Climb.canceled -= OnClimbCanceled;
        playerControl.Player.Shoot.performed -= OnShootPerformed;
    }
    private void FixedUpdate()
    {
        if (_canClimb)
        {
            rb.velocity = new Vector2(moveAxisX * _moveSpeed * Time.fixedDeltaTime / 2, moveAxisY * _moveSpeed * Time.fixedDeltaTime / 2);
        }
        else
        {
            rb.velocity = new Vector2(moveAxisX * _moveSpeed * Time.fixedDeltaTime, rb.velocity.y);
        }
        if (isFlipped)
        {
            transform.localScale = new Vector2(-1, 1);
        }
        else
        {
            transform.localScale = new Vector2(1, 1);
        }
    }
    private void OnMovementPerformed(InputAction.CallbackContext value)
    {
        moveAxisX = value.ReadValue<float>();
        if (moveAxisX < 0)
        {
            isFlipped = true;
        }
        else if (moveAxisX > 0)
        {
            isFlipped = false;
        }
        isRunning = true;
        _anims.Run(true);
    }
    private void OnMovementCanceled(InputAction.CallbackContext value)
    {
        moveAxisX = 0f;
        isRunning = false;
        _anims.Run(false);
    }
    private void OnJumpPerformed(InputAction.CallbackContext value)
    {
        if (IsGrounded() && !_canClimb && !doubleJump)
        {
            rb.AddForce(Vector2.up * _jumpPower);
            doubleJump = true;
            _anims.Jump();
        }
        else if (!IsGrounded() && !_canClimb && doubleJump)
        {
            rb.AddForce(Vector2.up * _jumpPower);
            doubleJump = false;
            _anims.DoubleJump();
        }
    }
    private bool IsGrounded()
    {
        if (collider2D.IsTouchingLayers(LayerMask.GetMask("Ground")))
        {
            doubleJump = false;
            return true;
        }
        return false;
    }
    private void OnClimbPerformed(InputAction.CallbackContext value)
    {
        moveAxisY = value.ReadValue<float>();
        _anims.ClimbDirection(moveAxisY);
    }
    private void OnClimbCanceled(InputAction.CallbackContext value)
    {
        moveAxisY = 0;
        _anims.ClimbDirection(moveAxisY);
    }
    private void OnShootPerformed(InputAction.CallbackContext value)
    {
        if (!IsGrounded())
        {
            return;
        }


        if (isRunning)
        {
            _anims.AttackRunning();
        }
        else
        {
            _anims.AttackIdle();
        }
    }
    public void SpawnProjectile()
    {
        GameObject bullet = BulletPool.Instance.GetBullet();
        bullet.transform.parent = FindAnyObjectByType<BulletPool>().transform;
        bullet.transform.SetPositionAndRotation(_bulletSpawnLocation.position, Quaternion.identity);
        bullet.SetActive(true);

        if (isFlipped)
        {
            bullet.GetComponent<Rigidbody2D>().velocity = Vector2.left * _moveSpeed / 25;
        }
        else
        {
            bullet.GetComponent<Rigidbody2D>().velocity = -Vector2.left * _moveSpeed / 25;
        }

    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ladder"))
        {
            _canClimb = true;
            _anims.OnLadder(true);
            rb.gravityScale = 0;
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Ladder"))
        {
            _canClimb = false;
            _anims.OnLadder(false);
            rb.gravityScale = 1;
        }
    }
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.layer == 3)
        {
            _anims.LandJump();
        }
    }
}