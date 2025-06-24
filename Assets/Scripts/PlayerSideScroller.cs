using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class PlayerPlatformer : MonoBehaviour
{
    [Header("이동/점프 설정")]
    public float moveSpeed = 5f;
    public float jumpForce = 7f;

    [Header("스프라이트")]
    [SerializeField] private Sprite frontSprite;
    [SerializeField] private Sprite leftSprite;
    [SerializeField] private Sprite rightSprite;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;

    [Header("점프 판정 설정")]
    [SerializeField] private LayerMask groundLayer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    void Update()
    {
        // 좌우 이동 입력
        float inputX = Input.GetAxisRaw("Horizontal");
        Vector2 velocity = rb.velocity;
        velocity.x = inputX * moveSpeed;
        rb.velocity = velocity;

        // 방향에 따라 스프라이트 변경
        if (inputX < 0)
            spriteRenderer.sprite = leftSprite;
        else if (inputX > 0)
            spriteRenderer.sprite = rightSprite;
        else
            spriteRenderer.sprite = frontSprite;

        // 점프 입력 (지면일 때만)
        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }
    }

    // 바닥 체크: 박스콜라이더 하단에서 짧은 Ray를 쏴서 groundLayer와 닿는지 확인
    private bool IsGrounded()
    {
        Vector2 origin = boxCollider.bounds.center;
        Vector2 size = boxCollider.bounds.size;
        Vector2 bottom = new Vector2(origin.x, origin.y - size.y / 2f);
        RaycastHit2D hit = Physics2D.BoxCast(bottom, new Vector2(size.x * 0.9f, 0.1f), 0f, Vector2.down, 0.05f, groundLayer);
        return hit.collider != null;
    }
}
