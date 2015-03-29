using UnityEngine;
using System.Collections;

[System.Serializable]
public abstract class BButton  
{
    int m_functionID;
	// Use this for initialization
	public virtual int GetID() 
    {
        return m_functionID;
	}
	// Update is called once per frame
	public virtual void SetID(int I) 
    {
        m_functionID = I;
	}

    public abstract void Invoke();

}


public class BButtonMove : BButton 
{
    public override void Invoke()
    {
       // DoBlockMove(false, (front * (ofset = (b_fixedstepedit) ? stepvalue : (b_front_X) ? BlockSize.x : BlockSize.z)));
    }

}