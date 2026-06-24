using UnityEngine;
using UnityEngine.UI;

public class CrosshairTargetIndicator : MonoBehaviour
{
    [SerializeField] private Image crosshairImage;
    [SerializeField] private Graphic[] crosshairGraphics;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color targetColor = Color.red;
    [SerializeField] private float maxDistance = 80f;
    [SerializeField] private float matchRadius = 2f;
    [SerializeField] private float bodyRayHeight = 1.2f;
    [SerializeField] private LayerMask aimMask = Physics.DefaultRaycastLayers;
    [SerializeField] private bool drawDebugRays = true;

    private PlayerCharacter _player;
    private CharacterCombat _combat;
    private Camera _camera;
    private readonly RaycastHit[] _raycastHits = new RaycastHit[16];
    private Vector3 _lastBodyOrigin;
    private Vector3 _lastBodyDirection;
    private Ray _lastCameraRay;
    private bool _lastMatched;

    
    public void Initialize(PlayerCharacter player)
    {
        _player = player;
        _combat = player != null ? player.GetComponent<CharacterCombat>() : null;
        _camera = Camera.main;

        SetColor(normalColor);
        SetVisible(_player != null && IsCursorLocked());
    }

    private void Awake()
    {
        if (crosshairImage == null)
        {
            crosshairImage = GetComponent<Image>();
        }

        CacheGraphics();
    }

    public static CrosshairTargetIndicator CreateDefault(Transform parent)
    {
        Canvas canvas = parent.GetComponentInParent<Canvas>();
        Transform rootParent = canvas != null ? canvas.transform : parent;

        GameObject root = new GameObject(
            "Crosshair",
            typeof(RectTransform),
            typeof(CrosshairTargetIndicator));

        root.transform.SetParent(rootParent, false);
        root.transform.SetAsLastSibling();

        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0.5f, 0.5f);
        rootRect.anchorMax = new Vector2(0.5f, 0.5f);
        rootRect.anchoredPosition = Vector2.zero;
        rootRect.sizeDelta = new Vector2(36f, 36f);

        Graphic[] graphics =
        {
            CreateLine(root.transform, "Top", new Vector2(0f, 10f), new Vector2(2f, 10f)),
            CreateLine(root.transform, "Bottom", new Vector2(0f, -10f), new Vector2(2f, 10f)),
            CreateLine(root.transform, "Left", new Vector2(-10f, 0f), new Vector2(10f, 2f)),
            CreateLine(root.transform, "Right", new Vector2(10f, 0f), new Vector2(10f, 2f))
        };

        CrosshairTargetIndicator indicator =
            root.GetComponent<CrosshairTargetIndicator>();

        indicator.crosshairGraphics = graphics;
        indicator.CacheGraphics();

