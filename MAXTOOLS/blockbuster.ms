
	--**************************************************************************************************************
	-- EDIT THIS LINES WITH YOUR OWN PATH *** there s actually no ini file to hold your settings that s for hardcore artist only 
	-- todo split path in several sub part to compose path on flighr more easily 
	
	
	global maxbasename = 	
	
	
	
	#( "E:\\atrabuio_SGHW-ATRABUIO_NOROTATOR_9469\\MAXTOOLS\\base\\HIGHTECH.max"	, 
		"E:\\atrabuio_SGHW-ATRABUIO_NOROTATOR_9469\\MAXTOOLS\\base\\JUNGLE.max",
		"E:\\atrabuio_SGHW-ATRABUIO_NOROTATOR_9469\\MAXTOOLS\\base\\TEMPLE.max",
		"E:\\atrabuio_SGHW-ATRABUIO_NOROTATOR_9469\\base\\SANDBOX.max"
		)
	global colbasename = 	#( "D:\\atrabuio_SGHW-ATRABUIO_NOROTATOR_9469\\MAXTOOLS\\base\\orientalcolset.max")
		
		
	global exportpath = "E:\\atrabuio_SGHW-ATRABUIO_NOROTATOR_9469\\Assets\\PLATFORM\\HIGHTECH\\"

		
		
	
	global 	ininame = "blockbuster6.ini" 
		
	--**************************************************************************************************************
	--**************************************************************************************************************
	--**************************************************************************************************************
	--**************************************************************************************************************
	--e:\alf_rot

	
