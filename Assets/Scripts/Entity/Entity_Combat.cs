using System;
using UnityEngine;

public class Entity_Combat : MonoBehaviour
{
    private Entity_VFX vfx;
    private Entity_Stats stats;
    public float damage = 10;
    
    [Header("Target Detection")]
    [SerializeField] private Transform targetCheck;
    [SerializeField] private float targetCheckRadius = 1;
    [SerializeField] private LayerMask whatIsTarget;
    
    [Header("Status effect details")]
    [SerializeField] private float defaultDuration = 3;
    [SerializeField] private float chillSlowMultiplier = .2f;

    private void Awake()
    {
        vfx = GetComponent<Entity_VFX>();
        stats = GetComponent<Entity_Stats>();
    }

    public void PerformAttack()
    {
        foreach (var target in GetDetectedColliders())
        {
            IDamagable damagable = target.GetComponent<IDamagable>();
            
            if(damagable == null)
                continue;

            float elementalDamage = stats.GetElementalDamage(out ElementType element);
            float damage = stats.GetPhysicalDamage(out bool isCrit);
            
            bool targetGotHit = damagable.TakeDamage(damage, elementalDamage, element, transform);

            if (element != ElementType.None)
                ApplyStatusEffect(target.transform, element);
            
            if (targetGotHit)
            {
                vfx.UpdateOnHitColor(element);
                vfx.CreateOnHitVFX(target.transform, isCrit);
            }
        }
    }

    public void ApplyStatusEffect(Transform target, ElementType element)
    {
        Entity_StatusHandler statusHandler = target.GetComponent<Entity_StatusHandler>();
        
        if(statusHandler == null)
            return;

        if (element == ElementType.Ice && statusHandler.CanBeApplied(ElementType.Ice))
            statusHandler.ApplyChilledEffect(defaultDuration, chillSlowMultiplier);

    }

    protected Collider2D[] GetDetectedColliders()
    {
        return Physics2D.OverlapCircleAll(targetCheck.position, targetCheckRadius, whatIsTarget);
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(targetCheck.position, targetCheckRadius);
    }
}
