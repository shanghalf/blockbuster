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
[XmlInclude(typeof(Pathnode))]
public class MovingPlatformDataset  : Dataset
{

    public Pathnode rotatelookpoint = new Pathnode();
    public float speed = 0.5f;
    public float move_ampl=0.0f;
    public Vector3 target = Vector3.zero;
    public bool b_revert_rotation=false;
    public bool b_triggeronce=false;
    public float rotationspeed=0.0f;
    public int rotationstepnumber=2;
    public bool b_triggered=false;
    //public int rotateindex=0;
    public int movedir = 1;
    public int maxhandle=1;
    //public Vector3[] quater = new Vector3[] { Vector3.forward, Vector3.left, Vector3.back, Vector3.right };
    public bool b_pathloop =false;
    public COLIDER_TYPE colider_type = COLIDER_TYPE.BOX;
    public bool b_path_rotation = true;
    public List<Pathnode> m_pathnodes = new List<Pathnode>();   // pathnodes array for any path 


    public override List<Pathnode> GetPathNodes()
    {
        return m_pathnodes;
    }

    public override bool SetSafeTargetIndex(int index)
    {
        if (m_pathnodes.Count == 0)
            return false;
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


    public override int GetSafeTargetIndex()
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




    public override Dataset Load(string path)
    {
        if (!System.IO.File.Exists(path))
            return null;
        XmlSerializer serializer = new XmlSerializer(typeof(MovingPlatformDataset));
        Stream stream = new FileStream(path, FileMode.Open);
        MovingPlatformDataset result = serializer.Deserialize(stream) as MovingPlatformDataset;
        stream.Close();
        return result;
    }



}








    [ExecuteInEditMode()]
    public class MovingPlatform : BBehavior
    {
        // paramblock is a custom data set that hold serializables 
        // properties of a behavior 
        public MovingPlatformDataset paramblock = new MovingPlatformDataset();

        // for convenient  used in bbehavior and actor should be unified 
        
        private Vector3 left = new Vector3(-1.0f, 0.0f, 0.0f);
        private Vector3 front = new Vector3(0.0f, 0.0f, 1.0f);
        private Vector3 right = new Vector3(1.0f, 0.0f, 0.0f);
        private Vector3 back = new Vector3(0.0f, 0.0f, -1.0f);
        private bool b_front_x;
        

        /// <summary>
        /// setup initial condition of this specific behavior
        /// </summary>
        public override void Start()
        {
            // for example if the acor running this behavior got 
            // a rotation effect from another behavior it s better to desactivate the 
            // look at function on pathnodes

            m_actor = (Actor)GetComponent(typeof(Actor));						// should be there 
            if (m_actor.GetComponent(typeof(RotatingPlatform)) != null)
                paramblock.b_path_rotation = false;
            base.Start();
        }

      
  
        
        /// <summary>
        /// param bock is a field and not a properie 
        /// defined in another class cause monobehaviours are not serializable 
        /// so all relevant properties are stored in a custom class derivated instance 
        /// that allow the serialization hope unity will fix this huge problem asap 
        /// </summary>
        /// <returns></returns>
        public override Dataset GetDataset()
        {
            return paramblock;
        }
        /// <summary>
        /// same comment accessor to the custom class instance field 
        /// </summary>
        /// <param name="D"></param>
        /// 
        public override void SetDataset(object o)
        {
            
            paramblock =(MovingPlatformDataset) o;
        }
  



#if UNITY_EDITOR
        /// <summary>
        /// register a custom display callback
        /// </summary>


        /// <summary>
        /// this is related to the movepad in the moving platform
        /// behaviour should be changed for a common move 
        /// the editor tool blockbuster got a nice one related to camera orientation 
        /// i should move it in a utility class but right now 
        /// behaviors are not in editor namespace and cannot see BB function 
        /// 
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="moveallpath"></param>
        void Move(Vector3 dir, bool moveallpath = true)
        {
            // <todo> a lot  
            transform.position += dir;
        }

