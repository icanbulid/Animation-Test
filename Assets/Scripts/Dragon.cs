using UnityEngine;

public class Dragon : MonoBehaviour
{
    public int HP = 100;
    public Animator animator;
    public void TakeDamage(int damageAmount)
    {
        HP -= damageAmount;
        if (HP <= 0)
        {
            AudioManager.instance.Play("Dragon_Death");
            animator.SetTrigger("Die");
            GetComponent<Collider>().enabled = false;
        }
        else {
            AudioManager.instance.Play("Dragon_Damage");
            animator.SetTrigger("Damage");
            animator.SetBool("isChasing",true);
        }
    }
}
