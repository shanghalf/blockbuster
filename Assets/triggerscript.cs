using UnityEngine;
using System.Collections;

public class triggerscript : MonoBehaviour {

    public GameObject  triggerobject ;
    public GameObject   parentobject  ;
    public bool triggeronce;
    private blocksetup bb;

	// Use this for initialization
	void Start () 
    {
        blocksetup bb ;
        if (parentobject)
        {
            bb = (blocksetup)parentobject.GetComponent("blocksetup");
            bb.paramblock.ismoving = false;
        }
   	
	}
	
	// Update is called once per frame
	void Update () 
    {
   
	}

    void OnTriggerEnter(Collider other)
    {
        if (!parentobject)
            return;

        bb = (blocksetup)parentobject.GetComponent("blocksetup");
		bb.paramblock.ismoving = true;
    
    }

}
