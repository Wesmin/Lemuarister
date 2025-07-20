using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetLine : MonoBehaviour
{
    private MeshRenderer[] tfs;
    // Start is called before the first frame update
    void Start()
    {
        Transform traPar = transform.Find("PointerRenderer");
        tfs = traPar.GetComponentsInChildren<MeshRenderer>();
        //Debug.Log(tfs.Length);
        Material material = new Material(Shader.Find("Custom/Shader1"));
        foreach (MeshRenderer mr in tfs)
        {
            mr.material = material;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