rollout batchrename "Batchrename" width:205 height:152
(
	dropdownList ddl1 "change instance name " pos:[7,7] width:189 height:41 items:#("JUNGLE", "TEMPLE", "HIGHTECH", "SANDBOX")
	button closebutton "close" pos:[7,112] width:186 height:20 toolTip:"close the window"
	
	progressBar pb1   "ProgressBar" pos:[10,55] width:181 height:17
	
	global keepgoing = true 
	global cmax = 0
	global newname = ""
	global bindex  =  1
	global pbinc = 0

	button renamebutton "rename" pos:[6,83] width:186 height:20 toolTip:"close the window"
	
	on ddl1 selected sel do
	(
		bindex = sel 
	)
		
	
	fn updatepb c  = 
	(
		pb1.value = pbinc * c ;
		windows.processPostedMessages()
	)
	
	on closebutton pressed do
	(
		destroydialog batchrename
	)
	
	
	on pb1 rightClick  do
	(
		keepgoing = false 
		pb1.value = 50
		
	)
	
	on renamebutton pressed do
	(
		if ( $ == undefined ) do 
		(
			messagebox "no selection "
			return false 
		)
		if ($ == selection ) do 
				cmax = $.count 
		
		pbinc = 100.0 /( cmax *2 ) 
		count = 0 
		for n in $  do 
		(
			count +=1 
			
			s = n.name as string 
			ns = filterstring s "_" 
			ns[1] =   ddl1.items[bindex]
			n.name ="" 
			for  ss in ns do
			(
				n.name +=  ss  
				if ( ss != ns[ns.count] ) do  n.name +=    "_" 
			)
			
			if ( keepgoing == false ) do  
			return false
			--updatepb  ( pbinc * count ) 
		
			
		pb1.value = pbinc * count ;
	
		)
			
		
			for n in $  do 
		(
			count +=1 	
			s =  getuserprop  n "originalname"
			
			ns = filterstring s "_" 
			ns[1] =   ddl1.items[bindex]
			newname ="" 
			for  ss in ns do
			(
				newname +=  ss  
				if ( ss != ns[ns.count] ) do  newname +=    "_" 
			)
			
			if ( keepgoing == false ) do 
			return false
			
			pb1.value = pbinc * count ;
			
			
			setuserprop n "originalname" newname
		)
		
		messagebox "done"
		
	)
)


		
	rollout enterclientpath "Enter Client Path" width:352 height:88
	(	
	editText clientpathtext "" pos:[8,16] width:336 height:22
	button cancelbutton  "cancel" pos:[128,48] width:99 height:22
	button okbutton  "OK" pos:[240,48] width:99 height:22
		
	
	

	on okbutton pressed    do 
	(
		messagebox (clientpathtext.text)
		setINISetting ( GetDir #userScripts + "/3dsmax.ini")  "LOCALPATH" "mylocalpath"clientpathtext.text 
		destroydialog enterclientpath
	)
	
	on cancelbutton pressed   do 
	(
		destroydialog enterclientpath
	)
	
		
	)	
		
	--**************************************************************************************************************
	--**************************************************************************************************************
	--**************************************************************************************************************
	--**************************************************************************************************************
	

	struct subblockdesc ( blockfilepath , objectname , blocktransform )
 
	global p = [0, 0, 0]
	global np = [0, 0, 0]
	global cam = 0
	global sel = #()
	global dir ="up"
	global basenamearray = #()
	global currentobjectindex = 1 
	global left= point3 0 0 0
	global modulesize  = point3 0 0 0 
	global front = point3 0 0 0	
	global savepos = point3 0 0 0
	global step = 1
	global steptable = #(0.01,0.1,0.5,1.0,1.5,2.0,2.5,3.0,3.5,4.0)	
	global movestepvalue =0.5
	global basenamenumber = 1
	global  subblocksdescarray = #() 
	global baseitems = #("ORI blocks")
	
	fn _sel  =
	(
		if $== undefined do return false
		currentselection =#()
		if $ != selection 
			then  
				append currentselection  $
			else 
				currentselection = $
		currentselection
	)
	


	fn getViewDirectionRay =

	(
		local coordSysTM = Inverse(getViewTM())
		local viewDir = -coordSysTM.row1
		local viewPt = coordSysTM.row4
		p=  ray viewPt viewDir
		return normalize ( p.dir) 
		
	)
	
	fn getpos s =
	(
		if s == undefined do return false 
		if s == selection then  
			return s[1].pos 
		else 
			try ( return s.pos  ) catch (return false)
	)

	fn getrotation s =
	(
		if s== undefined do return false 
		if s == selection then  
			return s[1].rotation 
		else 
			try ( return s.rotation ) catch (return false)
	)

	
	fn setpos s pos =
	(
		if s== undefined do return false 
			if s == selection then  
				for n in s do 
					n.pos = pos
				else 
					s.pos+= pos
	)
	
		fn dorotate sel rotatematrix =
	(
		if sel == undefined do return false 
		if sel != selection do 
		sel=#(sel)	
		
		originalpos= sel[1].pos
		for n in sel  do 
			(
				n.pos-=originalpos
				originaltransform = n.transform --n.rotation as matrix3	
				finaltransform = originaltransform * rotatematrix
				n.transform = finaltransform 
				n.pos+=originalpos
				setuserprop n "transform" n.transform
			)
			getmodelsize $
		
	)
	fn domove s  pos =
	(
		if s == false do 
			return false 
		if s.count < 1 do 
			return false 	
		move s    (  pos  )
		for n in s do 
			setuserprop n "transform" n.transform
	)
	

	fn getaxis =  
	(
		--n = getnodebyname "n"
		xp  = point3  	 1 	0 		0
		xm = point3  	-1 	0 		0
		yp = point3  	 0 	1 		0
		ym = point3  	 0 	-1 	0
		tm = viewport.getTM()
		np = getViewDirectionRay() 
		
		fxp = distance np xp
		fxm= distance np xm 
		fyp = distance np yp 
		fym=distance np ym  
		
		distancetable = #(fxp,fxm,fyp,fym)
		sort distancetable
		case distancetable[1] of 
		
		(
			fxp: 
			(
				left = xp
				front = ym
			)
			fxm:
			(
				left = xm
				front = yp
			)
			fyp:
			(
				left = yp
				front = xp
			)
			fym:
			(
				left = ym
				front = xm
			)
			default : return false
		)
	)

	-- calculate global bbox from selected node and from size 
fn calcselbboxfromsize  n = 
(
	s = point3 0 0 0 
	xt=#()
	yt=#()
	zt=#()
	if n == selection do 
		for c in n do 
		(
			b = nodeLocalBoundingBox c	
		
			xt[xt.count+1]= b[1].x	
			xt[xt.count+1]= b[2].x	
			
			yt[yt.count+1]= b[1].y	
			yt[yt.count+1]= b[2].y	
			
			zt[zt.count+1]= b[1].z	
			zt[zt.count+1]= b[2].z	
		)
	sort xt 
	sort yt
	sort zt 
	s.x= abs ( xt[1] - xt[xt.count] )
	s.y= abs ( yt[1] - yt[yt.count] )
	s.z= abs ( zt[1] - zt[zt.count] )
	return s
)

fn DeleteMasterObject =
(
	n = callbacks.notificationParam()
	if ( ( getuserprop n "originalname"  == n.name )  and  ( ( getuserprop n "oktodelete")  == undefined ) ) do 
		b = querybox ( "you are about to delete a master object are you sure " )
	
	if ( b == false  ) do 
		(
			buf = getuserpropbuffer n
			inst =  instance n 
			setuserpropbuffer inst buf
			inst.transform =  n.transform 
			inst.name = n.name
			inst.wirecolor = n.wirecolor
			select inst
			
		)
)


	
		fn getmodelsize n =
		(
			--if $ == undefined do return false
				
			bb = #( point3 0 0 0 , point3 0 0 0  )
			
			x=0
			y=0
			z=0
			
			if ( n == selection ) then 
			(
				size = calcselbboxfromsize n
				x=  	floor( abs( size.x+0.5	) ) 	as integer	
				y= 	floor( abs( size.y+0.5	) ) 	as integer
				z= 	floor( abs( size.z+0.5	) )	as integer
			)
			else
			( 
				bb= nodeLocalBoundingBox n
				x =   getuserprop n "xbase"
				y =   getuserprop n "ybase"
				z =   getuserprop n "zbase"
			)
			
			-- size from bbox or selection size return aprox that fit the grid
				
			if classof x == integer then 	
				MainRollout.xbasespinner.value = x 
			else
				MainRollout.xbasespinner.value = 	floor (	abs ( ( bb[2].x- bb[1].x  	) ) + 0.5 	) 
			if classof y == integer  then 	
				MainRollout.ybasespinner.value = y 
			else
				MainRollout.ybasespinner.value =  floor (	abs ( ( bb[2].y- bb[1].y	) ) +0.5 	)
			if classof z  == integer then 	
				MainRollout.zbasespinner.value = z 
			else
				MainRollout.zbasespinner.value = 	floor (	abs ( ( bb[2].z- bb[1].z	) ) +0.5		)
			
			-- for naming function 
			MainRollout.nmxed.text = ( MainRollout.xbasespinner.value as string ) 
			MainRollout.nmyed.text = ( MainRollout.ybasespinner.value as string ) 
			MainRollout.nmzed.text = ( MainRollout.zbasespinner.value as string ) 
			
		)
		

	
		 fn SelectCallBack = 
		 (
			 
			 if ( $ == undefined ) do 
				return false 
			 
			 getmodelsize $
			
			-- todo : change this to mannage the new master object method
			 
			 if (MainRollout.selectinstancechk.checked ==true and $ != selection ) do 
			 (
				 max create mode
				 objname = getuserprop $ "fromobjectnamed"
				for nn in $objects do 
					if ( getuserprop nn "fromobjectnamed" == objname) do 
						selectmore nn
				MainRollout.selectinstancechk.checked =false
			)
			
			
			for n in ( _sel()) do 
			(	 
				if ( ( getuserprop n "coumpoundblock" ) == true ) then 
				(
					MainRollout.mergeblockbutton.enabled = false
					MainRollout.unmergeblock.enabled = true
				)
				else 
				(  -- check if blocks could be merged 
					MainRollout.mergeblockbutton.enabled = true 
					MainRollout.unmergeblock.enabled = false
				)
			)
			
					
					
					
		)
			

rollout MainRollout "Untitled" width:401 height:536
(
	pickbutton pickblockbutton "pick" pos:[31,126] width:40 height:18
	button centerbutton "" pos:[50,49] width:21 height:20
	button frontbutton "" pos:[50,25] width:21 height:20
	button rightbutton "" pos:[74,48] width:21 height:20
	button backbutton "" pos:[50,73] width:21 height:20
	button leftbutton "" pos:[26,49] width:21 height:20
	button upbutton "" pos:[103,25] width:21 height:20
	button downbutton "" pos:[103,72] width:21 height:20
	button prevobjectbutton "<<" pos:[31,102] width:40 height:20
	button nextobjectbutton ">>" pos:[79,102] width:40 height:20
	button rotatexbutton "x" pos:[34,172] width:21 height:20
	button rotateybutton "y" pos:[66,172] width:21 height:20
	button rotatezbutton "z" pos:[98,172] width:21 height:20
	button duplicatebutton "use" pos:[79,126] width:40 height:18
	spinner xbasespinner "x" pos:[153,25] width:100 height:16 range:[1,100,0] type:#integer 
 
 
 
 
 
 
 
 
 
 
	spinner ybasespinner "y" pos:[153,49] width:100 height:16 range:[1,100,0] type:#integer 
 
 
 
 
 
 
 
 
 
 
	spinner zbasespinner "z" pos:[153,73] width:100 height:16 range:[1,100,0] type:#integer 
 
 
 
 
 
 
 
 
 
	GroupBox grp5 "move selection" pos:[269,7] width:112 height:141
	GroupBox grp6 "rotate selection" pos:[21,153] width:112 height:63
	GroupBox grp7 "move" pos:[21,8] width:112 height:141
	GroupBox grp1 "block size" pos:[137,7] width:127 height:141
	GroupBox grp17 "naming convention" pos:[21,296] width:259 height:209
	GroupBox grp8 "blocks type for layer" pos:[138,246] width:243 height:48
	GroupBox grp16 "layers" pos:[21,217] width:112 height:77
	GroupBox grp9 "misc" pos:[283,296] width:109 height:209
	dropdownList iterddl "" pos:[167,388] width:45 height:22 items:#("01", "02", "03", "04", "05", "06")
	dropdownList iterletterddl "" pos:[223,388] width:45 height:22 items:#("A", "B", "C", "D", "E", "F", "G", "H", "I", "J")
	dropdownList baseselddlist "active base" pos:[269,164] width:103 height:41 items:#("HIGHTECH", "JUNGLE", "TEMPLE", "SANDBOX")
	dropdownList propddlist "" pos:[261,263] width:103 height:22 items:#("ldblock", "lablock") selection:2
	dropdownList nmmapddl "" pos:[167,309] width:103 height:22 items:#("HIGHTECH", "JUNGLE", "TEMPLE", "COMON", "SANDBOX")
	dropdownList nmtypeddl "" pos:[168,336] width:103 height:22 items:#("BUILD", "VEG", "PROP", "DECO", "PLUG", "MISC", "COL")

	button setmodulesizebutton "set custom size" pos:[151,103] width:101 height:18
	button removecustomsizebutton "remove custom size" pos:[151,125] width:101 height:18
	button stepmovebutton "" pos:[277,103] width:95 height:18
	button movefrontbutton "" pos:[302,26] width:21 height:20
	button moverightbutton "" pos:[326,50] width:21 height:20
	button movebackbutton "" pos:[302,74] width:21 height:20
	button moveleftbutton "" pos:[278,50] width:21 height:20
	button moveupbutton "" pos:[351,26] width:21 height:20
	button movedownbutton "" pos:[351,73] width:21 height:20
	button mergeblockbutton "merge blocks" pos:[152,172] width:101 height:18 enabled:false
	button setpropbutton "set block prop" pos:[149,265] width:101 height:18
	button resetblocktransformbutton "reset" pos:[277,125] width:95 height:18
	button isolaterefblockbutton "isolate reference block" pos:[260,217] width:115 height:18
	button unmergeblock "split" pos:[153,194] width:100 height:18
	button addnamebutton "add" pos:[170,415] width:96 height:18
	button batchexport "BATCH EXPORT" pos:[288,480] width:96 height:21 --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
 --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
 --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
 --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
	button clearinifile "clear ini file " pos:[288,456] width:96 height:13
	button gencolbutton "gen col" pos:[288,384] width:96 height:18 enabled:false
	button cleanrayfiremesh "clean merged" pos:[288,432] width:96 height:18
	button newscenebutton "new scene" pos:[288,408] width:96 height:18
	button changenamebutton "Create Name" pos:[170,457] width:96 height:18
	button tounity "TO UNITY" pos:[288,320] width:96 height:18 enabled:true
	button generateblocksfromtextfile "generate" pos:[170,436] width:96 height:18

	checkbox globalrotcheck "global rotation" pos:[34,197] width:88 height:16 checked:true
	checkbox ldlayerchkbox "col" pos:[32,240] width:40 height:24 enabled:false checked:true
	checkbox lalayerchkbox "viz" pos:[32,264] width:37 height:21 enabled:false checked:true
	checkbox selectinstancechk "selectinstanecs" pos:[151,214] width:98 height:23 checked:false

	edittext listname "" pos:[24,312] width:135 height:183
	edittext nmxed "" pos:[166,361] width:27 height:20
	edittext nmyed "" pos:[198,361] width:27 height:20
	edittext nmzed "" pos:[230,361] width:27 height:20


--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
 --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
 --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
 --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
 --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
 --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------



		
		fn generateassetlist  =
		(
			
			if  ( doesfileexist ( exportpath+"AssetList.xml" ))  then
				(
					fstr= openfile 	( exportpath+"AssetList.xml" ) mode:"w+"
					print "file exist open "
				)
			else 	
				fstr = 	createFile ( exportpath+"AssetList.xml" ) 

			str = "<AssetsList>\n"
			format ( str ) to:  fstr
			arr = getfiles ( exportpath+"*.fbx" ) 
			for n in arr do 
			(
				str = "	<name>" + getfilenamefile n + "</name>\n"  
				format ( str ) to:  fstr
			)
			str = "</AssetsList>\n"
			format ( str ) to:  fstr
			
			dest = exportpath+"UassetList.xml"
			
			
			--local contents = (dotNetClass "System.IO.File").ReadAllLines ( exportpath+"AssetList.xml" ) 

			
			
		--( dotNetClass "System.IO.File" ).AppendAllLines dest  str 
			
			flush fstr
			close fstr 
			print "closed"
			
			
			
			--Ucopyfile   ( exportpath+"AssetList.xml" ) ( exportpath+"UAssetList.xml" ) 
			
			
		)	

		
		
		

	
	
	
	
	
	
	

	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	

	
	
	
	
	
	
	
	
	
	------------------------------------------------------------------------------------------------------------------------------- 
	------------------------------------------------------------------------------------------------------------------------------- 
	------------------------------------------------------------------------------------------------------------------------------- 
	------------------------------------------------------------------------------------------------------------------------------- 
	

	button batchrenamebutton "batch rename" pos:[171,479] width:96 height:18
	button hidebutton "hide" pos:[289,347] width:46 height:18 enabled:true
	button unhidebutton "unhide" pos:[337,347] width:43 height:18 enabled:true
	on MainRollout open do
	(
		
		callbacks.addScript #selectionSetChanged "SelectCallBack()"
		
		
		callbacks.addScript  #nodePreDelete "DeleteMasterObject()" 
		
		stepmovebutton.caption = ( steptable[step] as string ) 
		
		tmpstr = GetINISetting  ( GetDir #userScripts + "/3dsmax.ini")  "LOCALPATH" "mylocalpath"  
				
				if ( tmpstr == "") then  
					createdialog  enterclientpath 
				else 
				(
					--messagebox  tmpstr
					if ( doesfileexist tmpstr ) then 
						clientpath =  tmpstr
					else 
						createdialog  enterclientpath 
				)
				basenamearray = getMAXFileObjectNames  ( clientpath +"\\"+ maxbasename[1] )
	)
	on MainRollout close do
	(
		callbacks.removescripts #selectionSetChanged 
		callbacks.removescripts #nodePreDelete 
		if $== undefined do return false 
		try ( delete cbox ) catch ()
		max unhide all
		
	)
	on pickblockbutton picked obj do
	(
		if $== undefined do return false 
		b = getuserpropbuffer obj
		sel = $
		for it in (sel) do 
		(
				trs = it.transform
				n =  instance obj 
				n.transform=trs
				setuserpropbuffer n b
				setuserprop n "transform" n.transform
				print obj.name
				n.name =  ( getuserprop obj "originalname" ) +"_" + ( n.handle as string )
				n.wirecolor = color 115 115 115
		)
		delete sel 
	)
	on centerbutton pressed do
	(
		if $== undefined do return false 
		for n in (_sel() )do 
			(		
					buftext= getUserPropBuffer n
					inst = instance n
					setUserPropBuffer inst buftext
					setuserprop n "transform" n.transform
					inst.name = n.name 
					inst.wirecolor = color 115 115 115
					inst.name =  ( getuserprop inst "originalname" ) +"_" + ( inst.handle as string )
			)
	)
	on frontbutton pressed do
	( 
		getaxis()
		domove (_sel())  ( point3 (front.x*xbasespinner.value) (front.y*ybasespinner.value) 0 )
	)
	on rightbutton pressed do
	(	
		getaxis()
		domove (_sel())  ( point3 (-left.x*xbasespinner.value) (-left.y*ybasespinner.value) 0 )
	)
	on backbutton pressed do
	(		
		getaxis()
		domove (_sel())  ( point3 (-front.x*xbasespinner.value) (-front.y*ybasespinner.value) 0 )
	)
	on leftbutton pressed do
	(
		getaxis() -- retunr axis normalized vector 
		domove (_sel())  ( point3 (left.x*xbasespinner.value) (left.y*ybasespinner.value) 0 )
	)
	on upbutton pressed do
	(
		domove (_sel())  ( point3 0 0 (1* zbasespinner.value ) ) 
	)
	on downbutton pressed do
	(
		domove (_sel())  ( point3 0 0 (-1* zbasespinner.value ) ) 
	)
	on prevobjectbutton pressed do
	(
		max create mode
		currentobjectindex  -=1
		currentobjectindex  = ( mod  currentobjectindex basenamearray.count ) 
		if currentobjectindex <1 do currentobjectindex =basenamearray.count
		m = mergemaxfile  (  maxbasename[basenamenumber] )  #(basenamearray[currentobjectindex]) #useSceneMtlDups #skipDups 
		m= getnodebyname  basenamearray[currentobjectindex]
		if ( getrotation $ != false ) do 
			m.rotation = getrotation $
		if ( getpos $ == false ) 
		then 
			m.pos = savepos 
		else 
			m.pos = getpos $ 
		if ( $ != undefined ) do 
		(
			setuserprop $ "oktodelete" true
			delete $ 
		)
		select m
		setuserprop $ "frombasefile" ( maxbasename[basenamenumber] )
		setuserprop $ "fromobjectnamed" basenamearray[currentobjectindex]
		setuserprop $ "transform" $.transform
		print currentobjectindex
		
	)
	on nextobjectbutton pressed do
	(
		max create mode
		currentobjectindex  +=1
		s = _sel()
		currentobjectindex  = ( mod  currentobjectindex basenamearray.count )
		
		
			if ( currentobjectindex  == 0 ) do currentobjectindex =  basenamearray.count
		m = mergemaxfile ( maxbasename[basenamenumber] ) #(basenamearray[currentobjectindex]) #useSceneMtlDups #skipDups 
		m= getnodebyname  basenamearray[currentobjectindex]
		if ( getrotation $ != false ) do 
			m.rotation = getrotation $
		if ( getpos $ == false ) 
		then 
			m.pos = savepos 
		else 	
			m.pos = getpos $
		if ( $ != undefined ) do 
		(
			setuserprop $ "oktodelete" true
			delete $ 
		)
		select m
		setuserprop $ "frombasefile" ( maxbasename[basenamenumber] ) 
		setuserprop $ "fromobjectnamed" basenamearray[currentobjectindex]
		setuserprop $ "transform" $.transform
			print currentobjectindex
	)
	on rotatexbutton pressed do
	(
		sel = _sel()
		if ( globalrotcheck.checked )then  
			dorotate $   ( rotateXMatrix 90) 
		else 
			for n in sel do 
				dorotate n   ( rotateXMatrix 90)
	)
	on rotateybutton pressed do
	(
		sel = _sel()
		if ( globalrotcheck.checked )then  
			dorotate $   ( rotateYMatrix 90) 
		else 
			for n in sel do 
				dorotate n   ( rotateYMatrix 90)
	)
	on rotatezbutton pressed do
	(
		sel = _sel()
		if ( globalrotcheck.checked )then  
			dorotate $   ( rotateZMatrix 90) 
		else 
			for n in sel do 
				dorotate n   ( rotateZMatrix 90)
	)
	on duplicatebutton pressed do
	(
		try 
		( 
			select $
			savepos = $.pos 
		) catch (messagebox "no selection")
	)
	on baseselddlist selected sel do
	(	
		-- change base and refresh block list
		basenamenumber = sel
		if ( doesfileexist (  maxbasename[basenamenumber] ) )  then 
		(
				
			print ( maxbasename[basenamenumber] )
			basenamearray = getMAXFileObjectNames (  maxbasename[basenamenumber] )
			exportpath = "Project_2013\\BlockBuster\\Assets\\PLATFORM\\"+ getfilenamefile ( maxbasename[basenamenumber] )+"\\"
			
			
		)
		else
			messagebox (  "no such file : " + maxbasename[basenamenumber] )
	)
	on propddlist selected sel do
		(	
	)
	on setmodulesizebutton pressed do
	(
		if $==undefined do return false
		if $ == selection 
		then  
				messagebox "canot set custom value on multiple block"
		else 
			(
				setuserprop $ "xbase" xbasespinner.value
				setuserprop $ "ybase" ybasespinner.value
				setuserprop $ "zbase" zbasespinner.value
			)
	)
	on removecustomsizebutton pressed do
	(
		if $==undefined do return false
		for n in (_sel())  do  
			(
				setuserprop n "xbase" undefined
				setuserprop n "ybase" undefined
				setuserprop n "zbase" undefined
			)
	)
	on stepmovebutton pressed do
	(
		step+=1
		i =  ( mod    step steptable.count ) +1
		stepmovebutton.caption = ( steptable[i] as string ) 
		movestepvalue = steptable[i]
	)
	on movefrontbutton pressed do
	( 
		getaxis() 
		domove (_sel()) (point3 (front.x*movestepvalue )  (front.y*movestepvalue)  0) 
	)
	on moverightbutton pressed do
	(	
		getaxis() 
		domove (_sel()) (point3 (-left.x*movestepvalue )  (-left.y*movestepvalue)  0) 
	)
	on movebackbutton pressed do
	(		
		getaxis() 
		domove (_sel()) (point3 (-front.x*movestepvalue )  (-front.y*movestepvalue)  0) 
	)
	on moveleftbutton pressed do
	(
		getaxis() 
		domove (_sel()) (point3 (left.x*movestepvalue )  (left.y*movestepvalue)  0) 
	)
	on moveupbutton pressed do
	(
		getaxis() 
		domove (_sel()) (point3 0 0 1*movestepvalue)
	)
	on movedownbutton pressed do
	(
		getaxis() 
		domove (_sel()) (point3 0 0 -1*movestepvalue	 ) 
	)
	on mergeblockbutton pressed do
	(
		if ( $ == undefined or $!=selection  ) do return false 	
		newset=#()	
		sel =#()
		-- get all sub component 
		subblocktextbuffer = ""
		for n in $ do -- remove ld block from selection 
		(
			convertTo n editable_mesh
			sel[sel.count+1]=n
		)
		index = 1	
		rootmesh = sel[1]
		for n in sel do 
		(
			s= getuserprop  n "frombasefile"
			if s != undefined do 
			subblocktextbuffer+= ( s +"@" )
			s= getuserprop  n "fromobjectnamed"
			if s != undefined do 
			subblocktextbuffer+= ( s +"@" )
			s= ( n.transform * (inverse  rootmesh.transform) ) as string
			subblocktextbuffer+= ( s +"@" )
			s= getuserprop n "coumpoundblock"	
			if s == "true" do 
				subblocktextbuffer+= ( ( getuserprop n "subblocks" ) +"@" )
		)
		for c=2 to sel.count do 
		meshop.attach rootmesh sel[c]  condenseMat:true deleteSourceNode:true
		select rootmesh
		setuserprop $ "subblocks" subblocktextbuffer
		setuserprop $ "coumpoundblock" true
		setuserprop $ "transform" $.transform
		setuserprop $ "frombasefile"   undefined 
		setuserprop $ "fromobjectnamed"  (rootmesh.name+"_coumpound")
		-- remove custom size   
		setuserprop $ "xbase" undefined
		setuserprop $ "ybase" undefined
		setuserprop $ "zbase" undefined
	)
	on setpropbutton pressed do
	(
		if $==undefined do return false
		if $ == selection then 
		(
			for n in $ do 
				setuserprop n "blocktype" propddlist.items[propddlist.selection]
		)	
		else 		
			setuserprop n "blocktype" propddlist.items[propddlist.selection]
	)
	on resetblocktransformbutton pressed do
	(
		if $==undefined do return false 
		for n in (_sel()) do 
			try ( n.transform = ( execute ( getuserprop n "transform" 	) ) ) catch ()
	)
	on isolaterefblockbutton pressed do
	(	-- todo : change to use the new master object seting
			s=#()
			for n in $ do 
			(
			InstanceMgr.GetInstances n &instances 
			s[s.count+1] =  instances[1]
			)
			select s
	)
	on unmergeblock pressed do
	(
		if $ == undefined do return false 
		memsel = $
		subblocksdescarray = #() 
		subblockstring = getuserprop memsel "subblocks" 
		buf = filterString subblockstring "@"
		for c = 1 to (buf.count) by 3  do
		(	
			desc = subblockdesc()
			desc.blockfilepath=  buf[c]
			desc.objectname = buf[c+1]
			desc.blocktransform= buf[c+2]
			subblocksdescarray[subblocksdescarray.count+1]= desc
		)
		for n in subblocksdescarray do 
		try(
				mergemaxfile  (n.blockfilepath) #(n.objectname)  #useSceneMtlDups #mergeDups 
				m=getnodebyname n.objectname 
				 inst  =  instance  m 
				buftext= getUserPropBuffer m
				setUserPropBuffer inst buftext 
				-- validate block id 
				setuserprop inst "frombasefile" n.blockfilepath
				setuserprop inst "fromobjectnamed" n.objectname
				setuserprop m "oktodelete" true
				delete m
				mtrx = execute n.blocktransform 
				inst.transform =    mtrx   
				setuserprop inst "transform" inst.transform
				inst.wirecolor = color 115 115 115
				selectmore inst
			)catch( print "problem in splitblock " )
		recal = execute ( getuserprop memsel "transform" ) 
		delete memsel
		for n in (_sel()) do 
		(
			n.transform =  n.transform  * recal 
		)
	)
	on addnamebutton pressed do
	(
		newname = nmmapddl.selected+"_"+nmtypeddl.selected+"_"+nmxed.text+nmyed.text+nmzed.text+"_" +iterddl.selected+"_"+iterletterddl.selected+"\n"
		listname.text+=newname
	)
	on batchexport pressed do
	(
		blog =  ( querybox "see fbx export result interface" ) 
		count = 0 
		if ( $ == undefined ) do
		(	
			generateassetlist() 
			return false
		)
		
		
		
		A=  (filterstring exportpath "\\" )[( filterstring exportpath "\\" ).count]
		
		currentselection =#()
		if $ != selection then  
			append currentselection  $
		else 
			for n in $ do 
				append currentselection  n
		if  currentselection.count  == 0 do  
		(
			messagebox ( "nothing selected generate asset list anyway .. " )
			generateassetlist ()
			return  false 
		)
		theClasses =exporterPlugin.classes
		theClasses[9]
		bkpos = [0,0,0]
	
		if ( exportpath == "" )  -- todo use a normed name system for everysingle thing 
			do 
				exportpath =  getSavePath  "were is the unity repository for exported files "
	
		for it in (currentselection) do 
		(
			clearSelection()
			select it 
			
			b= ( filterstring it.name "_")[1] 
		
			if ( A != B) do 
			(
				if ( querybox  ( "export in wron folder not allowed " + "activebase: " + A + " assetname : " + B +" continue to export ? " )  ) then 
					continue 
				else 
					return false 
			)
			
			
			
			bkpos = it.pos
			it.pos = [0,0,0] 
			if ( blog == false  ) then 			
				exportFile (exportpath + it.name + ".fbx") #noPrompt   selectedonly:true  using: theClasses[9]  
			else 
			(
				exportFile (exportpath + it.name + ".fbx")  selectedonly:true  using: theClasses[9]
				blog = false
			)
			
			it.pos = bkpos 
		)
		-- write the xml file for unity 
		generateassetlist ()
	)
	on clearinifile pressed do
	(
		setINISetting ( GetDir #userScripts + "/3dsmax.ini")  "LOCALPATH" "mylocalpath" "" 	
	)
	on gencolbutton pressed do
	(
		if $ == undefined do return false 
		c=0
		for n in (_sel()) do 
			(
				if (	getuserprop n "coumpoundblock" == true  ) do return false -- deal with merged block 
				colname =  (getuserprop n "fromobjectnamed") + "_COL" 
				mergemaxfile colbasename[basenamenumber] #( colname )#useSceneMtlDups #skipDups 
				col = getnodebyname	colname 
				col.name+= ( c as string ) 
				c+=1
				print colbasename[basenamenumber] 
				print colname
				col.transform = n.transform
				setuserprop col "transform" tr 
				setuserprop col  "frombasefile"  colbasename[basenamenumber] 
				setuserprop col  "fromobjectnamed"  colname
			)	
	)
	on cleanrayfiremesh pressed do
	(
		if ( $ == undefined  or $ == selection ) do
			return
		matchlist = #() -- face to delete 
		for n = 1 to polyop.getNumFaces $  do 
		(
			far1 = polyop.getFaceVerts $ n  -- get vertindex 
			chk1 = point3  0 0 0   -- sum of vert in face 
			for c in far1 do  -- ref face 
			(
				v = polyop.getVert $ c  
				chk1  += v 
			)
			chk1 =  chk1 / far1.count  
			for i = n to polyop.getNumFaces $  do 
			(
				if ( i > n ) do -- bypass n 
				(
					far2 = polyop.getFaceVerts $  i
					if ( far1.count == far2.count ) do 
					(
						chk2 = point3 0 0 0  
						for c in far2 do  -- ref face 
						(
							v2 = polyop.getVert $ c  
							chk2 +=  v2 -- v2[1] + v2[2] + v2[3]
						)
							chk2 = chk2 / far2.count 
							if ( distance  chk1 chk2  < 0.01  )  do 
							(
								matchlist[matchlist.count+1] = i 
								matchlist[matchlist.count+1] = n
							)
					)
				)
			)
		)
		if ( matchlist.count > 0) do
			polyop.setFaceSelection  $ matchlist
	)
	on newscenebutton pressed do
	(
		if ( queryBox "start a new scene ? " ) do
		(
			loadMaxFile  (  "E:\\atrabuio_SGHW-ATRABUIO_NOROTATOR_9469\\MAXTOOLS\\base\\startfile.max" ) useFileUnits: true  quiet: false
			actionMan.executeAction 0 "16"
		)
	)
	on changenamebutton pressed do
	(
		newname = nmmapddl.selected+"_"+nmtypeddl.selected+"_"+nmxed.text+"_"+nmyed.text+"_"+nmzed.text+"_" +iterddl.selected+"_"+iterletterddl.selected
		if ( $ == undefined  or $ == selection ) do 
		(
			messagebox "wrong selection can only change name on a single object "
			return undefined
		)
		msel = $ -- store the object selection 
		-- check if name is not used 
		max select all 
		allobjects =  _sel()
		select msel 
		for it in allobjects do 
			if ( it.name ==  newname ) then
			(
				if ( querybox  "name allready in use change name seting"  )   then
				(		
					setuserprop $ "originalname" newname
					msel.name = ( newname ) 
					msel.wirecolor = color 6 135 113
					macros.run "Modifier Stack" "Convert_to_Poly"
				    max create mode
				)
				else 
					return false 
			)
			else 
			(
				setuserprop $ "originalname" newname
				msel.name = ( newname ) 
				msel.wirecolor = color 6 135 113
				macros.run "Modifier Stack" "Convert_to_Poly"
				max create mode
			)
	)
	on tounity pressed do
	(
					
			if  ( doesfileexist ( exportpath+"assettransfert.xml" ))  then
			(
					fstr= openfile 	( exportpath+"assettransfert.xml" ) mode:"w+"
					print "file exist open "
			)
			else 	
				fstr = 	createFile ( exportpath+"assettransfert.xml" ) 
			str = "<ObjectList>\n"
	
			sel =  $ --_sel()
			
			for n in $ do 
			(
				if ( classof n == Dummy) do continue 
	
				str+= "		<obj>\n"
				str+= "			<name>" + ( getuserprop n "originalname") + "</name>\n"  
				str+= "			<posx>"+( n.pos.x as string ) + "</posx>\n"  
				str+= "			<posy>"+( n.pos.y as string ) + "</posy>\n"  
				str+= "			<posz>"+( n.pos.z as string ) + "</posz>\n"  
				a = n.rotation as eulerangles 
				str+= "			<rotx>"+ (floor  (a.x + 0.5 ) ) as string + "</rotx>\n"  
				str+= "			<roty>"+ (floor  (a.y + 0.5 ) ) as string+ "</roty>\n"  
				str+= "			<rotz>"+ (floor  (a.z + 0.5 ) ) as string+ "</rotz>\n"  				
				str+= "		</obj>\n"
			)
			str += "</ObjectList>\n"
			print str
			format ( str ) to:  fstr
			flush fstr
			close fstr 
	)
	on generateblocksfromtextfile pressed do
	(
			blocksetname = getsavefilename()
			textfilestream = listname.text as stringstream
			creatednodes = #()
			while not eof textfilestream do 
			(
					objname =   readline textfilestream 
					if objname =="" do return false
					parsenamearray = filterstring objname "_"
					size = ""
					c= 1
					while ( classof size == string  or c < parsenamearray.count )   do 
					(
						currentstring = parsenamearray[c] 
						size = currentstring as integer
						if classof size == integer do  
							( --catchsize = size 
								c=parsenamearray.count 
							)
						c+=1
					)
					s= ( size as string ) as stringstream 
					x =(readchar s)
					y= (readchar s)
					z= (readchar s) 
					if ( not eof s ) do 
						if ( z == "1" ) do z+=readchar s
					
					x = x as integer 
					y = y as integer 
					z = z as integer 
					b= Box lengthsegs:1 widthsegs:1 heightsegs:1 length:x  width:y  height:z mapcoords:on pos:[0,0,0] isSelected:on  name:objname
					setuserprop b "blocktype" "lablock"
					append creatednodes b  
			)
			pthname = getfilenamepath blocksetname 
			fname = getfilenamefile blocksetname
			maxname = (pthname+fname+".max"	) 
			savenodes creatednodes maxname ( maxbasename[maxbasename.count+1] ) =maxname
			gotcha = false 
			for n in baseitems do 
			(
				if n==maxname do gotcha = true
			)
			if (not gotcha ) do  
			(
				arr = baseselddlist.items
				append arr fname 
				baseselddlist.items = arr
			)
	)
	on globalrotcheck changed state do
		(
	)
	on ldlayerchkbox changed state do
	(
		--unhide objects
		for n in $objects do 
		(
			case state of 
			(
				true: if ( getuserprop n "blocktype" == "ldblock" ) do unhide n
				false: if ( getuserprop n "blocktype" == "ldblock" ) do hide n
			)
		)
	)
	on lalayerchkbox changed state do
	(
		for n in $objects do 
		(
			case state of 
			(
				true: if ( getuserprop n "blocktype" == "lablock" ) do unhide n
				false: if ( getuserprop n "blocktype" == "lablock" ) do hide n
			)
		)
	)
	on selectinstancechk changed state do
		(
	)
	on batchrenamebutton pressed do
	(
			
		createdialog batchrename
		
	)
	on hidebutton pressed do
	(
		max hide selection
	)
	on unhidebutton pressed do
	(
		max unhide all
	)
	
	
)


createdialog MainRollout