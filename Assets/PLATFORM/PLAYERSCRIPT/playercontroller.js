#pragma strict

public var  agent : NavMeshAgent ;
public var Target : GameObject ; 
public var mainplayercam : Camera ;
public var playergrouphandle : GameObject ; 
public var camposdebug : GameObject; 
public var camfolowspeed : float = 0.5 ; 
public var Ofset : Vector3 ;
public var showdebugobjects : boolean = false ;
private var ray :Ray ;
private var p = ray.GetPoint(0.0);
private var hit : RaycastHit;

function Start () 
{
	agent.destination = transform.position;
	
	if ( showdebugobjects )
	{
		var prefab = Resources.LoadAssetAtPath( ("Assets/PLATFORM/Editor/target.fbx") , GameObject); // need fbx in editor folder 
		if (prefab != null ) 
		{
			camposdebug = Instantiate(prefab,transform.position ,Quaternion.identity   );
			camposdebug.name = "CAMPOS";
		}
		else 
		 	Debug.Log("cam target fbx not in editor folder check this ");	
	}
		
}

function Update () 
{
	ray  = mainplayercam.ScreenPointToRay (Input.mousePosition);
	if ( Input.GetMouseButtonDown(1) == true ) 
	Physics.Raycast(ray, hit) ;
	agent.destination = hit.point ;//Target.transform.position;
	mainplayercam.transform.position.x =  transform.position.x - Ofset.x ; 
	mainplayercam.transform.position.y =  transform.position.y - Ofset.y ; 
	mainplayercam.transform.position.z =  transform.position.z - Ofset.z ;   
	mainplayercam.transform.position = Vector3.Slerp( mainplayercam.transform.position,mainplayercam.transform.position , Time.deltaTime * camfolowspeed ) ;
	mainplayercam.transform.LookAt(this.transform);
}



