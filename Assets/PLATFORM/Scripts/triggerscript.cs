using UnityEngine;
using System.Collections;

public class triggerscript : MonoBehaviour {

    public GameObject  triggerobject ;
    public GameObject   parentobject  ;
    public bool triggeronce;
    private BBehavior bb;

	// Use this for initialization
	void Start () 
    {
        BBehavior bb ;
        if (parentobject)
        {
            bb = (BBehavior)parentobject.GetComponent("Platform");
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

        bb = (BBehavior)parentobject.GetComponent("Platform");
    
    }

}
