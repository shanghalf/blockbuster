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

/// for auto registration refer to template for more details  
/// <autoenum> MOVING_PLATFORM MovingPlatform </autoenum>



[System.Serializable]
public class MovingPlatformPathnode : Pathnode 
{
    public Vector3 pos;
    public int ilookatpoint = 0;
    //public List<Vector3> orb = new List<Vector3>() ;
    public float lookatspeed = 0.0f;
    public float translatespeed = 0.0f;
    public float waitonnode = 0.0f;
    public float Wtimer = 0.0f;
    public virtual Vector3 Getlookatpoint(int lookatindex, float radius, int step = 8)
    {
        float a = ((360.0f / step) * Mathf.Deg2Rad) * lookatindex + (Mathf.Deg2Rad * 45.0f);
        float ca = Mathf.Cos(a);
        float sa = Mathf.Sin(a);
        Vector3 RV = new Vector3(radius * ca - radius * sa, 0.0f, radius * sa + radius * ca);
        return (RV);//+ pos) ;
    }

}

[System.Serializable]
public class MovingPlatformDataset  : Dataset
{
  
    public List<MovingPlatformPathnode> m_pathnodes = new List<MovingPlatformPathnode>();
    private int targetindex = 0;
    public MovingPlatformPathnode rotatelookpoint = new MovingPlatformPathnode();
    public float speed = 0.5f;
    public float move_ampl=0.0f;
    public Vector3 target = Vector3.zero;
    public bool b_revert_rotation=false;
    public bool b_triggeronce=false;
    public float rotationspeed=0.0f;
    public int rotationstepnumber=2;
    public float rotationtempo=0.0f;
    public bool b_triggered=false;
    public int rotateindex=0;
    public int movedir = 1;
    public int maxhandle=0;
    public Vector3[] quater = new Vector3[] { Vector3.forward, Vector3.left, Vector3.back, Vector3.right };
    public bool b_pathloop =false;
    public COLIDER_TYPE colider_type = COLIDER_TYPE.BOX;
    public bool b_path_rotation = true;
    //public bool editsub;
    //public bool ismoving;
    //public bool b_front_x;
    //public bool b_showdebuginfos;
    //public ARRAY_BOUND indexbound;
    //public string actorguid;




    public bool SetSafeTargetIndex(int index)
    {
        if (m_pathnodes.Count == 0)
        {
            Debug.Log("no pathnodes target to set");
            return false;
        }
        int correctindex = Mathf.Clamp(index, 0, m_pathnodes.Count - 1);
        targetindex = correctindex;

        // array boundaries  
        if (targetindex == m_pathnodes.Count - 1)
        {
            indexbound = ARRAY_BOUND.UP;
            movedir = -1;
        }
        else if (targetindex == 0)
        {
            indexbound = ARRAY_BOUND.DOWN;
            movedir = 1;
        }
        else
            indexbound = ARRAY_BOUND.MIDDLE;

        return true;
    }


    public int GetSafeTargetIndex()
    {
        if (m_pathnodes.Count == 0)
            return -666;
        int correctindex = Mathf.Clamp(targetindex, 0, m_pathnodes.Count - 1);
        targetindex = correctindex;

        // array boundaries  
        if (targetindex == m_pathnodes.Count - 1)
        {
            indexbound = ARRAY_BOUND.UP;
            movedir = -1;
        }
        else if (targetindex == 0)
        {
            indexbound = ARRAY_BOUND.DOWN;
            movedir = 1;
        }
        else
            indexbound = ARRAY_BOUND.MIDDLE;

        return (correctindex > 0) ? correctindex : 0;
    }


