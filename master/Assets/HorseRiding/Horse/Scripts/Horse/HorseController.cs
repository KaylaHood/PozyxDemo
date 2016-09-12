using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;


#if UFPS_NETWORK
[RequireComponent(typeof(PhotonView))]
public class HorseController : Photon.MonoBehaviour
#else
public class HorseController : MonoBehaviour
#endif
{
    public enum HorseMeshType
    {
        Horse1 = 0, Horse2 = 1, BigHorse = 2, PolyArt = 3, MineCraft = 4, HorseForHeroes = 5, Custom = 6
    }

    #region Variables 

    #region Connection with Rider Variables

    [Tooltip("Enable this if you want to control the horse alone, without requiring mounting")]
    public bool PlayerControlled;

    [Tooltip("This activate the animator layer that contains the horse's bones offset, in order to use the same animation. Custom is for use your own Horse Model")]
    public HorseMeshType horseType;

    [HideInInspector]
    public bool Mounted;
    [HideInInspector]
    public bool Mountedside;
    [HideInInspector]
    public Animator RiderAnimator; // Rider's Animator to control both Sync animators from here
    #endregion

    #region Horse Components 
#if UFPS
    [Header("UFPS")]
    [Tooltip("This is the mass of the horse rigidbody when a player is mounted on it.")]
    public float MountedMass = 10f;
    [Tooltip("This is the mass of the horse rigidbody when a player is not mounted on it")]
    public float DismountedMass = 1000f;
#endif
    [HideInInspector]
    public Animator anim;
    [HideInInspector]
    public Rigidbody horseRigidBody;
    private Transform HorseTransform;
    private BoxCollider horseCollider;
    #endregion

    #region Animator Parameters Variables
    //States Variables
    private bool
         gallop,
         trot,
         walk,
         swim,
         death,
         fall,
         fallback,
         sleep,
         jump,
         shift, //Cantering ,Sprint, turn 180
         attack1,
         IsInWaterHip,
         IsInWater,
         stand,
         fowardPressed,
         fly,// Coming Soon Pegassus
         speed1,
         speed2,
         speed3;

    private int
        sleepCount,
        horseInt;

    private float
     speed,
     direction,
     maxSpeed = 1f,//1 Walking, 2 Trotting, 3 Cantering, 4 Galloping, 5 Sprinting
     horizontal,
     vertical,
     horsefloat,
     maxHeight,
     jumpingCurve,
     scaleFactor;
    #endregion

    [Header("Custom Options")]

    public LayerMask GroundLayer;
    [Tooltip("Add more rotation to the turn animations")]
    [Range(0, 100)]

    public float TurnSpeed = 5;

    [Tooltip("Amount of idle cycles before go to sleep state")]
    [Range(1, 100)]
    public int GoToSleep = 10;

    [Tooltip("Max Distance for the horse to recover from a big fall, Greater value: The horse will still running after falling from a great height")]
    [Range(1, 100)]
    public float recoverByFallDist;

    [Tooltip("Smoothness for snapping to ground, Higher value Harder Snap")]
    [Range(1, 100)]
    public float SnapToGround = 20f;


    [Header("Swim Options")]
    [Range(1, 10)]
    public float swimSpeed = 1f;
    [Range(1, 100)]
    public float SwimTurnSpeed = 1f;



    #region FixTransform_Variables

    const RigidbodyConstraints restoreConstraints = RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezePositionY;

    RaycastHit
        hit_Hip,
        hit_Chest,
        WaterHitCenter;

    Vector3
        Horse_Hip,
        Horse_Chest,
        ColliderCenter,
        rigidVelocity;

    const float fallingMultiplier = 1.6f;

    float
        heightHorse,
        waterDistanceHip,
        distanceChest,
        distanceHip,
        angleTerrain,
        inclination,
        waterlevel,
        fallheight;

    Pivots[] pivots;
    #endregion

#if UFPS
    #region Modify_the_Max_Speed_Variables
    float WalkSpeed = 1f;
    float TrotSpeed = 2f;
    float GallopSpeed = 5f;
    #endregion
#endif

    #region Transform feet and mount on the Horse 
    [Header("Reference For Rider IKs Points")]
    public Transform RidersLink;   // Reference for the RidersLink Bone
    public Transform LeftIK;       // Reference for the LeftFoot correct position on the mount
    public Transform RightIK;      // Reference for the RightFoot correct position on the mount
    public Transform LeftKnee;     // Reference for the LeftKnee correct position on the mount
    public Transform RightKnee;    // Reference for the RightKnee correct position on the mount
    #endregion

    #endregion
    #region Properties       
    public bool Stand
    {
        get { return this.stand; }
    }
    public float MaxFallHeight
    {
        set { fallheight = value; }
        get { return this.fallheight; }
    }
    public bool Sleep
    {
        set { sleep = value; }
    }

    public int SleepCount
    {
        set { sleepCount = value; }
        get { return this.sleepCount; }
    }
    public bool isFalling
    {
        // set { falling = value; }
        get { return this.fall; }
    }
    public int HorseInt
    {
        set { horseInt = value; }
        get { return this.horseInt; }
    }

    public float HorseFloat
    {
        set { horsefloat = value; }
        get { return this.horsefloat; }
    }


    public float Horizontal
    {
        get { return horizontal; }
        set { horizontal = value; }
    }

    public float Vertical
    {
        get { return vertical; }
        set { vertical = value; }
    }
    public bool Shift
    {
        get { return shift; }
        set { shift = value; }
    }
    public bool Attack1
    {
        get { return attack1; }
        set { attack1 = value; }
    }

    public bool Jump
    {
        get { return jump; }
        set { jump = value; }
    }

    public bool Death
    {
        get { return death; }
        set { death = value; }
    }

    public bool Speed1
    {
        get { return speed1; }
        set { speed1 = value; }
    }

    public bool Speed2
    {
        get { return speed2; }
        set { speed2 = value; }
    }

    public bool Speed3
    {
        get { return speed3; }
        set { speed3 = value; }
    }

    public bool FowardPressed
    {
        get { return fowardPressed; }
        set { fowardPressed = value; }
    }

    #endregion

#if UFPS
    vp_FPPlayerEventHandler m_FPPlayer = null;
    vp_FPPlayerEventHandler FPPlayer
    {
        get
        {
            if (m_FPPlayer == null && RiderAnimator != null)
                m_FPPlayer = RiderAnimator.gameObject.GetComponentInParent<vp_FPPlayerEventHandler>();
            return m_FPPlayer;
        }
    }
#endif

    // Use this for initialization
    // Use this for initialization
    void Start()
    {
        anim = GetComponent<Animator>();
        HorseTransform = transform;
        horseCollider = GetComponent<BoxCollider>();
        horseRigidBody = GetComponent<Rigidbody>();
        ColliderCenter = horseCollider.center;
        pivots = GetComponentsInChildren<Pivots>(); //Pivots are Strategically Transform objects use to cast rays used by the horse
        scaleFactor = HorseTransform.localScale.y;  //TOTALLY SCALABE HORSE
        heightHorse = pivots[0].transform.localPosition.y * scaleFactor; // Height from Chest to ground
        anim.SetInteger("HorseType", (int)horseType); //Adjust the layer for the curret horse Type
        horseRigidBody.drag = 1;
        sleepCount = 0;
    }

    //----------------------linking  the Parameters Horse && Rider------------------------------------------------------------------------

    void LinkingAnimator(Animator anim_)
    {
        jumpingCurve = anim.GetFloat("JumpCurve");
        anim_.SetFloat(HashIDs.sspeedHash, speed * vertical);
        anim_.SetFloat(HashIDs.horizontalHash, direction);
        anim_.SetBool(HashIDs.shiftHash, shift);
        anim_.SetBool(HashIDs.galopingHash, gallop);
        anim_.SetBool(HashIDs.trottingHash, trot);
        anim_.SetBool(HashIDs.standHash, stand);
        anim_.SetBool(HashIDs.jumpHash, jump);
        anim_.SetBool(HashIDs.fowardHash, fowardPressed);
        anim_.SetBool(HashIDs.SwimHash, swim);
        anim_.SetFloat(HashIDs.inclinationHash, inclination);
        anim_.SetBool(HashIDs.fallingHash, fall);
        anim_.SetBool(HashIDs.fallingBackHash, fallback);
        anim_.SetBool(HashIDs.sleepHash, sleep);
        anim_.SetBool(HashIDs.attackHash, attack1);
        anim_.SetInteger(HashIDs.IntHash, horseInt);


        if (fall || isJumping()) anim_.SetFloat(HashIDs.FloatHash, horsefloat);

        if (death) anim_.SetTrigger(HashIDs.deathHash);
    }

    //--Add more Rotations to the current turn animations  using the public turnSpeed float--------------------------------------------
    void TurnAmount()
    {
        //For going Foward and Backward
        float turnAmount = TurnSpeed;
        int dir = 1;

        if (vertical < 0) dir = -1;
        if (swim) turnAmount = SwimTurnSpeed;

        HorseTransform.Rotate(HorseTransform.up, turnAmount * 3 * horizontal * dir * Time.deltaTime);

        //Add Speed to current animation foward swim
        if (swim && vertical > 0)
        {
            HorseTransform.position = Vector3.Lerp(HorseTransform.position, HorseTransform.position + HorseTransform.forward * swimSpeed * vertical / 2, Time.deltaTime);
        }
    }

    //--------The Collider is not touching the floor to avoid collision with small objects, Here the Height is fixed , Terrain Compatible----------
    void FixPosition()
    {
        //Position to cast a ray downward from the  Horse Chest and Horse Hip.
        Horse_Hip = pivots[0].transform.position;
        Horse_Chest = pivots[1].transform.position;

        inclination = ((Horse_Chest.y - Horse_Hip.y) / 2) / scaleFactor;  //Calculate the slope  between front and rear legs

        Vector3 JumpingCurve = new Vector3(0, jumpingCurve, 0);

        //Ray From Horse Hip to the ground
        if (Physics.Raycast(Horse_Hip + JumpingCurve, -HorseTransform.up, out hit_Hip, (heightHorse + jumpingCurve) * fallingMultiplier * scaleFactor, GroundLayer))
        {
            Debug.DrawRay(Horse_Hip + JumpingCurve, -HorseTransform.up * heightHorse * scaleFactor * fallingMultiplier, Color.green);
            distanceHip = hit_Hip.distance - jumpingCurve;
        }
        else
        {
            fallback = true;
        }

        //Ray From Horse Chest to the ground
        if (Physics.Raycast(Horse_Chest + JumpingCurve, -HorseTransform.up, out hit_Chest, (heightHorse + jumpingCurve) * fallingMultiplier * scaleFactor, GroundLayer))
        {
            distanceChest = hit_Chest.distance - jumpingCurve;
            Debug.DrawRay(Horse_Chest + JumpingCurve, -HorseTransform.up * heightHorse * scaleFactor * fallingMultiplier, Color.green);
        }
        else
        {
            fall = true;
        }

        if (!swim)
        {
            //------------------------------------------------Terrain Adjusment-----------------------------------------
            //----------Calculate the Align vector of the terrain------------------
            Vector3 direction = (hit_Chest.point - hit_Hip.point).normalized;
            Vector3 Side = Vector3.Cross(Vector3.up, direction).normalized;
            Vector3 SurfaceNormal = Vector3.Cross(direction, Side).normalized;
            angleTerrain = Vector3.Angle(HorseTransform.up, SurfaceNormal);

            // ------------------------------------------Orient To Terrain-----------------------------------------  
            Quaternion finalRot = Quaternion.FromToRotation(HorseTransform.up, SurfaceNormal) * horseRigidBody.rotation;

            // If the horse is falling smoothly aling with the horizontal
            if ((isJumping(0.55f, true)) || fallback)
            {
                finalRot = Quaternion.FromToRotation(HorseTransform.up, Vector3.up) * horseRigidBody.rotation;
                HorseTransform.rotation = Quaternion.Lerp(HorseTransform.rotation, finalRot, Time.deltaTime * 5f);
            }
            else
            {
                // if the terrain changes hard smoothly adjust to the terrain 
                if (angleTerrain > 2 && !stand)
                {
                    HorseTransform.rotation = Quaternion.Lerp(HorseTransform.rotation, finalRot, Time.deltaTime * 10f);
                }
                else
                {
                    HorseTransform.rotation = finalRot;
                }
            }

            float distance = distanceHip;
            float realsnap = SnapToGround; // change in the inspector the  adjusting speed for the terrain

            if (isJumping(0.5f, false)) //Calculate from the front legss
            {
                //calculate the adjust distance from the front legs if is finishing jumping. Solution for jumping High places at the same distance
                distance = distanceChest;
                realsnap = SnapToGround / 4;
            }

            //-----------------------------------------Snap To Terrain-------------------------------------------
            if (distance != heightHorse)
            {
                float diference = heightHorse - distance;
                HorseTransform.position = Vector3.Lerp(HorseTransform.position, HorseTransform.position + new Vector3(0, diference, 0), Time.deltaTime * realsnap);
            }
        }
    }

    public void Falling()
    {
        //if the horse stay stucked while falling move foward  ... basic solution
        if (fall && horseRigidBody.velocity.magnitude < 0.1 && !swim)
        {
            HorseTransform.position = Vector3.Lerp(HorseTransform.position, HorseTransform.position + HorseTransform.forward * 2, Time.deltaTime);
        }

        RaycastHit offGround; //Front Falling Ray

        Vector3 FallingVectorFront = pivots[2].transform.position; // get the Transform Falling from vector within the horse hierarchy
        Debug.DrawRay(FallingVectorFront, -HorseTransform.up * heightHorse * fallingMultiplier, Color.blue);

        /*   //Ignore Layer Horse 20
           var layermask = 1 << 20;
           layermask = ~layermask;*/

        if (Physics.Raycast(FallingVectorFront, -HorseTransform.up, out offGround, heightHorse * fallingMultiplier, GroundLayer))
        {
            if ((offGround.distance <= heightHorse * fallingMultiplier) && !IsInWater)
            {
                if (fall)
                {
                    fall = false;
                    if (MaxFallHeight - HorseTransform.position.y < recoverByFallDist * scaleFactor) /**/ horseInt = -1;  // If the fall distance is too big changue to recoverfall animation
                    else horseInt = 0;
                }
                horseRigidBody.constraints = restoreConstraints;
            }
        }
        else
        {
            if (!IsInWater)
            {
                fall = true;
                //--------------------Fall Blend for Stretch Front Legs while falling-----------------
                RaycastHit HeightDistance;

                if (Physics.Raycast(FallingVectorFront, -HorseTransform.up, out HeightDistance, 100f))
                {
                    if (maxHeight < HeightDistance.distance) maxHeight = HeightDistance.distance; //Calculate the MaxDistance  from the horse.

                    //Fall Blend between fall animations
                    horsefloat = Mathf.Lerp(horsefloat, HeightDistance.distance / (maxHeight - heightHorse), Time.deltaTime * 10f);
                }
            }
        }
    }

    public void Swimming()
    {
        Vector3 WaterVector = pivots[5].transform.position; //Pivot 5 Water
        
        //Front RayWater Cast
        if (Physics.Raycast(WaterVector, -HorseTransform.up, out WaterHitCenter, heightHorse * scaleFactor, LayerMask.GetMask("Water")))
        {
            Debug.DrawRay(WaterVector, -HorseTransform.up * heightHorse * scaleFactor, Color.blue);

            //if there nothing on top of the water
            if (hit_Chest.distance > WaterHitCenter.distance)
            {
                IsInWater = true;
            }
            waterlevel = WaterHitCenter.point.y;
        }
        else
        {
            IsInWater = false;
        }

        //If the horse has  water near the Neck then start swimming
        if (IsInWater) //if we hit water
        {
            if ((Horse_Chest.y < waterlevel) || (fall && !isJumping()))
            {
                swim = true;
                fall = false;
            }
            if (distanceChest < 1f * scaleFactor)  //else stop swimming when he is coming out of the water
            {
                swim = false;
                maxSpeed = 1f;
                gallop = false;
                trot = false;
            }
        }
        //-----------------------------water Adjusment-------------------------------------------
        if (swim)
        {
            fall = false;
            fallback = false;
            horseRigidBody.constraints = restoreConstraints;

            float angleWater = Vector3.Angle(HorseTransform.up, WaterHitCenter.normal);

            Quaternion finalRot = Quaternion.FromToRotation(HorseTransform.up, WaterHitCenter.normal) * horseRigidBody.rotation;

            //Smoothy rotate until is Aling with the Water
            if (angleWater > 0.1f)
                HorseTransform.rotation = Quaternion.Lerp(HorseTransform.rotation, finalRot, Time.deltaTime * 10);
            else
                HorseTransform.rotation = finalRot;

            //Smoothy Move until is Aling with the Water
            if (WaterHitCenter.transform)
                HorseTransform.position = Vector3.Lerp(HorseTransform.position, new Vector3(HorseTransform.position.x, waterlevel - heightHorse, HorseTransform.position.z), Time.deltaTime * 5f);
        }

    }

    //--------------------Adjusting the Capsule Collider when the horse Jump----------------------------------------
    void JumpingCollider()
    {
        horseCollider.center = ColliderCenter + new Vector3(0, anim.GetFloat("JumpCurve"), 0);
    }

    //--------------------------Check if the horse is in the Jumping State------------------------------------------
    bool isJumping(float normalizedtime, bool half)
    {
        if (half)  //if is jumping the first half
        {

            if (anim.GetCurrentAnimatorStateInfo(0).IsTag("Jump"))
            {
                if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime < normalizedtime)
                    return true;
            }

            if (anim.GetNextAnimatorStateInfo(0).IsTag("Jump"))
            {
                if (anim.GetNextAnimatorStateInfo(0).normalizedTime < normalizedtime)
                    return true;
            }
        }
        else //if is jumping the second half
        {
            if (anim.GetCurrentAnimatorStateInfo(0).IsTag("Jump"))
            {
                if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime > normalizedtime)
                    return true;
            }

            if (anim.GetNextAnimatorStateInfo(0).IsTag("Jump"))
            {
                if (anim.GetNextAnimatorStateInfo(0).normalizedTime > normalizedtime)
                    return true;
            }
        }


        return false;

    }
    bool isJumping()
    {
        if (anim.GetCurrentAnimatorStateInfo(0).IsTag("Jump"))
        {
            return true;
        }
        if (anim.GetNextAnimatorStateInfo(0).IsTag("Jump"))
        {
            return true;
        }
        return false;
    }
    //--------------------------Prevent From falling while going backwards from a cliff------------------------------------------
    public void FallingBackPrevent()
    {
        RaycastHit BackRay;
        Vector3 FallingVectorBack = pivots[3].transform.position;

        if (Physics.Raycast(FallingVectorBack, -Vector3.up, out BackRay, heightHorse * scaleFactor * 1.7f, GroundLayer))
        {
            fallback = false;
        }
        else
        {
            if (!swim)
            {
                fallback = true;
                if (speed * vertical < 0)
                    speed = -0.10f;
            }
        }

        //prevent to go uphill
        if (inclination > 0.3 && speed * vertical > 0)
        {
            speed = Mathf.Lerp(speed, 0, Time.fixedDeltaTime * 1f); ;
        }

        if (inclination > 0.2 && speed * vertical > 0)
        {
            speed = Mathf.Lerp(speed, 1, Time.fixedDeltaTime * 1f); ;
        }

    }

    //--------------------------------------------------------------------------------------------------------------------------
    void FixedUpdate()
    {
#if UFPS_NETWORK
        if (!vp_Gameplay.IsMultiplayer || (photonView && photonView.isMine))
        {
#endif
        JumpingCollider();

        Falling();
        Swimming();
        if (!fall)
        {
            FixPosition();
            FallingBackPrevent();
        }

#if UFPS_NETWORK
        }
#endif
    }


    void MovementSystem()
    {
        if ((horizontal != 0) || (vertical != 0)) stand = false;
        else stand = true;

#if !UFPS
        if (speed1)       //Walking
        {
            maxSpeed = 1f;
            gallop = false;
            trot = false;
        }

        if (speed2)       //Trotting
        {
            maxSpeed = 2f;
            gallop = false;
            trot = true;
        }

        if (speed3)       //Galloping
        {
            maxSpeed = 3f;
            gallop = true;
            trot = false;
        }

        if (shift)
        {
            if (trot) maxSpeed = 2.5f;   //Cantering
            if (gallop) maxSpeed = 4f;     //Sprinting 
        }
        else
        {
            if (trot) maxSpeed = 2f;     //Stop Cantering
            if (gallop) maxSpeed = 3f;     //Stop Sprinting      
        }
#endif
#if UFPS
        // Increase speed on run
        if (FPPlayer && FPPlayer.InputGetButtonDown.Send("Run"))
        {
            if (maxSpeed == WalkSpeed)
            {
                //step it up
                maxSpeed = TrotSpeed;
                gallop = false;
                trot = true;
            }
            else if (maxSpeed == TrotSpeed)
            {
                maxSpeed = GallopSpeed;
                gallop = true;
                trot = false;
            }
        }
        if (stand)
        {
            maxSpeed = WalkSpeed;
            gallop = false;
            trot = false;
        }
        if (stand)
            speed = Mathf.Lerp(speed, 0, Time.deltaTime * 2); //Smoothly reduce speed to 0
        else
#endif
            speed = Mathf.Lerp(speed, maxSpeed, Time.deltaTime * 2);            //smoothly transitions bettwen velocities
        direction = Mathf.Lerp(direction, horizontal, Time.deltaTime * 8);  //smoothly transitions bettwen directions


        if (!stand) sleepCount = 0; // Reset the sleep conter

        LinkingAnimator(anim);
        //Control the Rider Animator from here...  Syncs animations
        if (RiderAnimator) LinkingAnimator(RiderAnimator);
    }

    void Update()
    {
#if UFPS_NETWORK
        if (!vp_Gameplay.IsMultiplayer || (photonView && photonView.isMine))
		{
#endif
        if (sleep && FowardPressed) sleep = false;

        if (attack1)
        {
            horseInt = Random.Range(0, 3);                                         //Random Attack ID: ID identify the type of attack
            sleepCount = 0;
        }
#if UFPS
        if (FPPlayer && FPPlayer.InputGetButtonDown.Send("Attack"))
        {
            horseInt = Random.Range(0, 3);                                         //Random Attack ID: ID identify the type of attack
            sleepCount = 0;
        }
#endif

        if (PlayerControlled) MovementSystem();

        else if (Mounted || PlayerControlled)
        {
            TurnAmount();
            if (RiderAnimator == null)
            {
                if (GetComponentInChildren<Rider>())
#if UFPS
                    RiderAnimator = GetComponentInChildren<Rider>().transform.GetComponentInChildren<Animator>();
#else
                    RiderAnimator = GetComponentInChildren<Rider>().transform.GetComponent<Animator>();
#endif
            }
            else
            {   // if the riders finish mounting
                if (!RiderAnimator.GetCurrentAnimatorStateInfo(RiderAnimator.GetLayerIndex("Mounted")).IsTag("Mounting"))
                {
                    MovementSystem();
                }
            }
        }
        else if (RiderAnimator != null)
        {
            if (!RiderAnimator.GetCurrentAnimatorStateInfo(RiderAnimator.GetLayerIndex("Mounted")).IsTag("Unmounting"))
            {
                RiderAnimator = null;
            }
        }
#if UFPS_NETWORK
        }
#endif
    }
}
