



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

createdialog batchrename