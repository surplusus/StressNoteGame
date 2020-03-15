///////////////////////////////////////////////////////////////////////////
//  IK Helper Tool 1.0                                 				     //
//  Kevin Iglesias - https://www.keviniglesias.com/     			     //
//  Contact Support: support@keviniglesias.com                           //
//  Documentation: 														 //
//  https://www.keviniglesias.com/assets/IKHelperTool/Documentation.pdf  //
///////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace KevinIglesias {

    public enum IKType {RightHand, LeftHand, RightFoot, LeftFoot}

    [System.Serializable]
    public class IKAttachment{
        
        public Transform iKAttachment;
        
        public bool useLocation;
        public bool useRotation;
        
        public int status; //0 - empty, 1 - incomplete, 2 - working
        
        public float time;
        public float speed;
        
        public float weight;
        
        public float totalTime;

        public bool played;
        
        public Vector3 initialPos;
        public Quaternion initialRot;

    }

    [System.Serializable]
    public class ActiveIK{
        
        public IKAttachment defaultIK = new IKAttachment();
        
        public List<IKAttachment> dynamicIKs = new List<IKAttachment>();

        public List<IKAttachment> dynamicIKsByTime = new List<IKAttachment>();
        
        public Color color;
        
        public bool open;
        
        public int status; //0 - empty, 1 - incomplete, 2 - working
        
        public bool dynamicIK;
        
        public ActiveIK()
        {
            dynamicIKs = new List<IKAttachment>();
            color = Color.white;
        }
        
    }

    [System.Serializable]
    public class IndividualIKs{
        
        public List<ActiveIK> typeIK = new List<ActiveIK>();
        
        public Color color;
        
        public bool open;
        
        public int status; //0 - empty, 1 - incomplete, 2 - working
        
        public bool isPlaying;
        
        public IndividualIKs()
        {
            typeIK = new List<ActiveIK>();
            color = Color.white;
            open = false;
        }
        
    }

    [RequireComponent(typeof(Animator))]
    public class IKHelperTool : MonoBehaviour {

        public Animator animator;
        
        public RuntimeAnimatorController controller;
        
        public bool useGlobalIK;
        
        public List<IKAttachment> globalIKs;

        public AnimationClip[] animations;
        
        public List<IndividualIKs> individualIKs;
        
        public bool individualIK;
        
        public List<IKAttachment> orderedIKs = new List<IKAttachment>();
        
        public RuntimeAnimatorController CheckCurrentController()
        {
            RuntimeAnimatorController newController = animator.runtimeAnimatorController;
            
            return newController;
        }
        
        public void Refresh()
        {
           OnValidate();
        }
        
        public void CheckController()
        {
            animator = GetComponent<Animator>();
            
            if(animator != null)
            {
                controller = animator.runtimeAnimatorController;
            }
        }
        
        void OnValidate()
        {
            //Initializes all
            
            animator = GetComponent<Animator>();

            if(animator != null)
            {
                controller = animator.runtimeAnimatorController;
                
                if(controller == null)
                {
                    return;
                }
                
                animations = animator.runtimeAnimatorController.animationClips;

                if(animator.isHuman)
                {
                    
                    if(globalIKs == null)
                    {
                        globalIKs = new List<IKAttachment>();
                    }
                    
                    if(individualIKs == null)
                    {
                        individualIKs = new List<IndividualIKs>();
                    }

                    //Global IKs
                    for(int i = 0; i < 4; i++)
                    {
                        if(globalIKs.Count <= i)
                        {
                            globalIKs.Add(new IKAttachment());
                        }
                        
                        if(globalIKs[i].iKAttachment != null)
                        {
                            if(globalIKs[i].useLocation || globalIKs[i].useRotation)
                            {
                                globalIKs[i].status = 2;
                            }else{
                                globalIKs[i].status = 1;
                            }
                        }else{
                            if(globalIKs[i].useLocation || globalIKs[i].useRotation)
                            {
                                globalIKs[i].status = 1;
                            }else{
                                globalIKs[i].status = 0;
                            }
                        }
                    }
                    
                    //Individual IKs for each animations
                    individualIK = false;
                    for(int i = 0; i < animations.Length; i++)
                    { 
                        if(individualIKs.Count <= i)
                        {
                            individualIKs.Add(new IndividualIKs());
                            
                            individualIKs[i].typeIK = new List<ActiveIK>();
                        }

                        if(individualIKs[i].typeIK.Count > 4)
                        {
                            individualIKs[i].typeIK.RemoveAt(individualIKs[i].typeIK.Count-1);
                        }
                        
                        bool yellowAnimColor = false;
                        bool greenAnimColor = false;
                        int animStatus = 0;
                        for(int j = 0; j < 4; j++)
                        {
                            int typeStatus = 0;
                            if(individualIKs[i].typeIK.Count <= j)
                            {
                                individualIKs[i].typeIK.Add(new ActiveIK());
                            }
                            
                            if(individualIKs[i].typeIK[j].dynamicIKs == null)
                            {
                                individualIKs[i].typeIK[j].dynamicIKs = new List<IKAttachment>();
                            }

                            int status = 0;
                            //Default and first IK
                            if(individualIKs[i].typeIK[j].defaultIK.iKAttachment != null)
                            {
                                if(individualIKs[i].typeIK[j].defaultIK.useLocation || individualIKs[i].typeIK[j].defaultIK.useRotation)
                                {
                                    status = 2;
                                }else{
                                    status = 1;
                                }
                            }else{
                                if(individualIKs[i].typeIK[j].defaultIK.useLocation || individualIKs[i].typeIK[j].defaultIK.useRotation)
                                {
                                    status = 1;
                                }else{
                                    status = 0;
                                }
                            }
                            
                            typeStatus = status;
                            
                            individualIKs[i].typeIK[j].defaultIK.status = status;
                            
                            if(Application.isPlaying)
                            {
                            
                                if(individualIKs[i].typeIK[j].defaultIK.iKAttachment != null)
                                {
                                    individualIKs[i].typeIK[j].defaultIK.initialPos = individualIKs[i].typeIK[j].defaultIK.iKAttachment.localPosition;
                                    individualIKs[i].typeIK[j].defaultIK.initialRot = individualIKs[i].typeIK[j].defaultIK.iKAttachment.localRotation;
                                }
                            }
                            
                            //Dynamic IKs
                            bool yellow = false;
                            for(int k = 0; k < individualIKs[i].typeIK[j].dynamicIKs.Count; k++)
                            {
                                if(individualIKs[i].typeIK[j].dynamicIKs[k].iKAttachment != null)
                                {
                                    if(individualIKs[i].typeIK[j].dynamicIKs[k].useLocation || individualIKs[i].typeIK[j].dynamicIKs[k].useRotation)
                                    {
                                        status = 2;
                                    }else{
                                        yellow = true;
                                        status = 1;
                                    }
                                }else{
                                    if(individualIKs[i].typeIK[j].dynamicIKs[k].useLocation || individualIKs[i].typeIK[j].dynamicIKs[k].useRotation)
                                    {
                                        yellow = true;
                                        status = 1;
                                    }else{
                                        status = 0;
                                    }
                                }
                                
                                individualIKs[i].typeIK[j].dynamicIKs[k].status = status;
                                
                                if(yellow)
                                {
                                    typeStatus = 1;
                                }

                                if(Application.isPlaying)
                                {
                                    if( individualIKs[i].typeIK[j].dynamicIKs[k].iKAttachment != null)
                                    {
                                        individualIKs[i].typeIK[j].dynamicIKs[k].initialPos = individualIKs[i].typeIK[j].dynamicIKs[k].iKAttachment.localPosition;
                                        individualIKs[i].typeIK[j].dynamicIKs[k].initialRot = individualIKs[i].typeIK[j].dynamicIKs[k].iKAttachment.localRotation;
                                    }
                                }
                                
                            }

                            individualIKs[i].typeIK[j].status = typeStatus;
                
                            animStatus = typeStatus;
                            
                            if(animStatus == 1)
                            {
                                yellowAnimColor = true;
                            }
                            
                            if(animStatus == 2)
                            {
                                greenAnimColor = true;
                            }
                            
                        }

                        if(greenAnimColor)
                        {
                            animStatus = 2;
                        }
                        
                        if(yellowAnimColor)
                        {
                            animStatus = 1;
                        }
                        
                        individualIKs[i].status = animStatus;
                        
                        if(individualIKs[i].status == 2)
                        {
                            individualIK = true;
                        }
         
                        if(individualIK)
                        {
                           for(int j = 0; j < 4; j++)
                           {
                                individualIKs[i].typeIK[j].dynamicIKsByTime = individualIKs[i].typeIK[j].dynamicIKs.OrderBy(w => w.time).ToList();
                           }
                           
                        }
                    }     
                }
            }  
        }
        
        AnimatorStateInfo stateInfo;
        AnimatorClipInfo[] clipInfo;
        float lastTime = 0f;
        float iTime = 1f;
        void OnAnimatorIK(int layerIndex){
            
            stateInfo = animator.GetCurrentAnimatorStateInfo(layerIndex);
            clipInfo = animator.GetCurrentAnimatorClipInfo(0);
            
            //Set IK working for global IKs
            if(useGlobalIK)
            {
                for(int i = 0; i < globalIKs.Count; i++)
                {
                    if(globalIKs[i].iKAttachment != null)
                    {
                        SetIK((IKType)i, globalIKs[i]);
                    }
                }
            }
            
            //Set IK working for individual IKs
            if(individualIK)
            {   

                //Get current time of the animation taking into account if animation is a loop
                float currentTime = stateInfo.normalizedTime - Mathf.Floor(stateInfo.normalizedTime);
      
                for(int i = 0; i < individualIKs.Count; i++)
                {   
                    if(individualIKs[i].status == 2)
                    {
                        //Reset values when animation finished
                        if(currentTime < lastTime)
                        {
                            for(int j = 0; j < individualIKs[i].typeIK.Count; j++)
                            {
                                if(individualIKs[i].typeIK[j].dynamicIK)
                                {   if(individualIKs[i].typeIK[j].status == 2)
                                    {
                                        for(int k = 0; k < individualIKs[i].typeIK[j].dynamicIKsByTime.Count; k++)
                                        {
                                            individualIKs[i].typeIK[j].dynamicIKsByTime[k].weight = 0;
                                            individualIKs[i].typeIK[j].dynamicIKsByTime[k].played = false;
                                          
                                            individualIKs[i].typeIK[j].dynamicIKsByTime[k].iKAttachment.localPosition = individualIKs[i].typeIK[j].dynamicIKsByTime[k].initialPos;
                                            individualIKs[i].typeIK[j].dynamicIKsByTime[k].iKAttachment.localRotation = individualIKs[i].typeIK[j].dynamicIKsByTime[k].initialRot;
                                            
                                        }
                                    }
                                }
                            }
                        }
                        
                        //Get the current animation playing
                        if(clipInfo[0].clip.name == animations[i].name)
                        {
                            individualIKs[i].isPlaying = true;
                            for(int j = 0; j < individualIKs[i].typeIK.Count; j++)
                            {
                                if(individualIKs[i].typeIK[j].dynamicIK)
                                {
                                    if(individualIKs[i].typeIK[j].dynamicIKsByTime.Count > 0)
                                    {
                                        if(individualIKs[i].typeIK[j].status == 2)
                                        {
                                            int kTime = 0;
                                            for(int k = 0; k < individualIKs[i].typeIK[j].dynamicIKsByTime.Count; k++)
                                            {
                                                
                                                if(individualIKs[i].typeIK[j].dynamicIKsByTime[k].iKAttachment != null)
                                                {

                                                    if(iTime <= currentTime)
                                                    {
                                                        iTime = 1f;
                                                    }
                                                    
                                                    if(individualIKs[i].typeIK[j].dynamicIKsByTime[k].time < iTime)
                                                    {
                                                        if(currentTime >= individualIKs[i].typeIK[j].dynamicIKsByTime[k].time)
                                                        {
                                                            kTime = k;
                                                            iTime = individualIKs[i].typeIK[j].dynamicIKsByTime[k].time;
                                                        }
                                                    }
                                                }
                                            }
                                            
                                            if(individualIKs[i].typeIK[j].dynamicIKsByTime.Count > 0)
                                            {
                                            
                                                if(currentTime <= individualIKs[i].typeIK[j].dynamicIKsByTime[kTime].time)
                                                {
                                                    //First dynamic IK is always the default IK;
                                                    SetIK((IKType)j, individualIKs[i].typeIK[j].defaultIK);
                                                }else{
                                                    
                                                    if(!individualIKs[i].typeIK[j].dynamicIKsByTime[kTime].played)
                                                    {

                                                        individualIKs[i].typeIK[j].dynamicIKsByTime[kTime].played = true;
                                                        if(kTime+1 < individualIKs[i].typeIK[j].dynamicIKsByTime.Count)
                                                        {
                                                            individualIKs[i].typeIK[j].dynamicIKsByTime[kTime].totalTime = individualIKs[i].typeIK[j].dynamicIKsByTime[kTime+1].time;
                                                        }else{
                                                            individualIKs[i].typeIK[j].dynamicIKsByTime[kTime].totalTime = 1;
                                                        }  
                                                    }                

                                                    //Second and following Dynamic IKs
                                                    //We need the previous IK for interpolate position/rotation
                                                    if(kTime-1 < 0)
                                                    {//Previous IK is first IK (DefaultIK)
                                                        SmoothSetIK((IKType)j, individualIKs[i].typeIK[j].dynamicIKsByTime[kTime], individualIKs[i].typeIK[j].defaultIK);
                                                    }else{
                                                        SmoothSetIK((IKType)j, individualIKs[i].typeIK[j].dynamicIKsByTime[kTime], individualIKs[i].typeIK[j].dynamicIKsByTime[kTime-1]);
                                                    }
                                                }
                                            }                                    
                                        }
                                    }else{
                                        //No dynamic IKs found, use only default IK if available
                                        if(individualIKs[i].typeIK[j].defaultIK.status == 2)
                                        {
                                            SetIK((IKType)j, individualIKs[i].typeIK[j].defaultIK);
                                        }
                                    }
                                }else{
                                    //Use Dynamic IK disabled, use only default IK if available
                                    if(individualIKs[i].typeIK[j].defaultIK.status == 2)
                                    {
                                        SetIK((IKType)j, individualIKs[i].typeIK[j].defaultIK);
                                    }
                                    
                                }
                            }
                        }else{
                            individualIKs[i].isPlaying = false;
                        }

                    }
                }
                lastTime = currentTime;
            }
        }
        
        void SmoothSetIK(IKType type, IKAttachment ik, IKAttachment prevIK)
        {
            if(ik.iKAttachment == null)
            {
                return;
            }
        
            //Interpolate location/rotation from previous IK to the active IK
            ik.iKAttachment.localPosition = Vector3.Lerp(prevIK.initialPos, ik.initialPos, ik.weight);
            ik.iKAttachment.localRotation = Quaternion.Lerp(prevIK.initialRot, ik.initialRot, ik.weight);
       
            //Make the IK work
            SetIK(type, ik);
        
            //Increase interpolating parameter based on the DynamicIK speed
            if(ik.weight < 1)
            {
                ik.weight += Time.deltaTime * ik.speed; 
            }
        }
        
        void SetIK(IKType type, IKAttachment ik, float weight = 1f)
        {
            switch(type)
            {
                case IKType.RightHand:
                
                    if(ik.useLocation)
                    {
                        animator.SetIKPositionWeight(AvatarIKGoal.RightHand, weight);
                        animator.SetIKPosition(AvatarIKGoal.RightHand, ik.iKAttachment.position);         
                    }
                    
                    if(ik.useRotation)
                    {
                        animator.SetIKRotationWeight(AvatarIKGoal.RightHand, weight);     
                        animator.SetIKRotation(AvatarIKGoal.RightHand, ik.iKAttachment.rotation);
                    }

                break;
                
                case IKType.LeftHand:
                    
                    if(ik.useLocation)
                    {
                        animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, weight);
                        animator.SetIKPosition(AvatarIKGoal.LeftHand, ik.iKAttachment.position);
                    }
                    
                    if(ik.useRotation)
                    {
                        animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, weight);                        
                        animator.SetIKRotation(AvatarIKGoal.LeftHand, ik.iKAttachment.rotation);
                    }
                break;
                
                case IKType.RightFoot:
                   
                    if(ik.useLocation)
                    {
                        animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, weight);
                        animator.SetIKPosition(AvatarIKGoal.RightFoot, ik.iKAttachment.position);         
                    }
                    
                    if(ik.useRotation)
                    {
                        animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, weight); 
                        animator.SetIKRotation(AvatarIKGoal.RightFoot, ik.iKAttachment.rotation);                     
                    }
                break;
                
                case IKType.LeftFoot:
                    
                    if(ik.useLocation)
                    {
                        animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, weight);
                        animator.SetIKPosition(AvatarIKGoal.LeftFoot, ik.iKAttachment.position);     
                    }
                    
                    if(ik.useRotation)
                    {
                        animator.SetIKRotation(AvatarIKGoal.LeftFoot, ik.iKAttachment.rotation);
                        animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, weight);  
                    }
                break;
            }
        }
 
    }
}
