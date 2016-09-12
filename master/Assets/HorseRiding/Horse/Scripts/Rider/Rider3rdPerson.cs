using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;

public class Rider3rdPerson : Rider
{
    [Space]
    [Space]
    
    UnityStandardAssets.Cameras.FreeLookCam CharacterCamera;
    int MountLayer;
    Vector3 Lastpos;


    #region IK VARIABLES    
    Transform leftIKFoot = null;
    Transform rightIKFoot = null;
    Transform leftIKKnee = null;
    Transform rightIKKnee = null;

    float leftIKFootWeight = 0f;
    float rightIKFootWeight = 0f;
    #endregion


    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    void Start()
    {
        hashs = GetComponent<HashIDs>();
        colliderRider = GetComponent<Collider>();
        RigidRider = GetComponent<Rigidbody>();
        MountLayer = anim.GetLayerIndex("Mounted");

        if (CharacterCamera) CharacterCamera.SetTarget(transform);
      
        if (StartMounted)  AlreadyMounted();
    }

    public void AlreadyMounted()
    {
        if (HorseToMount)
        {
            anim.SetLayerWeight(MountLayer, 1);
            findHorse(HorseToMount);
            EnableMounting();
            anim.Play("Idle Blend", MountLayer);
            StartCoroutine(iTCP());
        }
    }

    IEnumerator iTCP()
    {
        yield return 1;
             ActivateComponents(true);
        yield return 1;
        EnableMounting();
    }

    public override void EnableMounting()
    {
        base.EnableMounting();

        //Getting the correct Rotation for the rider
        transform.rotation = RiderLink.rotation;
        transform.localRotation = Quaternion.identity;
        transform.Rotate(new Vector3(0, -90, -90));
        transform.Rotate(RotationOffset);

        //disable ThirdPersonControllers

        ActivateComponents(false);

        if (CustomTPC) ThirdPersonCompatible(true);
   
        //Camera Change to horse 
        if (CharacterCamera) CharacterCamera.SetTarget(HorseCntler.transform);
    }

    public override void DisableMounting(Vector3 Laspos)
    {
        Lastpos = Laspos;
        base.DisableMounting(Laspos);

        //LastPosition on the ground when finish unmounting
        Ray LowerPoint = new Ray(Laspos + Vector3.up, -Vector3.up);
        RaycastHit hitray;

        if (Physics.Raycast(LowerPoint, out hitray, 2f, LayerMask.GetMask("Default")))
        {
            transform.position = new Vector3(Laspos.x, hitray.point.y, Laspos.z);
        }
        else transform.position = Laspos;

        //Rotation looking the horse
        Vector3 link = HorseCntler.RidersLink.position;
        Vector3 direction = new Vector3(link.x, transform.position.y, link.z);
        transform.LookAt(direction, Vector3.up);


        //Camera Change to Player
        if (CharacterCamera) CharacterCamera.SetTarget(transform);

        //If you are using Kubold, Invector or Opsive Controllers
        if (CustomTPC) ThirdPersonCompatible(false);

        IsInHorse = false;
        anim.SetLayerWeight(MountLayer, 0);
        StartCoroutine(Stybilize());
    }

    void ThirdPersonCompatible(bool active)
    {
        SendMessage("isMounted", active, SendMessageOptions.DontRequireReceiver);
    }

    // -------------Get the rigid body on sleep for remove unwanted flicker rigid position
    IEnumerator Stybilize()
    {
        yield return
         transform.position = new Vector3(Lastpos.x, transform.position.y, Lastpos.z);
        yield return
         
        transform.position = new Vector3(Lastpos.x, transform.position.y, Lastpos.z);
        anim.applyRootMotion = true;
        //Enable ThirdPersonControllers
        ActivateComponents(true);
    }

