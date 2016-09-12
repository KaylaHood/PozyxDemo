using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;
using System;

#if UFPS_NETWORK
/*
 * The PhotonView is added by UFPS on spawn, so don't put as a requirement
*/
public class Rider : Photon.MonoBehaviour
#else
public class Rider : MonoBehaviour
#endif
{
    #region Variables

    [HideInInspector]
    public bool Mounted;
    [HideInInspector]
    public bool Can_Mount;
    [HideInInspector]
    public bool IsInHorse = false;
    [HideInInspector]
    public bool Mountedside;
    [HideInInspector]
    public HorseController HorseCntler;
    [HideInInspector]
    public Transform RiderLink;

#if UFPS
    [HideInInspector]
    public bool IsFirstPersonOnMount;
    [HideInInspector]
    public int LastWeaponEquipped = 0;                     // used to store the id of our weapon so we can reequip it
   
#endif

    #region RiderComponents
    protected Animator anim;
    protected AnimatorStateInfo LayercurrentState;
    protected Rigidbody RigidRider;
    protected Collider colliderRider;
    protected HashIDs hashs;
    #endregion
    [Tooltip("Enable this if you are want to start mounted on a horse")]
    public bool StartMounted;
    [Tooltip("Reference the horse you want to start mounted on")]
    public HorseController HorseToMount;
    public KeyCode MountKey = KeyCode.F;


    #region Public Adjust Position and Rotation Variables when Riders mount the horse
    public Vector3 PositionOffset;
    public Vector3 RotationOffset;
    #endregion

    [Space]

    [Tooltip("Enable this if you are using Opsive or Invector 3rd Person Controller")]
    public bool CustomTPC;

    [Tooltip("Keep Referenced Scripts enable while is mounted on the horse")]
    //public MonoBehaviour[] KeepActive;
//#if UFPS
//    public string[] KeepActive; //      CWL --Changed to string array from MonoBehaviour array because I was having issues losing the links
                                //      and this allows me to link in scripts that don't exist on the prefab, but are added at runtime
//#else
    public MonoBehaviour[] KeepActive;
//#endif

    [HideInInspector]
    public bool stand;
    #endregion

    //--------------------------Setting the animator on the Root Game Object-------------------------------------------------------------------------
    protected void SetAnimator()
    {
        anim = GetComponent<Animator>();
        if (anim == null) anim = GetComponentInChildren<Animator>();
    }



    //--------------------------Mount Logic-----------------------------------------------------------------------------------------------------------    
    public virtual void EnableMounting()
    {
        //Send to the Horse controller that mounted is active
        Mounted = true;
        HorseCntler.Mounted = Mounted;
        HorseCntler.Mountedside = Mountedside;

        //Deactivate stuffs for the Rider's Rigid Body
#if !UFPS
        RigidRider.useGravity = false;
        RigidRider.isKinematic = true;
        colliderRider.enabled = false;
#endif
        IsInHorse = true;

        //Linking the Rider to the horse
        transform.parent = RiderLink;
        transform.position = RiderLink.position;
        transform.localPosition = transform.localPosition + PositionOffset;
    }

    //------------------------- UnMount Logic---------------------------------------------------------------------------------------------------------    
    public virtual void DisableMounting(Vector3 Laspos)
    {
        Mounted = false;

        //Send to the horse he is no longer mounted
        HorseCntler.Mounted = Mounted;
        HorseCntler.Mountedside = Mountedside;

        //Unlinking the rider to the horse
        transform.parent = null;


        //Reactivate stuffs for the Rider's Rigid Body
#if !UFPS
        RigidRider.useGravity = true;
        RigidRider.isKinematic = false;
        colliderRider.enabled = true;
#endif
        IsInHorse = false;

    }

    //-----------------------------------IN HERE (DE)ACTIVATE THE 3RD PERSON COMPONENTS---------------------------------------------------------------
    protected void ActivateComponents(bool enabled)
    {

        MonoBehaviour[] AllComponents = GetComponents<MonoBehaviour>();


        foreach (MonoBehaviour item in AllComponents)
        {
            //if you want to add a Script to not be deactivate while Mounting Added in here
            if (!(item is Rider) && !(item is HashIDs))
            {
                bool keepactive = false;

                //Keep active custom scripts
//#if UFPS
               // foreach (string keep in KeepActive)
//#else
                foreach (MonoBehaviour keep in KeepActive)
//#endif
                {
//#if UFPS
//                    if (item.GetType() == Type.GetType(keep)) //CWL - modified to use string array because of issue losing links when mounted
//#else
                    if (item == keep)
//#endif
                    {
                        keepactive = true;
                        break;
                    }
                }

                if (!keepactive)
                {
                    item.enabled = enabled;
                }
            }
        }

#if UFPS
        //Disable/enable some scripts that are not on the parent game object
        //Disable/enable the body animator for UFPS, otherwise our player won't update to the correct position on the horse
        vp_BodyAnimator bodyAnimator = GetComponentInChildren<vp_BodyAnimator>();
        if (bodyAnimator)
        {
            bodyAnimator.enabled = enabled;
        }

        //Disable/enable the vp_PlayerFootFXHandler - we are on a horse, we don't need it at the player level :-)
        vp_PlayerFootFXHandler playerFootFXHandler = GetComponentInChildren<vp_PlayerFootFXHandler>();
        if (playerFootFXHandler)
        {
            playerFootFXHandler.enabled = enabled;
        }

        //Disable/enable first person camera control to keep our Torso from twisting around
#if UFPS_NETWORK
        if (!vp_Gameplay.IsMultiplayer || (photonView && photonView.isMine))
        {
#endif
            vp_FPCamera fpCamera = GetComponentInChildren<vp_FPCamera>();
            if (fpCamera)
            {
                fpCamera.enabled = enabled;
            }
#if UFPS_NETWORK
        }
#endif

#endif
    }

    // ----------------------------------------------Find a horse when triggers the mount colliders on a near horse---------------------------------------
    public virtual void findHorse(HorseController horse)
    {
        HorseCntler = horse;

        if (HorseCntler == null)
        {
            RiderLink = null;
        }
        else
        {
            RiderLink = HorseCntler.RidersLink;
        }
    }

}