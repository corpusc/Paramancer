 
  
  class RenameTool extends EditorWindow {

    var showing : boolean = true;
 	var Find:String="";
 	var Replace:String="";
    var OBJ:Object;
 	var isTexture:boolean=false;
 	var isMaterial:boolean=false;
 	var isPrefab:boolean=false;
 	
 
    @MenuItem("Edit/Rename Tool")
   	 static function Init() {
     var window = GetWindow(RenameTool);

      window.minSize=Vector2(499,149);
      window.maxSize =Vector2(500,150);
      window.Show();

		
	 }

        
        
       function OnGUI() {
       
        var STR:String;
        var e:Event = Event.current;
        
        if (e.type == EventType.ValidateCommand && e.commandName == "Paste")
        {
            e.Use();
            
        }
        
                if (e.type == EventType.ValidateCommand && e.commandName == "Copy")
        {
            e.Use();
            var te:TextEditor = new TextEditor();
            
            if (GUI.GetNameOfFocusedControl ()=="Find")
            te.content = new GUIContent( Find);
            
            if (GUI.GetNameOfFocusedControl ()=="Replace")
            te.content = new GUIContent( Replace);
            
			te.SelectAll();
			te.Copy();
        }
 
        
        if (e.type == EventType.ExecuteCommand && e.commandName == "Paste")        
        {
    
            if (GUI.GetNameOfFocusedControl ()=="Find")
            Find  = EditorGUIUtility.systemCopyBuffer;
            
            
            if (GUI.GetNameOfFocusedControl ()=="Replace")
            Replace = EditorGUIUtility.systemCopyBuffer;
            
        }
 		
         GUI.Label (Rect (10, 10, 100, 30),"Find Text: ");
         GUI.Label (Rect (10, 40, 100, 30),"Replace Text:");
         
         GUI.SetNextControlName ("Find");
         Find = GUI.TextField (Rect (130, 10, 200 , 16),  Find.ToString());
          
             
   	     GUI.SetNextControlName ("Replace");
         Replace = GUI.TextField (Rect (130, 40, 200 , 16), Replace.ToString());
         
   
   		   isTexture  = GUI.Toggle(Rect(420, 10, 100, 30),isTexture, "Texture");
   		   isMaterial = GUI.Toggle(Rect(420, 40, 100, 30),isMaterial,"Material");
   		   isPrefab   = GUI.Toggle(Rect(420, 70, 100, 30),isPrefab,"Prefab");
   


 
   
         if (GUI.Button (Rect (80, 100, 60 , 16), "Replace"))
         { 
         if (Find.Length!=0 ){
      		if (Selection.objects.Length==0){
       		for(var gameObj : GameObject in GameObject.FindObjectsOfType(GameObject))
			{
  			 STR=gameObj.name;
  			 
  				gameObj.name= STR.Replace (Find.ToString(),Replace.ToString());
  		  
				}
			} else
		 
		 {
		  for (var i:int=0; i< Selection.objects.Length; i++){
		  		STR=Selection.objects[i].name;
  				Selection.objects[i].name= STR.Replace (Find.ToString(),Replace.ToString());
		  		}
		 	 }
       	  }
        }
        
         if (GUI.Button (Rect (260, 100, 140 , 16), "Replace Asset Name"))
         {
            if (Find.Length!=0 ){
            AssetDatabase.Refresh();
            
            if (isMaterial){
          	var aa=Resources.FindObjectsOfTypeAll (Material);
          	for (var x:int=0; x< aa.Length;x++ ){
  			 	STR=aa[x].name;
  				if (STR.Contains (Find.ToString()))
  				{
  					var Name= STR.Replace(Find.ToString(),Replace.ToString());
        	  		AssetDatabase.RenameAsset (aa[x].name.ToString(),Name.ToString())	;
        	   		AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(aa[x]),Name.ToString());
        	  		} 
        	  	}
  			  }
  			  
  		   if (isPrefab){
          	var  pa=Resources.FindObjectsOfTypeAll (GameObject);

          		for (var px:int=0; px< pa.Length;px++ ){
          		if( PrefabUtility.GetPrefabParent(pa[px]) == null && PrefabUtility.GetPrefabObject(pa[px]) != null)
          		{	
  			 		STR=pa[px].name;
  					if (STR.Contains (Find.ToString()))
  					{
  						var PName= STR.Replace(Find.ToString(),Replace.ToString());
        	  			AssetDatabase.RenameAsset (pa[px].name.ToString(),PName.ToString())	;
        	   			AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(pa[px]),PName.ToString());
        	  				} 
        	  			}
  			  		}	  
				 }
  			  
  			  if (isTexture){  
  			var ta=Resources.FindObjectsOfTypeAll (Texture2D);
          	for (var tx:int=0; tx< ta.Length;tx++ ){
  			 	STR=ta[tx].name;
  				if (STR.Contains (Find.ToString()))
  				{
  					var TName= STR.Replace(Find.ToString(),Replace.ToString());
        	  		AssetDatabase.RenameAsset (ta[tx].name.ToString(),TName.ToString())	;
        	   		AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(ta[tx]),TName.ToString());
        		  	} 
        	 	 }
        		}
        	  }
  			 }
  		    } 
        }
  

    
 function CharToUpper(ObjectName:String)
 {
 				var cnt:int=0;
   			 	if (ObjectName.Contains("_"))
  			 	{
  			 		var SSPL=ObjectName.Split ("_"[0]);
  			 		var SGname:String="";
  			 	 	var Ssign:boolean=true;
  			 	 	
  			 		for (var Sc:int; Sc< SSPL.Length-1;Sc++)
  			 		{
  			 			if (SSPL[Sc+1].Length>0 )
  			 			{ 
  			 			Ssign=true;
  			 			if (cnt==0)
  			 			SGname=SGname+SSPL[0];
  			 			
  			 				var SChar=SSPL[Sc+1].Substring(0,1);
  			 				if (SChar.ToString()!=SChar.ToUpper()){
  			 				 	Ssign=false;
  			 					SGname=SGname +SChar.ToUpper()+SSPL[Sc+1].Substring(1);	
  			 					SSPL[Sc]="";
  			 					cnt++;
  		  						} 
  		  						
  		  						if (SChar.ToString()!=SChar.ToUpper()){
  		  							SGname=SGname+SSPL[Sc] ;
  		  							SSPL[Sc]="";
  		  							
  		  							
  		  							}
   		  					}
  		  			 
  		  			 if (Ssign&&cnt>0){
  			 			 SGname=SGname+"_"+SSPL[Sc+1];
  			 			 
  			 			 }
  		  				}	
  			 		 }

 return SGname;
 
 
 }
 
 
 function CharToUpperAsset (Res:Object[]){
 
           	for (var x:int=0; x< Res.Length;x++ ){
  			 	var AssName=Res[x].name;
  			 
 				var assetName:String=CharToUpper(AssName); 
 				if (assetName){
 				
  					 if (assetName.ToString()!=AssName.ToString()){
  					 Debug.Log (AssName+"   "+assetName );
        	  		AssetDatabase.RenameAsset (AssName.ToString(),assetName.ToString())	;
        	   		AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(Res[x]),assetName.ToString());
        	  		}
        	  	}
        	  	}

 }