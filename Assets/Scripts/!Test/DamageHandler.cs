using UnityEngine;

public abstract class DamageHandler : MonoBehaviour
{
    protected abstract void OnTriggerEnter(Collider other);
    protected abstract void OnTriggerExit(Collider other);
    protected abstract void DealDamage();
}