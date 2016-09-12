using UnityEngine;
using System.Collections;

public class BlendIdle : StateMachineBehaviour
{

  
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetFloat("HorseFloat", Random.Range(0f, 4f));

        if (animator.GetComponent<HorseController>())
        {
            animator.GetComponent<HorseController>().SleepCount = animator.GetComponent<HorseController>().SleepCount + 1;
            if (animator.GetComponent<HorseController>().SleepCount > animator.GetComponent<HorseController>().GoToSleep)
            {
                //Sleep Count
                animator.GetComponent<HorseController>().Sleep = true;
                animator.GetComponent<HorseController>().SleepCount = 0;
            }
        }
    }

    
}
