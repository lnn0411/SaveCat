using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragonTailView : DragonBaseView
{
    public override void UpdatePositionAndRotation(Vector3 position, Quaternion rotation)
    {
        transform.position = position;
        transform.rotation = rotation;
    }
}
