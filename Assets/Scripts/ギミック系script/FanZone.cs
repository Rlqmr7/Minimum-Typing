using UnityEngine;

[RequireComponent(typeof(Collider))]
public class FanZone : MonoBehaviour
{
    [Header("風の基本設定")]
    [Tooltip("風の吹き出し口（未指定ならこのオブジェクトの位置）")]
    public Transform fanOrigin;

    [Tooltip("風の吹く方向（ローカル基準なら transform.forward などを自動使用）")]
    public bool useObjectForward = true;
    public Vector3 customWindDirection = Vector3.forward;

    [Header("風力と距離減衰")]
    [Tooltip("吹き出し口（距離0）での最大風速")]
    public float maxWindSpeed = 12f;

    [Tooltip("風が届く最大距離")]
    public float maxDistance = 10f;

    [Tooltip("横軸: 距離割合(0=至近距離, 1=最大距離) / 縦軸: 風力の強さ割合")]
    public AnimationCurve falloffCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0.1f);

    [Header("稼働状態")]
    [SerializeField] private bool isFanActive = true;
    public bool IsFanActive => isFanActive;

    [Header("ビジュアル（任意）")]
    public ParticleSystem windParticles;
    public Transform fanBlades;
    public float bladeRotateSpeed = 720f;

    private void Awake()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;

        if (fanOrigin == null)
        {
            fanOrigin = transform;
        }
    }

    private void Update()
    {
        if (isFanActive && fanBlades != null)
        {
            fanBlades.Rotate(Vector3.forward, bladeRotateSpeed * Time.deltaTime, Space.Self);
        }
    }

    public void SetFanActive(bool active)
    {
        isFanActive = active;

        if (windParticles != null)
        {
            if (isFanActive) windParticles.Play();
            else windParticles.Stop();
        }
    }

    public void ToggleFan()
    {
        SetFanActive(!isFanActive);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!isFanActive) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null)
        {
            player = other.GetComponentInParent<PlayerController>();
        }

        if (player != null)
        {
            Vector3 windVel = CalculateWindVelocity(player.transform.position);
            player.AddExternalWind(windVel);
        }
    }

    public Vector3 CalculateWindVelocity(Vector3 targetPos)
    {
        if (!isFanActive) return Vector3.zero;

        Vector3 dir = useObjectForward ? fanOrigin.forward : customWindDirection.normalized;
        Vector3 toTarget = targetPos - fanOrigin.position;

        float distanceAlongWind = Mathf.Max(0f, Vector3.Dot(toTarget, dir));

        if (distanceAlongWind > maxDistance)
        {
            return Vector3.zero;
        }

        float normalizedDist = Mathf.Clamp01(distanceAlongWind / maxDistance);
        float strengthMultiplier = falloffCurve.Evaluate(normalizedDist);

        return dir * (maxWindSpeed * strengthMultiplier);
    }

    public Vector3 GetWindVelocityAt(Vector3 worldPos)
    {
        return CalculateWindVelocity(worldPos);
    }

    public float GetWindSpeedAt(Vector3 worldPos)
    {
        return CalculateWindVelocity(worldPos).magnitude;
    }

    private void OnDrawGizmosSelected()
    {
        Transform origin = fanOrigin != null ? fanOrigin : transform;
        Vector3 dir = useObjectForward ? origin.forward : customWindDirection.normalized;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(origin.position, origin.position + dir * maxDistance);
        Gizmos.DrawWireSphere(origin.position, 0.25f);
        Gizmos.DrawWireSphere(origin.position + dir * maxDistance, 0.15f);
    }
}