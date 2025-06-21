using UnityEngine;

public class SwitchController : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Sprite offSprite;
    public Sprite onSprite;

    private bool isOn = false;
    private Transform player;
    public float interactDistance = 2f;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        UpdateSwitchVisual();

        // SwitchManager에 등록
        SwitchManager.Instance?.RegisterSwitch(this);
    }

    private void Update()
    {
        if (player == null) return;

        // 플레이어와 거리 체크
        if (Vector3.Distance(transform.position, player.position) > interactDistance)
            return;

        // 마우스 왼쪽 클릭 감지
        if (Input.GetMouseButtonDown(0))
        {
            // 카메라에서 마우스 클릭 위치로 레이캐스트 쏴서 내 오브젝트인지 체크
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                ToggleSwitch();
            }
        }
    }

    private void ToggleSwitch()
    {
        if (isOn) return;

        isOn = true;
        UpdateSwitchVisual();

        SwitchManager.Instance?.CheckAllSwitches();
    }

    private void UpdateSwitchVisual()
    {
        if (spriteRenderer != null)
            spriteRenderer.sprite = isOn ? onSprite : offSprite;
    }

    public bool IsOn()
    {
        return isOn;
    }
}
