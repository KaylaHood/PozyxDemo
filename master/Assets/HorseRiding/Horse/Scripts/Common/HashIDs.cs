using UnityEngine;
using System.Collections;

public class HashIDs : MonoBehaviour
{
    public static int jumpHash = Animator.StringToHash("Jumping");
    public static int sspeedHash = Animator.StringToHash("Speed");
    public static int horizontalHash = Animator.StringToHash("Horizontal");
    public static int shiftHash = Animator.StringToHash("Shift");
    public static int galopingHash = Animator.StringToHash("Galloping");
    public static int trottingHash = Animator.StringToHash("Trotting");
    public static int deathHash = Animator.StringToHash("Death");
    public static int standHash = Animator.StringToHash("Stand");
    public static int fowardHash = Animator.StringToHash("FowardPressed");
    public static int mountHash = Animator.StringToHash("Mount");
    public static int mountSide = Animator.StringToHash("MountSide");
    public static int SwimHash = Animator.StringToHash("Swimming");
    public static int inclinationHash = Animator.StringToHash("Inclination");
    public static int fallingHash = Animator.StringToHash("Falling");
    public static int fallingBackHash = Animator.StringToHash("FallingBack");
    public static int sleepHash = Animator.StringToHash("Sleep");
    public static int attackHash = Animator.StringToHash("HorseAttack");
    public static int IntHash = Animator.StringToHash("HorseInt");
    public static int FloatHash = Animator.StringToHash("HorseFloat");
}
