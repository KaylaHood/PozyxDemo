using UnityEngine;
using System.Collections;

public class Mounting : StateMachineBehaviour
{
    Vector3 lastpos;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
#if !UFPS
        if (stateInfo.IsTag("Mounting"))
        {
            if (animator.transform.GetComponent<Rider>())
            {
                Rider rider = animator.transform.GetComponent<Rider>();
                rider.EnableMounting();
            }
        }

#else
        if (stateInfo.IsTag("Mounting"))
        {
            if (animator.transform.parent.GetComponent<Rider>())
            {
                Rider rider = animator.transform.parent.GetComponent<Rider>();
                rider.EnableMounting();
            }
        }
#endif
    }


    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        #if !UFPS
        if (stateInfo.IsTag("Unmounting"))
        {
            if (animator.transform.GetComponent<Rider>())
            {
                Rider rider = animator.transform.GetComponent<Rider>();
                rider.DisableMounting(lastpos);
            }
        }
        #else
        if (stateInfo.IsTag("Unmounting"))
        {
            if (animator.transform.parent.GetComponent<Rider>())
            {
                Rider rider = animator.transform.parent.GetComponent<Rider>();
                rider.DisableMounting(lastpos);
            }
        }
        #endif
    }

    // OnStateUpdate is called before OnStateUpdate is called on any state inside this state machine
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (stateInfo.IsTag("Unmounting") && stateInfo.normalizedTime <= 0.95f)
                lastpos = animator.pivotPosition;
        
    }
}