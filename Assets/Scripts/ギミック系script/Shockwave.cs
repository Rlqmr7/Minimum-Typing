using UnityEngine;

public class Shockwave : MonoBehaviour
{
    [Header("移動・消滅設定")]
    public float moveSpeed = 6f;
    public Vector3 moveDirection = Vector3.right; // 右（+X）方向へ進む
    public float lifeTime = 2.0f;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.position += moveDirection.normalized * moveSpeed * Time.deltaTime;
    }
}