    void Update()
    {
        if (HorseCntler != null)
        {
            stand = anim.GetBool(HashIDs.standHash);
            if (Can_Mount && !Mounted)
            {
            #if !MOBILE_INPUT
                if (Input.GetKeyDown(MountKey))
            #else
                if (CrossPlatformInputManager.GetButtonDown(MountInput)) //Needed to add on Edit/ProjectSettings/Input   "Mount"  
            #endif
                {
                    //Send to the Horse controller that mounted is active
                    Mounted = true;
                    HorseCntler.Mounted = Mounted;
                    HorseCntler.Mountedside = Mountedside;
                    anim.SetLayerWeight(MountLayer, 1);
                }
            }
            else
            {
                if (Mounted)
                {
                    #if !MOBILE_INPUT
                    if (Input.GetKeyDown(MountKey) && HorseCntler.Stand)
                    #else
                    if (CrossPlatformInputManager.GetButtonDown(MountInput) && HorseCntler.Stand) //Needed to add on Edit/ProjectSettings/Input   "Mount"  
                    #endif
                    {
                        Mounted = false;
                        HorseCntler.Mounted = false;
                    }
                }
            }
            anim.SetBool("Mount", Mounted);
            anim.SetBool("MountSide", Mountedside);
        }
    }
    // ----------------------------------------------Find a horse when triggers the mount colliders on a near horse---------------------------------------
    public override void findHorse(HorseController horse)
    {
        HorseCntler = horse;
        if (HorseCntler == null)
        {
            RiderLink = null;
            leftIKFoot = null;
            rightIKFoot = null;
            leftIKKnee = null;
            rightIKKnee = null;
        }
        else
        {
            RiderLink = HorseCntler.RidersLink;
            leftIKFoot = HorseCntler.LeftIK;
            rightIKFoot = HorseCntler.RightIK;
            leftIKKnee = HorseCntler.LeftKnee;
            rightIKKnee = HorseCntler.RightKnee;
        }
    }

    //----------------------IK Feet Adjusment-------------------------------------------------------------------------------------------------------------
    void OnAnimatorIK()
    {
        if (HorseCntler != null)
        {
            //linking the weights to the animator
            if (IsInHorse)
            {
                leftIKFootWeight = 1f;
                rightIKFootWeight = 1f;

                if (anim.GetCurrentAnimatorStateInfo(MountLayer).IsTag("Mounting") || anim.GetCurrentAnimatorStateInfo(MountLayer).IsTag("Unmounting"))
                {
                    leftIKFootWeight = anim.GetFloat("IKLeftFoot");
                    rightIKFootWeight = anim.GetFloat("IKRightFoot");
                }
                if (anim.GetNextAnimatorStateInfo(MountLayer).IsName("Idle Blend"))
                {
                    leftIKFootWeight = 1f;
                    rightIKFootWeight = 1f;
                }

                //setting the weight
                anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, leftIKFootWeight);
                anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, rightIKFootWeight);

                anim.SetIKHintPositionWeight(AvatarIKHint.LeftKnee, leftIKFootWeight);
                anim.SetIKHintPositionWeight(AvatarIKHint.RightKnee, rightIKFootWeight);

                //Knees
                anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, leftIKFootWeight);
                anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, rightIKFootWeight);

                //setting the IK Positions
                anim.SetIKPosition(AvatarIKGoal.LeftFoot, leftIKFoot.position);
                anim.SetIKPosition(AvatarIKGoal.RightFoot, rightIKFoot.position);

                //Knees
                anim.SetIKHintPosition(AvatarIKHint.LeftKnee, leftIKKnee.position);
                anim.SetIKHintPosition(AvatarIKHint.RightKnee, rightIKKnee.position);

                //setting the IK Rotations
                anim.SetIKRotation(AvatarIKGoal.LeftFoot, leftIKFoot.rotation);
                anim.SetIKRotation(AvatarIKGoal.RightFoot, rightIKFoot.rotation);
            }
        }
        else
        {
            anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0f);
            anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0f);

            anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 0f);
            anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, 0f);
        }
    }
}
