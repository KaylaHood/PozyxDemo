using UnityEngine;
using System.Collections;

public class FallingBehavior : StateMachineBehaviour
{

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

        if (stateInfo.IsTag("Fall") && animator.applyRootMotion)
        {
            if (animator.applyRootMotion == true)
            {
                animator.applyRootMotion = false;
                animator.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationX;
            }
            animator.GetComponent<Rigidbody>().drag = 0;
        }
        if (stateInfo.IsTag("Jump"))
        {
            animator.GetComponent<HorseController>().MaxFallHeight = animator.transform.position.y;
            animator.GetComponent<HorseController>().HorseFloat = 1;
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //if the horse finish recovering activate rootmotion
        if (stateInfo.IsTag("Recover"))
        {
            animator.applyRootMotion = true;
            animator.GetComponent<Rigidbody>().drag = 0;
        }

        //if the horse finish Jumping and is not falling activate rootmotion
        if (stateInfo.IsTag("Jump"))
        {
            if (!animator.GetComponent<HorseController>().isFalling)
            {
                animator.applyRootMotion = true;
                animator.GetComponent<Rigidbody>().drag = 0;
            }
        }
    }


    // OnStateUpdate is called before OnStateUpdate is called on any state inside this state machine
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //If the horse is in the middle of the air and falls, deactivate rootmotion
        if (stateInfo.IsTag("Jump"))
        {
            //When finish Jump State Set Horse ID to 0! will changue to -1 if the fall is to high
            if (stateInfo.normalizedTime > 0.9)
            {
                animator.GetComponent<HorseController>().HorseInt = 0;
            }
        }

        //Smooth Stop when RecoverFalls
        if (stateInfo.IsTag("Recover"))
        {
            animator.applyRootMotion = false;
            if (stateInfo.normalizedTime < 0.5f)
            {
                animator.GetComponent<Rigidbody>().drag = Mathf.Lerp(animator.GetComponent<Rigidbody>().drag, 3, Time.deltaTime * 10f);
            }
            else
            {
                animator.applyRootMotion = true;
            }
        }

        if (stateInfo.IsTag("JumpDown") || stateInfo.IsTag("Fall"))
        {
            Quaternion finalRot = Quaternion.FromToRotation(animator.transform.up, Vector3.up) * animator.GetComponent<Rigidbody>().rotation;
            animator.transform.rotation = Quaternion.Lerp(animator.transform.rotation, finalRot, Time.fixedDeltaTime * 5);
            animator.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationX;
        }



    }

}
