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
    



    public override void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        int pmax = paramblock.GetPathNodes().Count;
        if (pmax <= 0)
            return;
        for (int c = 0; c <= pmax - 1; c++)
        {
            Gizmos.color = Color.red;
            Pathnode p = (Pathnode)paramblock.GetPathNode(c);
            Vector3 lookatpoint = p.Getlookatpoint(p.ilookatpoint, 1.0f) + p.pos;
            Gizmos.DrawSphere(lookatpoint, 0.1f);
            Gizmos.DrawLine(lookatpoint, p.pos);

        }

    }
    // Use this for initialization
    public override void Start()
    {
        if (paramblock.ismoving)
        {
            /*   rp = (ReplayerLogOutput)gameObject.AddComponent(typeof(ReplayerLogOutput));
            rp.m_entityname = this.name;*/
        }
        if (!block_transform)
            return;
        //Actorprops.orig_transform = block_transform.rotation;
        //Actorprops.orig_pos = block_transform.position;

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
        Pathnode p = paramblock.GetPathNode(paramblock.targetindex);
        int pmax = paramblock.m_pathnodes.Count;
        if (paramblock.targetindex >= pmax || paramblock.editsub)
            return;
        if (paramblock.targetindex >= pmax - 1)
        {
            if (paramblock.b_pathloop)
                if ((Vector3.Distance(transform.position, p.pos) == 0.0f))
                {
                    paramblock.targetindex = 0;
                    paramblock.movedir = (1);
                    // cannot pop at exact pos 
                    Vector3 offs = Vector3.forward / 100; // slight offset 
                    transform.position = p.pos + offs;
                }
            paramblock.movedir = (-1);
        }
        else if (paramblock.targetindex <= 0)
            paramblock.movedir = (1);
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
                paramblock.targetindex += paramblock.movedir;
            }
        }
        if (paramblock.ismoving)//|| (Vector3.Distance(transform.position, paramblock.pathnodes[0].pos) > 0.0f))
        {
            int ti = paramblock.targetindex;
            int lkp = p.ilookatpoint;
            //Debug.Log(lkp);
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
#endif
#if !UNITY_EDITOR
            transform.position = Vector3.MoveTowards(transform.position, target, tspeed * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, Qr, (rspeed * Time.deltaTime));
#endif


        
        }

        }

}
