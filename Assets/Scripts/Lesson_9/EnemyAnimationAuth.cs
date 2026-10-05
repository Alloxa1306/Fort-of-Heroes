using UnityEngine;
using Unity.Entities;

public class EnemyAnimationAuth : MonoBehaviour, IConvertGameObjectToEntity
{
    public string speedParamName = "Speed";
    public string attackTriggerName = "Attack";
    public string dieTriggerName = "Die";

    [HideInInspector] public int SpeedHash;
    [HideInInspector] public int AttackHash;
    [HideInInspector] public int DieHash;

    private void Awake()
    {
        SpeedHash = Animator.StringToHash(speedParamName);
        AttackHash = Animator.StringToHash(attackTriggerName);
        DieHash = Animator.StringToHash(dieTriggerName);
    }

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentObject(entity, this);
    }

    public void OnAttackHit()
    {
        AttackBehaviour attack = GetComponentInParent<AttackBehaviour>();
        if (attack == null)
        {
            attack = GetComponentInChildren<AttackBehaviour>();
        }

        if (attack != null)
        {
            attack.ExecuteAttackLogic();
        }
    }
}
