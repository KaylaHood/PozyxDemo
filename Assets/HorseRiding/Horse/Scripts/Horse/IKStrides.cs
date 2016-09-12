using UnityEngine;
using System.Collections;

public class IKStrides : MonoBehaviour {
    HorseController myHorse;
    public Transform StrideHandle_L, StrideHandle_R;
    private Vector3 LocalStride_L, LocalStride_R;
    Transform riderHand_L, riderHand_R;

    // Use this for initialization
    void Start () {
        myHorse = GetComponent<HorseController>();
        LocalStride_L = StrideHandle_L.localPosition;
        LocalStride_R = StrideHandle_R.localPosition;
    }
	
	// Update is called once per frame
	void Update () {
        if (myHorse.RiderAnimator)
        {
            riderHand_L = myHorse.RiderAnimator.GetBoneTransform(HumanBodyBones.LeftHand);
            riderHand_R = myHorse.RiderAnimator.GetBoneTransform(HumanBodyBones.RightHand);

            if (!GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsTag("Idle"))
            {
                 StrideHandle_L.position = Vector3.Lerp(riderHand_L.position, riderHand_L.GetChild(0).position, 0.5f);
                 StrideHandle_R.position = Vector3.Lerp(riderHand_R.position, riderHand_R.GetChild(0).position, 0.5f);
            }
            else
            {
                StrideHandle_L.localPosition = Vector3.Lerp(StrideHandle_L.localPosition, LocalStride_L, 5 * Time.deltaTime);
                StrideHandle_R.localPosition = Vector3.Lerp(StrideHandle_R.localPosition, LocalStride_R, 5 * Time.deltaTime);
            }
        }
	
	}



}
