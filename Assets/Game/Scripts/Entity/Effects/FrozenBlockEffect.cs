using UnityEngine;

/// <summary>
/// Frozen blocks cannot be clicked until another block bumps into them.
/// </summary>
public class FrozenBlockEffect : IBlockEffect
{
    private const string EffectDisplayName = "Frozen";
    private const string BlockMeshChildName = "重构网格_1";
    private const string IceMaterialAssetPath = "Assets/Game/Shader/Ice/M_Ice_Basic.mat";

    private static Material _cachedIceMaterial;

    public string EffectName => EffectDisplayName;

    private BlockData _blockData;
    private MeshRenderer _targetRenderer;
    private Material[] _originalMaterials;
    private bool _isFrozen = true;

    public void OnBlockInitialized(BlockData data, BlockView view)
    {
        _blockData = data;
        ApplyFrozenMaterial(view);

        Debug.Log($"[FrozenBlockEffect] Block {data.Id} has been frozen.");
    }

    public bool CanBeClicked()
    {
        return !_isFrozen;
    }

    public void OnHitByOtherBlock(BlockData hitByBlock)
    {
        if (!_isFrozen)
        {
            return;
        }

        _isFrozen = false;
        Debug.Log($"[FrozenBlockEffect] Block {_blockData.Id} unfrozen by Block {hitByBlock.Id}.");

        _blockData.RemoveEffect(this);
    }

    public void OnEffectRemoved()
    {
        RestoreOriginalMaterial();
    }

    private void ApplyFrozenMaterial(BlockView view)
    {
        if (view == null)
        {
            return;
        }

        _targetRenderer = FindTargetRenderer(view);
        if (_targetRenderer == null)
        {
            Debug.LogWarning($"[FrozenBlockEffect] Block {_blockData.Id} has no MeshRenderer target for frozen material.");
            return;
        }

        Material iceMaterial = GetIceMaterial();
        if (iceMaterial == null)
        {
            Debug.LogWarning($"[FrozenBlockEffect] Ice material not found at {IceMaterialAssetPath}.");
            return;
        }

        _originalMaterials = _targetRenderer.materials;

        int materialCount = Mathf.Max(1, _originalMaterials.Length);
        Material[] frozenMaterials = new Material[materialCount];
        for (int i = 0; i < frozenMaterials.Length; i++)
        {
            frozenMaterials[i] = iceMaterial;
        }

        _targetRenderer.materials = frozenMaterials;
    }

    private void RestoreOriginalMaterial()
    {
        if (_targetRenderer == null || _originalMaterials == null || _originalMaterials.Length == 0)
        {
            return;
        }

        _targetRenderer.materials = _originalMaterials;
        _originalMaterials = null;
    }

    private MeshRenderer FindTargetRenderer(BlockView view)
    {
        Transform meshTransform = view.transform.Find(BlockMeshChildName);
        if (meshTransform != null && meshTransform.TryGetComponent(out MeshRenderer meshRenderer))
        {
            return meshRenderer;
        }

        return view.GetComponentInChildren<MeshRenderer>();
    }

    private static Material GetIceMaterial()
    {
        if (_cachedIceMaterial != null)
        {
            return _cachedIceMaterial;
        }

#if UNITY_EDITOR
        _cachedIceMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(IceMaterialAssetPath);
#endif

        if (_cachedIceMaterial == null)
        {
            _cachedIceMaterial = Resources.Load<Material>("M_Ice_Basic");
        }

        return _cachedIceMaterial;
    }
}
