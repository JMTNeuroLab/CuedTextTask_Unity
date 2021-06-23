using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttachedObject
{
    public static AttachedObject instance = null;
    private GameObject go;
    private GameObject CueObject; 
    private MeshRenderer mr;
    private MeshFilter mf;

    public AttachedObject()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    // Start is called before the first frame update
    public void Attach(Mesh m, Material mat, Transform attach_to, GameObject attach_from)
    {
        // Hide collided Cue Object
        CueObject = attach_from;
        CueObject.GetComponent<Collider>().enabled = false;
        CueObject.GetComponent<Renderer>().enabled = false;

        go = new GameObject(m.name);
        go.transform.parent = attach_to;
        go.transform.localPosition = new Vector3 { x = 0, y = 0, z = 2.5f };
        go.transform.localScale = new Vector3 { x = 0.5f, y = 0.5f, z = 0.5f };
        mf = go.AddComponent<MeshFilter>();
        mr = go.AddComponent<MeshRenderer>();

        mf.mesh = new Mesh
        {
            vertices = m.vertices, 
            normals = m.normals, 
            subMeshCount = m.subMeshCount, 
            uv = m.uv, 
            triangles = m.triangles, 
            tangents = m.tangents
        };
        mr.material = mat;
    }

    public void Detach(bool showOrigCollider)
    {
        // Remove attached object
        Object.Destroy(go);

        // Show original collider
        if (showOrigCollider && CueObject != null)
        {
            CueObject.GetComponent<Collider>().enabled = true;
            CueObject.GetComponent<Renderer>().enabled = true;
        }
    }



    // Update is called once per frame
    void Update()
    {
        
    }
}
