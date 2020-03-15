///////////////////////////////////////////////////////////////////////////
//  IK Helper Tool 1.0 - Custom Inspector                                //
//  Kevin Iglesias - https://www.keviniglesias.com/     			     //
//  Contact Support: support@keviniglesias.com                           //
//  Documentation: 														 //
//  https://www.keviniglesias.com/assets/IKHelperTool/Documentation.pdf  //
///////////////////////////////////////////////////////////////////////////

//This script makes a custom inspector for the IK Helper Tool MonoBehaviour

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;

namespace KevinIglesias {

    public class IKColors
    {
        public static Color workingIKColor = Color.green;
        
        public static Color incompleteIKColor = Color.yellow;
        
        public static Color defaultColor = Color.white;
    }

    [CustomEditor(typeof(IKHelperTool))]
    public class IKHelperToolCustomInspector : Editor
    {

        private string[] iKTypeStrings = new string[4] {"Right Hand", "Left Hand", "Right Foot", "Left Foot"};
    
        private RuntimeAnimatorController currentController;
    
        bool enabledGUI = true;
    
        public override void OnInspectorGUI()
		{
            var iKScript = target as IKHelperTool;
       
            //Show warning when no controller found
            if(iKScript.controller == null)
            {
                GUILayout.BeginHorizontal(); 
                GUILayout.Label("No controller found in the Animator.");
                GUILayout.EndHorizontal();
                iKScript.Refresh();
                return;
            }
            
            //Show warning when no avatar found / avatar is not human
            if(!iKScript.animator.isHuman)
            {
                GUILayout.BeginHorizontal(); 
                GUILayout.Label("Animator Avatar not found or is not human.");
                GUILayout.EndHorizontal();
                iKScript.Refresh();
                return;
            }

            GUI.enabled = enabledGUI;
       
            //Use Global IKs checkbox
            EditorGUI.BeginChangeCheck();
            GUILayout.BeginHorizontal(); 
            GUIContent globalIK = new GUIContent("Use Global IKs", "Use the same IKs for all the animations");
            bool useGlobalIK = EditorGUILayout.Toggle(globalIK, iKScript.useGlobalIK);
            GUILayout.EndHorizontal();
            if(EditorGUI.EndChangeCheck()) {
                Undo.RegisterUndo(target, "Use Global IK");
                iKScript.useGlobalIK = useGlobalIK;
            }
  
            DrawUILine(Color.black);
            
            //Show Global IKs if enabled
            if(iKScript.useGlobalIK && enabledGUI)
            {
                //Right Hand, Left Hand, Right Foot and Left Foot
                for(int i = 0; i < iKScript.globalIKs.Count; i++)
                {
                    GUI.backgroundColor = IKColors.defaultColor;
                
                    if(iKScript.globalIKs[i].status == 1)
                    {
                        GUI.backgroundColor = IKColors.incompleteIKColor;
                    }
                    
                    if(iKScript.globalIKs[i].status == 2)
                    {
                        GUI.backgroundColor = IKColors.workingIKColor;
                    }
                    
                    //Transform Attach IK
                    EditorGUI.BeginChangeCheck();
                    GUILayout.BeginHorizontal();
                    Transform iIKAttachment = EditorGUILayout.ObjectField("Global IK "+iKTypeStrings[i], iKScript.globalIKs[i].iKAttachment, typeof(Transform)) as Transform;
                    GUILayout.EndHorizontal();
                    if(EditorGUI.EndChangeCheck()) {
                        Undo.RegisterUndo(target, "Change IK Transform "+iKTypeStrings[i]);
                        iKScript.globalIKs[i].iKAttachment = iIKAttachment;
                    }
                    
                    //Use Location of IK
                    EditorGUI.BeginChangeCheck();
                    GUILayout.BeginHorizontal();
                    bool iUseLocation = EditorGUILayout.Toggle("Use Location", iKScript.globalIKs[i].useLocation);
                    GUILayout.EndHorizontal();
                    if(EditorGUI.EndChangeCheck()) {
                        Undo.RegisterUndo(target, "Change Use Location "+iKTypeStrings[i]);
                        iKScript.globalIKs[i].useLocation = iUseLocation;
                    }
                    
                    //Use Rotation of IK
                    EditorGUI.BeginChangeCheck();
                    GUILayout.BeginHorizontal();
                    bool iUseRotation = EditorGUILayout.Toggle("Use Rotation", iKScript.globalIKs[i].useRotation);
                    GUILayout.EndHorizontal();
                    if(EditorGUI.EndChangeCheck()) {
                        Undo.RegisterUndo(target, "Change Use Rotation "+iKTypeStrings[i]);
                        iKScript.globalIKs[i].useRotation = iUseRotation;
                    }
                    
                    GUILayout.Space(5);
                    
                    GUI.backgroundColor = IKColors.defaultColor;
                }  

                DrawUILine(Color.black);  
            }
               
            GUILayout.Space(5);    
                
                
            ///Individual IKs for each Animation
            //Check if controller changed
            currentController = iKScript.CheckCurrentController();
            if(currentController != iKScript.controller)
            {
                //If controller changed force user to click in Update for avoiding data loss if misclick.

                GUI.enabled = true;
                
                enabledGUI = false;
                
                GUILayout.BeginHorizontal();
                GUILayout.Label("Controller changed ("+iKScript.controller.name+")");
                GUILayout.EndHorizontal();

                GUI.skin.button.alignment = TextAnchor.MiddleCenter;
                GUILayout.BeginHorizontal();
                if(GUILayout.Button("Update"))
                {
                   iKScript.Refresh();
                }

                GUILayout.EndHorizontal();
                
                GUI.enabled = enabledGUI;
            }else{
                //Show number of detected animations
                
                GUILayout.BeginHorizontal();
                GUILayout.Label("Individual IKs - Detected Animations: "+ iKScript.animations.Length);
                GUILayout.EndHorizontal();

                enabledGUI = true;
            }

            //Make sure everything is initialized
            if(iKScript.individualIKs == null)
            {
                iKScript.Refresh();
                return;
            }
            
            DrawUILine(Color.black);

            //Show IK panels for each animation
            for(int i = 0; i < iKScript.animations.Length; i++)
            {
        
                GUI.backgroundColor = IKColors.defaultColor;
                
                if(iKScript.individualIKs[i].status == 1)
                {
                    GUI.backgroundColor = IKColors.incompleteIKColor;
                }
                
                if(iKScript.individualIKs[i].status == 2)
                {
                    GUI.backgroundColor = IKColors.workingIKColor;
                }
        
                GUI.skin.button.alignment = TextAnchor.MiddleLeft;
                
                GUILayout.BeginHorizontal();
                
                if(iKScript.individualIKs[i].open)
                {
                    if(iKScript.individualIKs[i].isPlaying)
                    {
                        if(GUILayout.Button("▼ (PLAYING) "+(i+1).ToString("000")+". "+iKScript.animations[i].name))
                        {
                            iKScript.individualIKs[i].open = !iKScript.individualIKs[i].open;
                        }
                    }else{
                        if(GUILayout.Button("▼ "+(i+1).ToString("000")+". "+iKScript.animations[i].name))
                        {
                            iKScript.individualIKs[i].open = !iKScript.individualIKs[i].open;
                        }
                    }

                    GUI.backgroundColor = IKColors.defaultColor;
                    
                    GUILayout.EndHorizontal();
   
                    //Right Hand, Left Hand, Right Foot and Left Foot
                    for(int j = 0; j < iKScript.individualIKs[i].typeIK.Count; j++)
                    {
                        
                        GUI.backgroundColor = IKColors.defaultColor;
                
                        if(iKScript.individualIKs[i].typeIK[j].status == 1)
                        {
                            GUI.backgroundColor = IKColors.incompleteIKColor;
                        }
                        
                        if(iKScript.individualIKs[i].typeIK[j].status == 2)
                        {
                            GUI.backgroundColor = IKColors.workingIKColor;
                        }

                        //Type IK Panel (Right Hand, Left Hand, Right Foot or Left Foot)
                        if(iKScript.individualIKs[i].typeIK[j].open)
                        {
                            
                            GUILayout.BeginHorizontal();
                            GUILayout.Space(15);
                            if(GUILayout.Button("▼ "+iKTypeStrings[j]))
                            {
                                iKScript.individualIKs[i].typeIK[j].open = !iKScript.individualIKs[i].typeIK[j].open;
                            }
                           
                            GUILayout.EndHorizontal();

                            GUI.skin.button.alignment = TextAnchor.MiddleCenter;
                            
                            GUILayout.BeginHorizontal();
                            GUILayout.Space(30);
                            DrawUILine(Color.black);
                            GUILayout.EndHorizontal();
                     
                            GUI.backgroundColor = IKColors.defaultColor;
                
                            if(iKScript.individualIKs[i].typeIK[j].defaultIK.status == 1)
                            {
                                GUI.backgroundColor = IKColors.incompleteIKColor;
                            }
                            
                            if(iKScript.individualIKs[i].typeIK[j].defaultIK.status == 2)
                            {
                                GUI.backgroundColor = IKColors.workingIKColor;
                            }
      
                            //Default IK Transform for this IK type and animation
                            EditorGUI.BeginChangeCheck();
                            GUILayout.BeginHorizontal();
                            GUILayout.Space(30);
                            Transform iIKAttachment = EditorGUILayout.ObjectField("", iKScript.individualIKs[i].typeIK[j].defaultIK.iKAttachment, typeof(Transform)) as Transform;
                            GUILayout.EndHorizontal();
                            if(EditorGUI.EndChangeCheck()) {
                                Undo.RegisterUndo(target, "Change Default IK Attachment "+iKScript.animations[i].name);
                                iKScript.individualIKs[i].typeIK[j].defaultIK.iKAttachment = iIKAttachment;
                            }
                            
                            //Use Location of Default IK
                            EditorGUI.BeginChangeCheck();
                            GUILayout.BeginHorizontal();
                            GUILayout.Space(30);
                            bool iUseLocation = EditorGUILayout.Toggle("Use Location", iKScript.individualIKs[i].typeIK[j].defaultIK.useLocation);
                            GUILayout.EndHorizontal();
                            if(EditorGUI.EndChangeCheck()) {
                                Undo.RegisterUndo(target, "Default IK Use Location "+iKScript.animations[i].name);
                                iKScript.individualIKs[i].typeIK[j].defaultIK.useLocation = iUseLocation;
                            }
                            
                            //Use Rotation of Default IK
                            EditorGUI.BeginChangeCheck();
                            GUILayout.BeginHorizontal();
                            GUILayout.Space(30);
                            bool iUseRotation = EditorGUILayout.Toggle("Use Rotation", iKScript.individualIKs[i].typeIK[j].defaultIK.useRotation);
                            GUILayout.EndHorizontal();
                            if(EditorGUI.EndChangeCheck()) {
                                Undo.RegisterUndo(target, "Default IK Use Rotation "+iKScript.animations[i].name);
                                iKScript.individualIKs[i].typeIK[j].defaultIK.useRotation = iUseRotation;
                            }
                            
                            GUILayout.Space(5);
                            
                            GUI.backgroundColor = IKColors.defaultColor;
                       
                            //Dynamic IK: Use more than one IK of the same type in the same animation (First IK will be the default IK)
                            EditorGUI.BeginChangeCheck();
                            GUILayout.BeginHorizontal();
                            GUILayout.Space(30);
                            bool iUseDynamic = EditorGUILayout.Toggle("Use Dynamic IKs", iKScript.individualIKs[i].typeIK[j].dynamicIK);
                            GUILayout.EndHorizontal();
                            if(EditorGUI.EndChangeCheck()) {
                                Undo.RegisterUndo(target, "Use Dynamic IK "+iKScript.animations[i].name);
                                iKScript.individualIKs[i].typeIK[j].dynamicIK = iUseDynamic;
                            }
                            
                            EditorGUI.BeginChangeCheck();

                            if(iKScript.individualIKs[i].typeIK[j].dynamicIK)
                            {
                                //Add buttons for adding / removing dynamic IKs
                                for(int k = 0; k < iKScript.individualIKs[i].typeIK[j].dynamicIKs.Count; k++)
                                {
                                    ///Dynamic IK
                                    //Button for remove this IK
                                    GUILayout.BeginHorizontal();
                                    GUILayout.Space(30);
                                    GUIContent removeDynamicIK = new GUIContent("[X] "+(k+1).ToString("00"), "Remove this IK");
                                    if(GUILayout.Button(removeDynamicIK, GUILayout.Width(50)))
                                    {
                                        iKScript.individualIKs[i].typeIK[j].dynamicIKs.RemoveAt(k);
                                        break;
                                    }
                                    
                                    GUI.backgroundColor = IKColors.defaultColor;
                    
                                    if(iKScript.individualIKs[i].typeIK[j].dynamicIKs[k].status == 1)
                                    {
                                        GUI.backgroundColor = IKColors.incompleteIKColor;
                                    }
                                    
                                    if(iKScript.individualIKs[i].typeIK[j].dynamicIKs[k].status == 2)
                                    {
                                        GUI.backgroundColor = IKColors.workingIKColor;
                                    }
       
                                    //Dynamic IK Transform
                                    EditorGUI.BeginChangeCheck();
                                    Transform IDynIKAttachment = EditorGUILayout.ObjectField("", iKScript.individualIKs[i].typeIK[j].dynamicIKs[k].iKAttachment, typeof(Transform)) as Transform;
                                    if(EditorGUI.EndChangeCheck()) {
                                        Undo.RegisterUndo(target, "Individual IK Transform Change "+iKScript.animations[i].name);
                                        iKScript.individualIKs[i].typeIK[j].dynamicIKs[k].iKAttachment = IDynIKAttachment;
                                    }
                                    GUILayout.EndHorizontal();
                                    
                                    
                                    //Use Location of Dynamic IK
                                    EditorGUI.BeginChangeCheck();
                                    GUILayout.BeginHorizontal();
                                    GUILayout.Space(30);
                                    bool dynUseLocation = EditorGUILayout.Toggle("Use Location", iKScript.individualIKs[i].typeIK[j].dynamicIKs[k].useLocation);
                                    GUILayout.EndHorizontal();
                                    if(EditorGUI.EndChangeCheck()) {
                                        Undo.RegisterUndo(target, "Individual IK Use Location "+iKScript.animations[i].name);
                                        iKScript.individualIKs[i].typeIK[j].dynamicIKs[k].useLocation = dynUseLocation;
                                    }
                                    
                                    //Use Rotation of Dynamic IK
                                    EditorGUI.BeginChangeCheck();
                                    GUILayout.BeginHorizontal();
                                    GUILayout.Space(30);
                                    bool dynUseRotation = EditorGUILayout.Toggle("Use Rotation", iKScript.individualIKs[i].typeIK[j].dynamicIKs[k].useRotation);
                                    GUILayout.EndHorizontal();
                                    if(EditorGUI.EndChangeCheck()) {
                                        Undo.RegisterUndo(target, "Individual IK Use Location "+iKScript.animations[i].name);
                                        iKScript.individualIKs[i].typeIK[j].dynamicIKs[k].useRotation = dynUseRotation;
                                    }

                                    //Time in the animation where the IK change will happen (in %, 0.5 means middle of the animation)
                                    EditorGUI.BeginChangeCheck();
                                    GUIContent timeContent = new GUIContent("Time", "Time in the animation where the IK change will happen (in %, 0.5 means middle of the animation)");
                                    GUILayout.BeginHorizontal();
                                    GUILayout.Space(30);
                                    float iTime = EditorGUILayout.FloatField(timeContent, iKScript.individualIKs[i].typeIK[j].dynamicIKs[k].time);
                                    GUILayout.EndHorizontal();
                                    if(EditorGUI.EndChangeCheck()) {
                                        Undo.RegisterUndo(target, "Individual IK Time "+iKScript.animations[i].name);
                                        iKScript.individualIKs[i].typeIK[j].dynamicIKs[k].time = iTime;
                                    }
                                    
                                    //How fast the IK will change to this one
                                    EditorGUI.BeginChangeCheck();
                                    GUIContent speedContent = new GUIContent("Speed", "How fast the IK will change to this one");
                                    GUILayout.BeginHorizontal();
                                    GUILayout.Space(30);
                                    float iSpeed = EditorGUILayout.FloatField(speedContent, iKScript.individualIKs[i].typeIK[j].dynamicIKs[k].speed);
                                    GUILayout.EndHorizontal();
                                    if(EditorGUI.EndChangeCheck()) {
                                        Undo.RegisterUndo(target, "Individual IK Speed "+iKScript.animations[i].name);
                                        iKScript.individualIKs[i].typeIK[j].dynamicIKs[k].speed = iSpeed;
                                    }
                                    
                                    GUILayout.Space(5);
                                    
                                    GUI.backgroundColor = IKColors.defaultColor;

                                }
                                
                                //Add a new Dynamic IK
                                GUILayout.BeginHorizontal();
                                GUILayout.Space(30);
                                GUIContent addButton = new GUIContent("[+] Add Dynamic IK", "Add a new IK to switch during this animation");
                                if(GUILayout.Button(addButton))
                                {
                                    if(iKScript.individualIKs[i].typeIK[j].dynamicIKs == null)
                                    {
                                        iKScript.individualIKs[i].typeIK[j].dynamicIKs = new List<IKAttachment>();
                                    }
                                    iKScript.individualIKs[i].typeIK[j].dynamicIKs.Add(new IKAttachment());
                                }
                                
                                //Remove all Dynamic IK of this type of this animation
                                GUIContent removeAll = new GUIContent("[X] Remove All", "Clear ALL Dynamic IKs from this list");
                                if(GUILayout.Button(removeAll))
                                {
                                    iKScript.individualIKs[i].typeIK[j].dynamicIKs = new List<IKAttachment>();
                                }
                                
                                GUILayout.EndHorizontal(); 
                            }
                            
                            List<IKAttachment> iDynamicIKs = iKScript.individualIKs[i].typeIK[j].dynamicIKs;
                           
                            if(EditorGUI.EndChangeCheck()) {
                                Undo.RegisterUndo(target, "Change Dynamic IKs Size "+iKScript.animations[i].name);
                                iKScript.individualIKs[i].typeIK[j].dynamicIKs = iDynamicIKs;
                            }
                            
                            GUI.skin.button.alignment = TextAnchor.MiddleLeft;

                            GUILayout.BeginHorizontal();
                            GUILayout.Space(30);
                            DrawUILine(Color.black);
                            GUILayout.EndHorizontal();
                            
                        }else{
                            
                            GUILayout.BeginHorizontal();
                            GUILayout.Space(15);
                            if(GUILayout.Button("► "+iKTypeStrings[j]))
                            {
                                iKScript.individualIKs[i].typeIK[j].open = !iKScript.individualIKs[i].typeIK[j].open;
                            }
                            GUILayout.EndHorizontal();
                            
                            GUI.backgroundColor = IKColors.defaultColor;
    
                        }
    
                    }
                }else{
                    
                    if(iKScript.individualIKs[i].isPlaying)
                    {
                        if(GUILayout.Button("► (PLAYING) "+(i+1).ToString("000")+". "+iKScript.animations[i].name))
                        {
                            iKScript.individualIKs[i].open = !iKScript.individualIKs[i].open;
                        }
                    }else{
                        if(GUILayout.Button("► "+(i+1).ToString("000")+". "+iKScript.animations[i].name))
                        {
                            iKScript.individualIKs[i].open = !iKScript.individualIKs[i].open;
                        }
                    }
                    GUILayout.EndHorizontal();
                    
                    GUI.backgroundColor = IKColors.defaultColor;
                }
                
                GUILayout.Space(5);

                DrawUILine(Color.black);
                GUILayout.Space(5);
                
            }
   
            if(GUI.changed)
            {
                if(!EditorApplication.isPlaying)
				{
                    iKScript.Refresh();
					EditorUtility.SetDirty(target);
					EditorApplication.MarkSceneDirty();
                }
            }
        }

        //FUNCTION FOR DRAWING A SEPARATOR
		public static void DrawUILine(Color color, int thickness = 1, int padding = 2)
		{
			Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding+thickness));
			r.height = thickness;
			r.y+=padding/2;
			EditorGUI.DrawRect(r, color);
		}
    }
}
#endif