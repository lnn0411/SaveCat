using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Block effect helpers.
/// </summary>
public class BlockEffectManager
{
    public static void AddFrozenEffect(BlockData blockData, BlockView blockView = null)
    {
        if (blockData == null)
        {
            return;
        }

        FrozenBlockEffect frozenEffect = new FrozenBlockEffect();
        blockData.AddEffect(frozenEffect);

        if (blockView != null)
        {
            frozenEffect.OnBlockInitialized(blockData, blockView);
        }
    }

    public static void AddFrozenEffectWithProbability(BlockData blockData, float probability, BlockView blockView = null)
    {
        if (blockData == null)
        {
            return;
        }

        if (!GridMapManager.Instance.CanBeHitByOtherBlocks(blockData))
        {
            Debug.Log($"[BlockEffectManager] Block {blockData.Id} cannot be frozen because no other block can hit it.");
            return;
        }

        if (Random.value < probability)
        {
            AddFrozenEffect(blockData, blockView);
        }
    }

    public static void AddFrozenEffectToRandomBlocks(
        List<BlockData> blocks,
        Dictionary<BlockData, BlockView> blockViews,
        int count)
    {
        if (count <= 0 || blocks == null || blocks.Count == 0)
        {
            return;
        }

        count = Mathf.Min(count, blocks.Count);

        List<BlockData> shuffledBlocks = new List<BlockData>(blocks);
        for (int i = 0; i < shuffledBlocks.Count; i++)
        {
            int randomIndex = Random.Range(i, shuffledBlocks.Count);
            BlockData temp = shuffledBlocks[i];
            shuffledBlocks[i] = shuffledBlocks[randomIndex];
            shuffledBlocks[randomIndex] = temp;
        }

        List<BlockData> selectedFrozenBlocks = new List<BlockData>();
        HashSet<int> selectedFrozenIds = new HashSet<int>();

        foreach (BlockData block in shuffledBlocks)
        {
            if (selectedFrozenBlocks.Count >= count || block == null)
            {
                continue;
            }

            selectedFrozenBlocks.Add(block);
            selectedFrozenIds.Add(block.Id);

            if (!CanAllSelectedFrozenBlocksBeHit(selectedFrozenBlocks, selectedFrozenIds))
            {
                selectedFrozenBlocks.RemoveAt(selectedFrozenBlocks.Count - 1);
                selectedFrozenIds.Remove(block.Id);
            }
        }

        if (selectedFrozenBlocks.Count <= 0)
        {
            Debug.Log("[BlockEffectManager] No blocks can be frozen because no unfrozen block can hit them.");
            return;
        }

        for (int i = 0; i < selectedFrozenBlocks.Count; i++)
        {
            BlockData block = selectedFrozenBlocks[i];
            BlockView view = blockViews != null && blockViews.TryGetValue(block, out BlockView blockView)
                ? blockView
                : null;

            AddFrozenEffect(block, view);
        }

        Debug.Log($"[BlockEffectManager] Froze {selectedFrozenBlocks.Count} blocks.");
    }

    public static void AddFrozenEffectToBlocksByType(
        List<BlockData> blocks,
        BlockType targetType,
        Dictionary<BlockData, BlockView> blockViews = null)
    {
        if (blocks == null)
        {
            return;
        }

        int frozenCount = 0;
        foreach (BlockData block in blocks)
        {
            if (block == null || block.Type != targetType)
            {
                continue;
            }

            if (!GridMapManager.Instance.CanBeHitByOtherBlocks(block))
            {
                Debug.Log($"[BlockEffectManager] Block {block.Id} (type {targetType}) cannot be frozen because no other block can hit it.");
                continue;
            }

            BlockView view = blockViews != null && blockViews.TryGetValue(block, out BlockView blockView)
                ? blockView
                : null;

            AddFrozenEffect(block, view);
            frozenCount++;
        }

        Debug.Log($"[BlockEffectManager] Froze {frozenCount} blocks of type {targetType}.");
    }

    private static bool CanAllSelectedFrozenBlocksBeHit(List<BlockData> selectedFrozenBlocks, HashSet<int> selectedFrozenIds)
    {
        foreach (BlockData frozenBlock in selectedFrozenBlocks)
        {
            if (!GridMapManager.Instance.CanBeHitByOtherBlocks(frozenBlock, selectedFrozenIds))
            {
                return false;
            }
        }

        return true;
    }
}
