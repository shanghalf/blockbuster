using UnityEngine;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Xml.Serialization;
using System.Collections;
#if UNITY_EDITOR
    using UnityEditor;
#endif





[ExecuteInEditMode()]
public class MovingPlatform : Behavior
{

#if UNITY_EDITOR
    /// <summary>
    /// viewport callback 
    /// </summary>
    private void OnEnable()  { SceneView.onSceneGUIDelegate += OnCustomSceneGUI;}
    private void OnDisable() { SceneView.onSceneGUIDelegate -= OnCustomSceneGUI;} 
    

    protected void OnCustomSceneGUI(SceneView sceneview)
    {
        if (paramblock.m_pathnodes.Count == 0)
            return;
        string S = "";
        S += "current target = " + paramblock.GetSafeTargetIndex().ToString() + "\n";
        for (int i = 0; i < paramblock.m_pathnodes.Count; i++)
        {
            Handles.color = Color.blue;
            Actor AP = (Actor)GetComponent(typeof(Actor));
            if (paramblock.b_showdebuginfos)
            {
                Handles.Label(transform.position + Vector3.right * 5, S);
                S += "pathway= " + " [" + i.ToString() + "--" + (i + 1).ToString() + "] \n";
            }
            Pathnode p0 = paramblock.GetPathNode(i);
            Pathnode p1 = paramblock.GetPathNode(i + 1);
            Handles.DrawLine(p0.pos, p1.pos);
            if (paramblock.b_showdebuginfos)
            {
                Handles.Label(p0.pos + Vector3.up, i.ToString() + "\n" + "Timer :" + p0.timer.ToString());
                Handles.Label(transform.position + Vector3.up, paramblock.GetSafeTargetIndex().ToString());
            }
            //Handles.DrawBezier(transform.transform.position, oldPoint, oldPoint,-oldPoint,Color.red,null,width);
            Handles.FreeRotateHandle(Quaternion.identity, p0.pos, 0.2f);
            Handles.FreeRotateHandle(Quaternion.identity, p1.pos, 0.2f);
        }
    }



#endif



    public override void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        for (int c = 0; c < paramblock.m_pathnodes.Count; c++)
        {
            Gizmos.color = Color.red;
            Pathnode p = (Pathnode)paramblock.m_pathnodes[c];
            Vector3 lookatpoint = p.Getlookatpoint(p.ilookatpoint, 1.0f) + p.pos;
            Gizmos.DrawSphere(lookatpoint, 0.1f);
            Gizmos.DrawLine(lookatpoint, p.pos);

        }

    }

    public override void Wait(float tempo)
    {
        paramblock.ismoving = false;
        var a = new WaitForSeconds(tempo);
        WaitForSeconds s = new WaitForSeconds(tempo);
        paramblock.ismoving = true;
    }


    // Update is called once per frame
    public override void Update()
    {
        // useless to move 
        if (!paramblock.ismoving)
            return;

        Pathnode p = paramblock.m_pathnodes[paramblock.GetSafeTargetIndex()];
        Vector3 target = p.pos;
        if (Vector3.Distance(transform.position, target) == 0.0f)
        {
            if (p.timer > 0)
                #if UNITY_EDITOR   
                    p.timer -= editortick ;
                #endif
                #if !UNITY_EDITOR
                paramblock.pathnodes[paramblock.targetindex].timer -= Time.deltaTime;
                #endif
            else
            {
                p.timer = p.waitonnode;
                paramblock.SetSafeTargetIndex( paramblock.GetSafeTargetIndex() + paramblock.movedir );
            }
        }
        if (paramblock.ismoving)//|| (Vector3.Distance(transform.position, paramblock.pathnodes[0].pos) > 0.0f))
        {
            int lkp = p.ilookatpoint;
            float rspeed, tspeed;
            Vector3 targetpos = p.Getlookatpoint(lkp, 1.0f);
            // ude local speed for TR or global 
            if (paramblock.move_ampl == 0.0f)
            {
                rspeed = p.lookatspeed;
                tspeed = p.translatespeed;
            }
            else
            {
                rspeed = paramblock.move_ampl / 3; // arbitrary could find a better way 
                tspeed = paramblock.move_ampl;
            }
            if (looktarget != null)
                looktarget.transform.position = targetpos;

            var Qr = Quaternion.LookRotation(Vector3.up, targetpos);
            //Qr *= Quaternion.Euler(Vector3.forward);

            #if UNITY_EDITOR
                transform.position = Vector3.MoveTowards(transform.position, target, tspeed * editortick);
                transform.rotation = Quaternion.Slerp(transform.rotation, Qr, (rspeed * editortick));
             //Debug.Log(Vector3.MoveTowards(transform.position, target, tspeed * editortick).ToString());
            #endif
            #if !UNITY_EDITOR
                transform.position = Vector3.MoveTowards(transform.position, target, tspeed * Time.deltaTime);
                transform.rotation = Quaternion.Slerp(transform.rotation, Qr, (rspeed * Time.deltaTime));
            #endif

            switch (paramblock.indexbound )
            {
                case ARRAY_BOUND.DOWN :
                    paramblock.movedir = 1;
                    break;
                case ARRAY_BOUND.UP:
                    paramblock.movedir = -1;
                    break;
            }


        
        }

        }

}
