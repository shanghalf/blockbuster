
matchlist = #() -- face to delete 
for n = 1 to polyop.getNumFaces $  --  all faces 
	do 
	(
		polyop.setFaceSelection  $ matchlist
		far1 = polyop.getFaceVerts $ n  -- get vertindex 
		chk1 = 0   -- sum of vert in face 
		for c in far1 do  -- ref face 
		(
			v = polyop.getVert $ c  
			chk1 += v[1] + v[2] + v[3] 
			--print ( "chk1 : " + ( chk1 as string ) )
		)
		for i = n to polyop.getNumFaces $  do 
		(
			if ( i != n ) do -- bypass n 
			(
				far2 = polyop.getFaceVerts $  i
				chk2 = 0 
				for c in far2 do  -- ref face 
				(
					v2 = polyop.getVert $ c  
					chk2 += v2[1] + v2[2] + v2[3]
					if ( ( chk2 as string )  ==(  chk1 as string ) )  do 
					(
						matchlist[matchlist.count+1] = i 
						matchlist[matchlist.count+1] = n
					)
				)
			)
		)
		
	)
	polyop.setFaceSelection  $ matchlist
	
	