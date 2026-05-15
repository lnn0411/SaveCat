using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
public class DragonShardView : MonoBehaviour
{
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Collider shardCollider;

    private static MaterialPropertyBlock _mpb;
    private Coroutine _recycleRoutine;
    private Action<GameObject> _recycle;

    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (shardCollider == null) shardCollider = GetComponent<Collider>();
    }

    public void Play(Color color,Vector3 position,Quaternion rotation,Vector3 targetScale,Vector3 impulse,Vector3 torque,
        float lifeTime, Action<GameObject> recycle)
    {
        _recycle = recycle;

        transform.DOKill();

        if (_recycleRoutine != null)
        {
            StopCoroutine(_recycleRoutine);
            _recycleRoutine = null;
        }

        transform.SetPositionAndRotation(position, rotation);
        transform.localScale = targetScale * 0.15f;

        SetColor(color);

        rb.isKinematic = false;
        rb.useGravity = true;

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        if (shardCollider != null)
        {
            shardCollider.enabled = true;
        }

        transform.DOScale(targetScale, 0.12f).SetEase(Ease.OutBack);

        rb.AddForce(impulse, ForceMode.Impulse);
        rb.AddTorque(torque, ForceMode.Impulse);

        _recycleRoutine = StartCoroutine(RecycleAfter(lifeTime));
    }

    private IEnumerator RecycleAfter(float lifeTime)
    {
        yield return new WaitForSeconds(lifeTime);

        transform.DOKill();

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        if (shardCollider != null)
        {
            shardCollider.enabled = false;
        }

        _recycle?.Invoke(gameObject);
    }

    private void SetColor(Color color)
    {
        if (meshRenderer == null) return;

        if (_mpb == null)
        {
            _mpb = new MaterialPropertyBlock();
        }

        meshRenderer.GetPropertyBlock(_mpb);
        _mpb.SetColor("_Color", color);
        _mpb.SetColor("_BaseColor", color);
        meshRenderer.SetPropertyBlock(_mpb);
    }
}