        /// <summary>
        /// update pathnodes make sure that maxhandle is the size of the pathnodes array 
        /// grow and add at V pos or remove at the same place 
        /// </summary>
        /// <param name="v"></param>
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
                    Pathnode pn = new Pathnode();
                    pn.ilookatpoint = 0;
                    pn.pos = v;
                    //m_behavior.paramblock.m_pathnodes.Add(pn);
                    Debug.Log(paramblock.GetSafeTargetIndex().ToString());
                    int localindex = (paramblock.GetSafeTargetIndex() > -1) ? paramblock.GetSafeTargetIndex() : 0;
                    paramblock.m_pathnodes.Insert(localindex, pn);
                }
            }
        }

        [BBCtrlVisible] // define a function visible for BBControl 
        public int  TestForLinBlink(float ftest, string stringtest )
        {
            return 1;
        }

        /// <summary>
        /// crap this is defined in several point but not in the same namespace
        /// editor for block move and for movepad 
        /// should find a way to simplify 
        /// and share the logic without adding useless value to paramblock base 
        /// </summary>
        /// 
        [BBCtrlVisible] // define a function visible for BBControl 
        public  void GetDir()
        {

            if (SceneView.currentDrawingSceneView == null) return;
            Transform cam = SceneView.currentDrawingSceneView.camera.transform;
            Vector3 flatcamvector = new Vector3(cam.forward.x, 0.0f, cam.forward.z);
            float AF = Vector3.Angle(flatcamvector, Vector3.forward);
            float AB = Vector3.Angle(flatcamvector, Vector3.back);
            float AL = Vector3.Angle(flatcamvector, Vector3.left);
            float AR = Vector3.Angle(flatcamvector, Vector3.right);

            float[] anglearray = new float[] { AF, AB, AL, AR };

            System.Array.Sort(anglearray);
            if (AF == anglearray[0])
            {
                front = Vector3.forward;
                back = Vector3.back;
                left = Vector3.right;
                right = Vector3.left;
                b_front_x = false;
            }
            if (AB == anglearray[0])
            {
                front = Vector3.back;
                back = Vector3.forward;
                left = Vector3.left;
                right = Vector3.right;
                b_front_x= false;
            }
            if (AL == anglearray[0])
            {
                front = Vector3.left;
                back = Vector3.right;
                left = Vector3.forward;
                right = Vector3.back;
                b_front_x = false;
            }
            if (AR == anglearray[0])
            {
                front = Vector3.right;
                back = Vector3.left;
                left = Vector3.back;
                right = Vector3.forward;
                b_front_x = false;
            }


        }


        /// <summary>
        /// this is the Blockbuster GUI loop for this behavior 
        /// implementing user interface at the same level 
        /// make it easy to provide update , interface and custom data at the same place 
        /// pick up the UI logic here 
        /// </summary>
        /// <param name="mainwindow"></param>
        public override void DoGUILoop(Rect mainwindow)
        {
            GetDir();
            if (paramblock == null)
                return;
            // catch a handle on the host actor for this behavior 
            Actor tmpactor = (Actor)Selection.activeGameObject.GetComponent(typeof(Actor));
            int movepadofset = 0; // for the movepad ( temporary solution ill make a better thing here ) 
            int bsz = 20; // that roughly the unit size for movepad layout (crap i ll do something better soon) 
            float ofset = 0.0f;
            paramblock.ismoving = !paramblock.editsub;
            // ========== add limitation to the target index 
            Pathnode targetedpathnode = null;
            if (paramblock.m_pathnodes.Count != 0)
                targetedpathnode = paramblock.m_pathnodes[paramblock.GetSafeTargetIndex()];
            paramblock.b_hideedition = EditorGUILayout.Toggle("hide path", paramblock.b_hideedition);
            if (paramblock.b_hideedition == false )
                paramblock.b_showdebuginfos = EditorGUILayout.Toggle("show debug infos", paramblock.b_showdebuginfos);
            paramblock.editsub = EditorGUILayout.Toggle("EditSub", paramblock.editsub);
            paramblock.move_ampl = EditorGUILayout.Slider("move speed", paramblock.move_ampl, 0, 10);
            EditorGUILayout.BeginVertical();
            if (paramblock.GetSafeTargetIndex() > -1)
            {
                targetedpathnode = paramblock.m_pathnodes[paramblock.GetSafeTargetIndex()];
                targetedpathnode.lookatspeed = EditorGUILayout.Slider("lookatspeed", targetedpathnode.lookatspeed, 0, 10.0f);
                targetedpathnode.translatespeed = EditorGUILayout.Slider("translatespeed", targetedpathnode.translatespeed, 0, 10.0f);
                targetedpathnode.ilookatpoint = (int)EditorGUILayout.Slider("lookat", targetedpathnode.ilookatpoint, 0, 8);
                targetedpathnode.waitonnode = (float)EditorGUILayout.Slider("wait", targetedpathnode.waitonnode, 0, 30);
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
            paramblock.b_pathloop = EditorGUILayout.Toggle("loop", paramblock.b_pathloop, GUILayout.MinWidth(280), GUILayout.MaxWidth(280));
            paramblock.b_triggered = EditorGUILayout.Toggle("have trigger", paramblock.b_triggered, GUILayout.MinWidth(280), GUILayout.MaxWidth(280));
            //paramblock.b_showdebuginfos = EditorGUILayout.Toggle("show debug infos", paramblock.b_showdebuginfos, GUILayout.MinWidth(280), GUILayout.MaxWidth(280));
            EditorGUILayout.EndVertical();
            if (paramblock.editsub)
            {
                EditorGUILayout.BeginHorizontal();
                // this syntax is gdamned compact but everything else look like a novel 
                if (GUILayout.Button( "FRONT")) //------------------------------- FRONT
                    Move((front * (ofset = (paramblock.b_front_x) ? m_actor.Actorprops.block_size.x : m_actor.Actorprops.block_size.z)), false);
                if (GUILayout.Button("BACK")) //------------------------------- back
                    Move((back * (ofset = (paramblock.b_front_x) ? m_actor.Actorprops.block_size.x : m_actor.Actorprops.block_size.z)), false);
                if (GUILayout.Button("LEFT")) //------------------------------- right
                    Move((right * (ofset = (!paramblock.b_front_x) ? m_actor.Actorprops.block_size.x : m_actor.Actorprops.block_size.z)), false);
                if (GUILayout.Button("RIGHT")) //------------------------------- left
                    Move((left * (ofset = (!paramblock.b_front_x) ? m_actor.Actorprops.block_size.x : m_actor.Actorprops.block_size.z)), false);
                if (GUILayout.Button("UP")) //------------------------------- UP
                    Move((Vector3.up * m_actor.Actorprops.block_size.y), false);
                if (GUILayout.Button("DOWN")) //------------------------------- DOWN
                    Move((Vector3.down * m_actor.Actorprops.block_size.y), false);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("<< goto")) //------------ NEXT POINT
                {
                    paramblock.SetSafeTargetIndex(paramblock.GetSafeTargetIndex() + 1);
                    targetedpathnode = paramblock.m_pathnodes[paramblock.GetSafeTargetIndex()];
                    // orient in the lookat point dir of the pathnode lookatpoint (360/8 deg step) 
                    int lkp = targetedpathnode.ilookatpoint;
                    Vector3 targetpos = targetedpathnode.Getlookatpoint(lkp, 1.0f);
                    var Qr = Quaternion.LookRotation(Vector3.up, targetpos);
                    Qr *= Quaternion.Euler(Vector3.forward);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, Qr, 360.0f);
                    transform.position = targetedpathnode.pos;
                }
                if (GUILayout.Button("goto >>")) //------------ NEXT POINT
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
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("pathnode +"))
                {
                    paramblock.maxhandle += 1;
                    UpdatePathnodes(Selection.activeGameObject.transform.position);
                }
                if (GUILayout.Button("pathnode -"))
                {
                    paramblock.maxhandle--;
                    UpdatePathnodes(Selection.activeGameObject.transform.position);
                    if (paramblock.GetSafeTargetIndex() > -1)
                        transform.position = paramblock.m_pathnodes[paramblock.GetSafeTargetIndex()].pos;
                }
                EditorGUILayout.EndHorizontal();
            }
            
            
        }

        /// <summary>
        ///  display paramblock.m_pathnodes 
        ///  in viewport 
        /// </summary>
        /// <param name="sceneview"></param>
        public  override void OnCustomSceneGUI(SceneView sceneview)
        {
            if (paramblock == null ) 
                return ;
            if(paramblock.m_pathnodes.Count == 0 || paramblock.b_hideedition )
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
                Pathnode p0 = ( Pathnode ) paramblock.GetPathNode(i);
                Pathnode p1 = ( Pathnode ) paramblock.GetPathNode(i + 1);
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
                Pathnode p = (Pathnode)paramblock.m_pathnodes[c];
                Vector3 lookatpoint = p.Getlookatpoint(p.ilookatpoint, 1.0f) + p.pos;
                Gizmos.DrawSphere(lookatpoint, 0.1f);
                Gizmos.DrawLine(lookatpoint, p.pos);

            }

        }

        [BBCtrlVisible] // define a function visible for BBControl 
        public void Movpltftest(string t1, bool t2, int t3)
        { 
        
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
            Pathnode p = paramblock.m_pathnodes[paramblock.GetSafeTargetIndex()];

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




/*
        if (paramblock.editsub)
            {
                
                // this syntax is gdamned compact but everything else look like a novel 
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
                // movepad navigation into pathnodes 
                if (GUI.Button(new Rect(bsz * 12, 0, bsz * 2, bsz), ">>")) //------------ NEXT POINT
                {
                    paramblock.SetSafeTargetIndex(paramblock.GetSafeTargetIndex() + 1);
                    targetedpathnode = paramblock.m_pathnodes[paramblock.GetSafeTargetIndex()];
                    // orient in the lookat point dir of the pathnode lookatpoint (360/8 deg step) 
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
                // add or remove a pathnode 
                // size is drivent by maxhandle value and modified accordingly by updatepathnodes function 
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
                
            }
 */