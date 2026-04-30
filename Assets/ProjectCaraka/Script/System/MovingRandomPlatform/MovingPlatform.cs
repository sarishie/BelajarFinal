using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Header("Move")]
    public Vector2 moveDirection = Vector2.up;
    public float moveSpeed = 2f;

    [Header("Life Time")]
    public float lifeTime = 10f;

    private float lifeTimer;

    private PlatformSpawner ownerSpawner;
    private int columnIndex = -1;
    private bool hasReleasedColumn;

    private void Start()
    {
        lifeTimer = lifeTime;
    }

    private void Update()
    {
        MovePlatform();
        CountLifeTime();
    }

    public void SetPlatformData(Vector2 direction, float speed, float duration, PlatformSpawner spawner, int column)
    {
        moveDirection = direction.normalized;
        moveSpeed = speed;
        lifeTime = duration;
        lifeTimer = lifeTime;

        ownerSpawner = spawner;
        columnIndex = column;
        hasReleasedColumn = false;
    }

    private void MovePlatform()
    {
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);
    }

    private void CountLifeTime()
    {
        lifeTimer -= Time.deltaTime;

        if (lifeTimer <= 0)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        PlayerMovement player = collision.collider.GetComponentInParent<PlayerMovement>();

        if (player == null) return;

        if (player.transform.position.y > transform.position.y)
        {
            player.transform.SetParent(transform);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        PlayerMovement player = collision.collider.GetComponentInParent<PlayerMovement>();

        if (player == null) return;

        player.transform.SetParent(null);
    }

    private void OnDestroy()
    {
        ReleaseColumn();
    }

    private void ReleaseColumn()
    {
        if (hasReleasedColumn) return;

        hasReleasedColumn = true;

        if (ownerSpawner != null && columnIndex >= 0)
        {
            ownerSpawner.ReleaseColumn(columnIndex);
        }
    }
}