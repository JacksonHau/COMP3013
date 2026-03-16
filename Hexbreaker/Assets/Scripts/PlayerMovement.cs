using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public Rigidbody2D rb;

    // Fixed Update is called 50x frame
    void FixedUpdate()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        rb.linearVelocity = new Vector2(horizontal, vertical) * speed;
    }
}
