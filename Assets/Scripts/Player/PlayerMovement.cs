using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3f;
    private bool canMove = true;

    private float horizontalInput;
    private float verticalInput;

    void Update()
    {
        if (!canMove)
        {
            horizontalInput = 0f;
            verticalInput = 0f;
            return;
        }

        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
    }

    void FixedUpdate()
    {
        Vector3 movement = new(horizontalInput, verticalInput, 0f);
        transform.Translate(moveSpeed * Time.fixedDeltaTime * movement);
    }

    public void SetMovementEnabled(bool enabled)
    {
        canMove = enabled;
    }
}
