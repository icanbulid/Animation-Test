using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public int damageAmount = 20;
    private void Start()
    {
        Destroy(gameObject, 10);
    }
    private void OnTriggerEnter(Collider other)
    {
        Destroy(transform.GetComponent<Rigidbody>());
        if(other.tag == "Enemy")
        {
            transform.parent = other.transform;
            other.GetComponent<Dragon>().TakeDamage(damageAmount);
        }
    }
}
