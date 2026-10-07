using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Object : MonoBehaviour
{
    // Start is called before the first frame update
    public string id;
    public string name_;
    public stats stats;

    public float threatRelationshipValue;
    public float allegianceRelationshipValue;
    public bool isActive = true;
    protected virtual void Awake()
    {
        ObjectRegistry.Register(id, this);
    }

    protected virtual void OnDestroy()
    {
        ObjectRegistry.Unregister(id, this);
    }
}
