using UnityEngine;

public static class BlockColorUtility
{
    public static Color GetColor(BlockType type)
    {
        string hex = type switch
        {
            BlockType.Red => "#fc4337",
            BlockType.Blue => "#278cf8",
            BlockType.Green => "#54d62e",
            BlockType.Yellow => "#fddb00",
            BlockType.Purple => "#904ee6",
            _ => "#ffffff"
        };

        if (ColorUtility.TryParseHtmlString(hex, out Color color))
        {
            return color;
        }

        return Color.white;
    }
}
