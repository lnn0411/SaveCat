using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameObject coreGroup = new GameObject("CoreManagers");

        coreGroup.AddComponent<ActivityRoot>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
