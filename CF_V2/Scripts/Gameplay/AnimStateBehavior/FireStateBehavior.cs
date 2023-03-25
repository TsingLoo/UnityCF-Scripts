using System.Collections;
using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    public class FireStateBehavior : StateMachineBehaviour
    {
        WeaponController weapon;

        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            weapon = animator.GetComponent<WeaponController>();
            if (weapon != null)
            {
                weapon.CurrentState = EWeaponState.Fire;
            }
        }

        // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (weapon != null)
            {
                if(weapon.FireTimerOut)
                {
                    weapon.CurrentState = EWeaponState.Idle;
                }
            }
        }

        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (weapon != null)
            {
                weapon.CurrentState = EWeaponState.Idle;
            }
        }

        // OnStateMove is called right after Animator.OnAnimatorMove()
        //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //    // Implement code that processes and affects root motion
        //}

        // OnStateIK is called right after Animator.OnAnimatorIK()
        //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //    // Implement code that sets up animation IK (inverse kinematics)
        //}
    }
}