    public object GetPathNode(int index)
    {
        // protect the access of pathnodelist 
        int correctindex = Mathf.Clamp(index, 0, m_pathnodes.Count - 1);
        return (object)m_pathnodes[correctindex];

    }

  




}








    [ExecuteInEditMode()]
    public class MovingPlatform : Behavior
    {
        public override string GetClassname(bool shortname=false)
        {
            if (shortname)
                return "MovingPlatform";

            string fullyQualifiedName = typeof(MovingPlatform).AssemblyQualifiedName;
            return fullyQualifiedName;


        }
        public override Dataset GetDataset()
        {
            return (MovingPlatformDataset)paramblock;
        }
        public override void Start()
        {
            m_actor = (Actor)GetComponent(typeof(Actor));						// should be there 
            if (m_actor.GetComponent(typeof(RotatingPlatform)) != null)
                paramblock.b_path_rotation = false;
            base.Start();
        }

        public MovingPlatformDataset paramblock = new MovingPlatformDataset();


#if UNITY_EDITOR
        /// <summary>
        /// viewport callback 
        /// </summary>
        private void OnEnable() { SceneView.onSceneGUIDelegate += OnCustomSceneGUI; }
        private void OnDisable() { SceneView.onSceneGUIDelegate -= OnCustomSceneGUI; }

        void Move(Vector3 dir, bool moveallpath = true)
        {
                transform.position += dir;
        }


        public void UpdatePathnodes(Vector3 v)
        {
            if (paramblock.m_pathnodes.Count != paramblock.maxhandle)
            {
                // maxhandle protection (should be private and accessed through get set)
                paramblock.maxhandle = (paramblock.maxhandle >= 0) ? paramblock.maxhandle : 0;
                if (paramblock.m_pathnodes.Count > paramblock.maxhandle)
                {
                    for (int c = paramblock.m_pathnodes.Count; c > paramblock.maxhandle; c--)
                        paramblock.m_pathnodes.RemoveAt(paramblock.GetSafeTargetIndex());
                }
                for (int c = paramblock.m_pathnodes.Count; c < paramblock.maxhandle; c++)
                {
                    MovingPlatformPathnode pn = new MovingPlatformPathnode();
                    pn.ilookatpoint = 0;
                    pn.pos = v;
                    //m_behavior.paramblock.m_pathnodes.Add(pn);
                    Debug.Log(paramblock.GetSafeTargetIndex().ToString());
                    int localindex = (paramblock.GetSafeTargetIndex() > -1) ? paramblock.GetSafeTargetIndex() : 0;
                    paramblock.m_pathnodes.Insert(localindex, pn);
                }
            }
        }



        public override void DoGUILoop(Rect mainwindow)
        {
            if (paramblock == null)
                return;
            base.DoGUILoop(mainwindow);
            int movepadofset = 0;
            int bsz = 20;
            float ofset = 0.0f;
            paramblock.ismoving = !paramblock.editsub;
            // ========== add limitation to the target index 
            MovingPlatformPathnode targetedpathnode = null;
            if (paramblock.m_pathnodes.Count != 0)
                targetedpathnode = paramblock.m_pathnodes[paramblock.GetSafeTargetIndex()];
            paramblock.b_showdebuginfos = EditorGUILayout.Toggle("show debug infos", paramblock.b_showdebuginfos, GUILayout.MinWidth(280), GUILayout.MaxWidth(280));

            paramblock.editsub = EditorGUILayout.Toggle("EditSub", paramblock.editsub);
            paramblock.move_ampl = EditorGUILayout.Slider("move speed", paramblock.move_ampl, 0, 10, GUILayout.MinWidth(280), GUILayout.MaxWidth(280));
            //---------------------------------------------------------------------------------------------- RESET POSITION 
 

            EditorGUILayout.BeginVertical();
            if (paramblock.GetSafeTargetIndex() > -1)
            {
                targetedpathnode = paramblock.m_pathnodes[paramblock.GetSafeTargetIndex()];
                targetedpathnode.lookatspeed = EditorGUILayout.Slider("lookatspeed", targetedpathnode.lookatspeed, 0, 10.0f, GUILayout.MinWidth(280), GUILayout.MaxWidth(280));
                targetedpathnode.translatespeed = EditorGUILayout.Slider("translatespeed", targetedpathnode.translatespeed, 0, 10.0f, GUILayout.MinWidth(280), GUILayout.MaxWidth(280));
                targetedpathnode.ilookatpoint = (int)EditorGUILayout.Slider("lookat", targetedpathnode.ilookatpoint, 0, 8, GUILayout.MinWidth(280), GUILayout.MaxWidth(280));
                targetedpathnode.waitonnode = (float)EditorGUILayout.Slider("wait", targetedpathnode.waitonnode, 0, 30, GUILayout.MinWidth(280), GUILayout.MaxWidth(280));
                movepadofset += 80;

                if (!paramblock.ismoving)
                {
                    int lkp = targetedpathnode.ilookatpoint;
                    Vector3 targetpos = targetedpathnode.Getlookatpoint(lkp, 1.0f);
                    var Qr = Quaternion.LookRotation(Vector3.up, targetpos);
                    Qr *= Quaternion.Euler(Vector3.forward);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, Qr, 360.0f);
                }
            }
            Actor tmpactor = (Actor) Selection.activeGameObject.GetComponent(typeof( Actor ));

            

            paramblock.b_pathloop = EditorGUILayout.Toggle("loop", paramblock.b_pathloop, GUILayout.MinWidth(280), GUILayout.MaxWidth(280));
            paramblock.b_triggered = EditorGUILayout.Toggle("have trigger", paramblock.b_triggered, GUILayout.MinWidth(280), GUILayout.MaxWidth(280));
            //paramblock.b_showdebuginfos = EditorGUILayout.Toggle("show debug infos", paramblock.b_showdebuginfos, GUILayout.MinWidth(280), GUILayout.MaxWidth(280));
            EditorGUILayout.EndVertical();
            if (paramblock.editsub)
            {
                GUI.BeginGroup(new Rect(0, 100 + movepadofset, mainwindow.width, mainwindow.height));

                //GUI.BeginGroup(new Rect(5, y, 280, 600));
                if (GUI.Button(new Rect(bsz * 2, 0, bsz, bsz), "\"")) //------------------------------- FRONT
                    Move((Vector3.forward * (ofset = (paramblock.b_front_x) ? m_actor.Actorprops.block_size.x : m_actor.Actorprops.block_size.z)), false);
                if (GUI.Button(new Rect(bsz * 2, bsz * 2 , bsz, bsz), ".")) //------------------------------- back
                    Move((Vector3.back * (ofset = (paramblock.b_front_x) ? m_actor.Actorprops.block_size.x : m_actor.Actorprops.block_size.z)), false);
                if (GUI.Button(new Rect(bsz * 1, bsz , bsz, bsz), "<")) //------------------------------- left
                    Move(-(Vector3.left * (ofset = (!paramblock.b_front_x) ? m_actor.Actorprops.block_size.x : m_actor.Actorprops.block_size.z)), false);
                if (GUI.Button(new Rect(bsz * 3, bsz , bsz, bsz), ">")) //------------------------------- right
                    Move(-(Vector3.right * (ofset = (!paramblock.b_front_x) ? m_actor.Actorprops.block_size.x : m_actor.Actorprops.block_size.z)), false);
                if (GUI.Button(new Rect(bsz * 6, 0, bsz, bsz), "\"")) //------------------------------- UP
                    Move((Vector3.up * m_actor.Actorprops.block_size.y), false);
                if (GUI.Button(new Rect(bsz * 6, bsz * 2 , bsz, bsz), ".")) //------------------------------- DOWN
                    Move((Vector3.down * m_actor.Actorprops.block_size.y), false);
                if (GUI.Button(new Rect(bsz * 12, 0, bsz * 2, bsz), ">>")) //------------ NEXT POINT
                {
                    paramblock.SetSafeTargetIndex(paramblock.GetSafeTargetIndex() + 1);
                    //m_behavior.paramblock.targetindex = Mathf.Clamp(m_behavior.paramblock.targetindex+1 , 1 ,m_behavior.paramblock.m_pathnodes.Count) ;
                    targetedpathnode = paramblock.m_pathnodes[paramblock.GetSafeTargetIndex()];
                    //int ti = m_actor.paramblock.targetindex;
                    int lkp = targetedpathnode.ilookatpoint;
                    Vector3 targetpos = targetedpathnode.Getlookatpoint(lkp, 1.0f);
                    var Qr = Quaternion.LookRotation(Vector3.up, targetpos);
                    Qr *= Quaternion.Euler(Vector3.forward);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, Qr, 360.0f);
                    transform.position = targetedpathnode.pos;
                }
                if (GUI.Button(new Rect(bsz * 9, 0, bsz * 2, bsz), "<<")) //------------ NEXT POINT
                {
                    paramblock.SetSafeTargetIndex(paramblock.GetSafeTargetIndex() - 1);
                    targetedpathnode = paramblock.m_pathnodes[paramblock.GetSafeTargetIndex()];
                    int lkp = targetedpathnode.ilookatpoint;
                    Vector3 targetpos = targetedpathnode.Getlookatpoint(lkp, 1.0f);
                    var Qr = Quaternion.LookRotation(Vector3.up, targetpos);
                    Qr *= Quaternion.Euler(Vector3.forward);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, Qr, 360.0f);
                    transform.position = targetedpathnode.pos;
                }
                if (GUI.Button(new Rect(bsz * 12, bsz * 2 , bsz * 2, bsz), "+"))
                {
                    paramblock.maxhandle += 1;
                    UpdatePathnodes(Selection.activeGameObject.transform.position);
                }
                if (GUI.Button(new Rect(bsz * 9, bsz * 2 , bsz * 2, bsz), "-"))
                {
                    paramblock.maxhandle--;
                    UpdatePathnodes(Selection.activeGameObject.transform.position);
                    if (paramblock.GetSafeTargetIndex() > -1)
                        transform.position = paramblock.m_pathnodes[paramblock.GetSafeTargetIndex()].pos;
                }
                GUI.EndGroup();
                
                
            }
        }





        protected void OnCustomSceneGUI(SceneView sceneview)
        {
            if (paramblock == null)
                return;
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
                MovingPlatformPathnode p0 = ( MovingPlatformPathnode ) paramblock.GetPathNode(i);
                MovingPlatformPathnode p1 = ( MovingPlatformPathnode ) paramblock.GetPathNode(i + 1);
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
            if (paramblock.m_pathnodes == null || paramblock.b_path_rotation == false)
                return;
            Gizmos.color = Color.yellow;
            for (int c = 0; c < paramblock.m_pathnodes.Count; c++)
            {
                Gizmos.color = Color.red;
                MovingPlatformPathnode p = (MovingPlatformPathnode)paramblock.m_pathnodes[c];
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
            if (paramblock.m_pathnodes.Count == 0)
                return;
            MovingPlatformPathnode p = paramblock.m_pathnodes[paramblock.GetSafeTargetIndex()];

            Vector3 target = p.pos;
            if (Vector3.Distance(transform.position, target) == 0.0f)
            {
                if (p.timer > 0)
#if UNITY_EDITOR
                    p.timer -= editortick;
#endif
#if !UNITY_EDITOR
                paramblock.pathnodes[paramblock.targetindex].timer -= Time.deltaTime;
#endif
                else
                {
                    p.timer = p.waitonnode;
                    paramblock.SetSafeTargetIndex(paramblock.GetSafeTargetIndex() + paramblock.movedir);
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
                if (paramblock.b_path_rotation)
                    transform.rotation = Quaternion.Slerp(transform.rotation, Qr, (rspeed * editortick));
                //Debug.Log(Vector3.MoveTowards(transform.position, target, tspeed * editortick).ToString());
#endif
#if !UNITY_EDITOR
                transform.position = Vector3.MoveTowards(transform.position, target, tspeed * Time.deltaTime);
                transform.rotation = Quaternion.Slerp(transform.rotation, Qr, (rspeed * Time.deltaTime));
#endif

                switch (paramblock.indexbound)
                {
                    case ARRAY_BOUND.DOWN:
                        paramblock.movedir = 1;
                        break;
                    case ARRAY_BOUND.UP:
                        paramblock.movedir = -1;
                        break;
                }



            }

        }

    }
