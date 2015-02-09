using UnityEngine;
using System.Collections;

public class triggerscript : MonoBehaviour {

    public GameObject  triggerobject ;
    public GameObject   parentobject  ;
    public bool triggeronce;
    private Behavior bb;

	// Use this for initialization
	void Start () 
    {
        Behavior bb ;
        if (parentobject)
        {
            bb = (Behavior)parentobject.GetComponent("Platform");
            bb.paramblock.ismoving = false;
        }
   	
	}
	
	// Update is called once per frame
	void Update () 
    {
        // test for git integration vs 2012

	}

    void OnTriggerEnter(Collider other)
    {
        if (!parentobject)
            return;

        bb = (Behavior)parentobject.GetComponent("Platform");
		bb.paramblock.ismoving = true;
    
    }

}
