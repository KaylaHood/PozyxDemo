using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;


public class Rider1stPerson : Rider
{
    void Start()
    {
        #if !UFPS
        RigidRider = GetComponent<Rigidbody>();
        colliderRider = GetComponent<Collider>();
        #endif
        Mounted = false;

        if (StartMounted)
        {
            AlreadyMounted();
        }
    }

    public void AlreadyMounted()
    {
        if (HorseToMount)
        {
            findHorse(HorseToMount);
            EnableMounting();
        }
    }

    public override void EnableMounting()
    {
        base.EnableMounting();
        transform.parent = HorseCntler.transform;
        transform.rotation = HorseCntler.transform.rotation;
        UnityStandardAssets.Characters.FirstPerson.RigidbodyFirstPersonController RFPC = GetComponent<UnityStandardAssets.Characters.FirstPerson.RigidbodyFirstPersonController>();
        if (RFPC)
        {
            RFPC.mouseLook.Init(transform, transform);
        }
    }

    public override void DisableMounting(Vector3 Laspos)
    {
        base.DisableMounting(Laspos);

        UnityStandardAssets.Characters.FirstPerson.RigidbodyFirstPersonController RFPC = GetComponent<UnityStandardAssets.Characters.FirstPerson.RigidbodyFirstPersonController>();
        if (RFPC)
            RFPC.mouseLook.Init(HorseCntler.transform, HorseCntler.transform);
    }

    void InHorse()
    {
        transform.position = RiderLink.position + PositionOffset;
    }


    void Update()
    {
        if (IsInHorse) InHorse();
      
        if (HorseCntler != null)
        {
            if (Can_Mount && !Mounted)
            {
#if !MOBILE_INPUT
                if (Input.GetKeyDown(MountKey))
#else
                if (CrossPlatformInputManager.GetButtonDown(MountInput)) //Needed to add on Edit/ProjectSettings/Input   "Mount"  
#endif
                {
                   EnableMounting();
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
                        DisableMounting(Vector3.zero);
                    }
                }
            }
        }
    }

    public override void findHorse(HorseController horse)
    {
        HorseCntler = horse;

        if (HorseCntler == null)  RiderLink = null;
       
        else   RiderLink = HorseCntler.RidersLink;
    }
}