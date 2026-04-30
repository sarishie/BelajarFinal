using UnityEngine;

public class SimpleGrappleHook : MonoBehaviour
{
    [Header("Grapple Points")]
    public Transform snapToThisPoint;
    public Transform centerPos;

    [Header("Settings")]
    public bool autoSnapWhenTouch = true;
    public KeyCode grappleKey = KeyCode.E;

    [Header("Debug")]
    public bool showDebug = true;
    public bool drawDebugGizmos = true;

    private PlayerMovement playerInArea;
    private bool canGrapple;

    private void Start()
    {
        if (showDebug)
        {
            Debug.Log("[SimpleGrappleHook] Script aktif di object: " + gameObject.name);
        }

        if (snapToThisPoint == null)
        {
            Debug.LogWarning("[SimpleGrappleHook] snapToThisPoint belum diisi di Inspector! Object: " + gameObject.name);
        }

        if (centerPos == null)
        {
            Debug.LogWarning("[SimpleGrappleHook] centerPos belum diisi di Inspector! Object: " + gameObject.name);
        }

        Collider2D col = GetComponent<Collider2D>();

        if (col == null)
        {
            Debug.LogWarning("[SimpleGrappleHook] Tidak ada Collider2D di object ini. Tambahkan BoxCollider2D ke: " + gameObject.name);
        }
        else if (!col.isTrigger)
        {
            Debug.LogWarning("[SimpleGrappleHook] Collider2D belum Is Trigger. Centang Is Trigger di object: " + gameObject.name);
        }
    }

    private void Update()
    {
        if (autoSnapWhenTouch) return;

        if (canGrapple && playerInArea != null && Input.GetKeyDown(grappleKey))
        {
            if (showDebug)
            {
                Debug.Log("[SimpleGrappleHook] Tombol grapple ditekan. Mulai snap player.");
            }

            StartPlayerGrapple();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (showDebug)
        {
            Debug.Log("[SimpleGrappleHook] Ada object masuk trigger: " + collision.gameObject.name);
        }

        PlayerMovement player = collision.GetComponentInParent<PlayerMovement>();

        if (player == null)
        {
            if (showDebug)
            {
                Debug.LogWarning("[SimpleGrappleHook] Object yang masuk tidak punya PlayerMovement: " + collision.gameObject.name);
            }

            return;
        }

        playerInArea = player;
        canGrapple = true;

        if (showDebug)
        {
            Debug.Log("[SimpleGrappleHook] Player terdeteksi: " + player.gameObject.name);
        }

        if (autoSnapWhenTouch)
        {
            if (showDebug)
            {
                Debug.Log("[SimpleGrappleHook] Auto snap aktif. Memanggil StartPlayerGrapple().");
            }

            StartPlayerGrapple();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        PlayerMovement player = collision.GetComponentInParent<PlayerMovement>();

        if (player == null) return;

        if (player == playerInArea)
        {
            if (showDebug)
            {
                Debug.Log("[SimpleGrappleHook] Player keluar dari area grapple: " + player.gameObject.name);
            }

            playerInArea = null;
            canGrapple = false;
        }
    }

    private void StartPlayerGrapple()
    {
        if (playerInArea == null)
        {
            Debug.LogWarning("[SimpleGrappleHook] Gagal grapple: playerInArea masih null.");
            return;
        }

        if (snapToThisPoint == null)
        {
            Debug.LogWarning("[SimpleGrappleHook] Gagal grapple: snapToThisPoint belum diisi.");
            return;
        }

        if (centerPos == null)
        {
            Debug.LogWarning("[SimpleGrappleHook] Gagal grapple: centerPos belum diisi.");
            return;
        }

        if (showDebug)
        {
            Debug.Log("[SimpleGrappleHook] BERHASIL memanggil StartGrapple(). Player akan snap ke: " + snapToThisPoint.name);
        }

        playerInArea.StartGrapple(snapToThisPoint, centerPos);
    }

    private void OnDrawGizmos()
    {
        if (!drawDebugGizmos) return;

        if (snapToThisPoint != null)
        {
            Gizmos.DrawWireSphere(snapToThisPoint.position, 0.2f);
            Gizmos.DrawLine(transform.position, snapToThisPoint.position);
        }

        if (centerPos != null)
        {
            Gizmos.DrawWireSphere(centerPos.position, 0.3f);
        }
    }
}