        return indicator;
    }

    private static Graphic CreateLine(
        Transform parent,
        string name,
        Vector2 anchoredPosition,
        Vector2 size)
    {
        GameObject line = new GameObject(
            name,
            typeof(RectTransform),
            typeof(Image));

        line.transform.SetParent(parent, false);

        RectTransform rect = line.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        Image image = line.GetComponent<Image>();
        image.raycastTarget = false;

        return image;
    }
    private bool _isCrosshairVisible;

    private void SetVisible(bool visible)
    {
        if (_isCrosshairVisible == visible)
        {
            return;
        }

        _isCrosshairVisible = visible;

        if (crosshairGraphics != null && crosshairGraphics.Length > 0)
        {
            foreach (Graphic graphic in crosshairGraphics)
            {
                if (graphic != null)
                {
                    graphic.enabled = visible;
                }
            }
        }

        if (crosshairImage != null)
        {
            crosshairImage.enabled = visible;
        }
    }
    private void Update()
    {
        bool shouldShow =
            Cursor.lockState == CursorLockMode.Locked &&
            (RoundManager.Instance == null ||
             RoundManager.Instance.CurrentState != RoundState.CharacterSelect);

        SetVisible(shouldShow);

        if (!shouldShow)
        {
            SetColor(normalColor);
            return;
        }

        if (_player == null)
        {
            SetVisible(false);
            SetColor(normalColor);
            return;
        }

        if (_camera == null)
        {
            _camera = Camera.main;
        }

        RefreshDebugRays();
        DrawDebugRays();

        SetVisible(shouldShow);

        if (!shouldShow)
        {
            SetColor(normalColor);
            return;
        }

        bool isMatched = _camera != null && IsCameraAimMatchedWithBodyAim();

        _lastMatched = isMatched;
        SetColor(isMatched ? targetColor : normalColor);
    }

    private bool IsCursorLocked()
    {
        return Cursor.lockState == CursorLockMode.Locked;
    }

    private bool IsCameraAimMatchedWithBodyAim()
    {
        Ray cameraRay = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        _lastCameraRay = cameraRay;

        if (!TryGetFirstNonSelfRayHit(cameraRay, out RaycastHit cameraHit))
        {
            return false;
        }

        if (!IsValidTarget(cameraHit.collider))
        {
            return false;
        }

        Vector3 bodyOrigin = GetBodyRayOrigin();
        Vector3 bodyDirection = GetBodyRayDirection();
        _lastBodyOrigin = bodyOrigin;
        _lastBodyDirection = bodyDirection;

        if (bodyDirection.sqrMagnitude <= 0.001f)
        {
            return false;
        }

        bodyDirection.Normalize();
        _lastBodyDirection = bodyDirection;

        if (HasMatchingBodyHit(
                bodyOrigin,
                matchRadius,
                bodyDirection,
                cameraHit.collider))
        {
            return true;
        }

        return IsValidTarget(cameraHit.collider) &&
               IsPointNearBodyRay(cameraHit.point, bodyOrigin, bodyDirection);
    }

    private bool TryGetFirstNonSelfRayHit(Ray ray, out RaycastHit result)
    {
        int hitCount = Physics.RaycastNonAlloc(
            ray,
            _raycastHits,
            maxDistance,
            aimMask,
            QueryTriggerInteraction.Ignore);

        return TryGetClosestNonSelfHit(hitCount, out result);
    }

    private bool HasMatchingBodyHit(
        Vector3 origin,
        float radius,
        Vector3 direction,
        Collider cameraTarget)
    {
        int hitCount = Physics.SphereCastNonAlloc(
            origin,
            radius,
            direction,
            _raycastHits,
            maxDistance,
            aimMask,
            QueryTriggerInteraction.Ignore);

        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit hit = _raycastHits[i];

            if (hit.collider == null || IsSelf(hit.collider))
            {
                continue;
            }

            if (HasSameDamageableRoot(cameraTarget, hit.collider) &&
                IsValidTarget(hit.collider))
            {
                return true;
            }
        }

        return false;
    }

    private bool TryGetClosestNonSelfHit(int hitCount, out RaycastHit result)
    {
        result = default;
        float closestDistance = float.MaxValue;
        bool found = false;

        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit hit = _raycastHits[i];

            if (hit.collider == null || IsSelf(hit.collider))
            {
                continue;
            }

            if (hit.distance >= closestDistance)
            {
                continue;
            }

            closestDistance = hit.distance;
            result = hit;
            found = true;
        }

        return found;
    }

    private Vector3 GetBodyRayOrigin()
    {
        if (_combat != null && _combat.AttackSpawnPoint != null)
        {
            return _combat.AttackSpawnPoint.position;
        }

        return _player.transform.position + Vector3.up * bodyRayHeight;
    }

    private Vector3 GetBodyRayDirection()
    {
        if (_combat != null && _combat.AttackSpawnPoint != null)
        {
            return _combat.AttackSpawnPoint.forward;
        }

        return _player.transform.forward;
    }

    private void RefreshDebugRays()
    {
        if (_camera != null)
        {
            _lastCameraRay = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        }

        _lastBodyOrigin = GetBodyRayOrigin();
        _lastBodyDirection = GetBodyRayDirection();
    }

    private void DrawDebugRays()
    {
        if (!drawDebugRays)
        {
            return;
        }

        if (_lastCameraRay.direction.sqrMagnitude > 0.001f)
        {
            Debug.DrawRay(
                _lastCameraRay.origin,
                _lastCameraRay.direction * maxDistance,
                _lastMatched ? Color.red : Color.white);
        }

        if (_lastBodyDirection.sqrMagnitude > 0.001f)
        {
            Debug.DrawRay(
                _lastBodyOrigin,
                _lastBodyDirection.normalized * maxDistance,
                _lastMatched ? Color.red : Color.yellow);
        }
    }

    private bool IsPointNearBodyRay(Vector3 point, Vector3 rayOrigin, Vector3 rayDirection)
    {
        Vector3 toPoint = point - rayOrigin;
        float forwardDistance = Vector3.Dot(toPoint, rayDirection);

        if (forwardDistance < 0f || forwardDistance > maxDistance)
        {
            return false;
        }

        Vector3 closestPoint = rayOrigin + rayDirection * forwardDistance;
        return Vector3.Distance(point, closestPoint) <= matchRadius;
    }

    private bool HasSameDamageableRoot(Collider a, Collider b)
    {
        IDamageable damageableA = a.GetComponentInParent<IDamageable>();
        IDamageable damageableB = b.GetComponentInParent<IDamageable>();

        if (damageableA == null || damageableB == null)
        {
            return a.transform.root == b.transform.root;
        }

        return damageableA.GetDamageableObject() == damageableB.GetDamageableObject();
    }

    private bool IsValidTarget(Collider hit)
    {
        IDamageable damageable = hit.GetComponentInParent<IDamageable>();

        if (damageable == null)
        {
            return false;
        }

        GameObject targetObject = damageable.GetDamageableObject();

        if (targetObject == null || targetObject.transform.root == _player.transform.root)
        {
            return false;
        }

        PlayerCharacter targetPlayer = targetObject.GetComponent<PlayerCharacter>();

        if (targetPlayer != null && targetPlayer.Team == _player.Team)
        {
            return false;
        }

        return true;
    }

    private bool IsSelf(Collider hit)
    {
        return hit != null &&
               _player != null &&
               hit.transform.root == _player.transform.root;
    }

    private void SetColor(Color color)
    {
        bool changedAny = false;

        if (crosshairGraphics != null && crosshairGraphics.Length > 0)
        {
            foreach (Graphic graphic in crosshairGraphics)
            {
                if (graphic != null)
                {
                    graphic.color = color;
                    changedAny = true;
                }
            }
        }

        if (crosshairImage != null && !changedAny)
        {
            crosshairImage.color = color;
        }
    }

    private void CacheGraphics()
    {
        if (crosshairGraphics == null || crosshairGraphics.Length == 0)
        {
            crosshairGraphics = GetComponentsInChildren<Graphic>(true);
        }
    }

    private void OnDrawGizmos()
    {
        if (!drawDebugRays)
        {
            return;
        }

        Gizmos.color = _lastMatched ? Color.red : Color.white;

        if (_lastCameraRay.direction.sqrMagnitude > 0.001f)
        {
            Gizmos.DrawRay(_lastCameraRay.origin, _lastCameraRay.direction * maxDistance);
        }

        if (_lastBodyDirection.sqrMagnitude > 0.001f)
        {
            Gizmos.DrawRay(_lastBodyOrigin, _lastBodyDirection.normalized * maxDistance);
            Gizmos.DrawWireSphere(_lastBodyOrigin + _lastBodyDirection.normalized * 2f, matchRadius);
        }
    }
}
