using UnityEngine;
using System.Collections;

[System.Serializable]
public abstract class BBMovepadControll  
{

    System.Guid guid = System.Guid.NewGuid();
    NodeGraph nodegraph = new NodeGraph();
    string nodegraphpath;


	// Use this for initialization
	public int GetID() 
    {
        return guid.GetHashCode();
	}

    public void Init ()
    {

        nodegraphpath = BBDir.Get(BBpath.SETING) + guid.GetHashCode() + ".xml";
        if (!System.IO.File.Exists(nodegraphpath))
        {
            BBCtrlEditor.MovepadButtonEdited = this;
            BBCtrlEditor.init();
        }

    }



    public abstract void Invoke();

}


public class BButton : BBMovepadControll 
{

    public override void Invoke()
    {
       // DoBlockMove(false, (front * (ofset = (b_fixedstepedit) ? stepvalue : (b_front_X) ? BlockSize.x : BlockSize.z)));
    }

}