using Unity.Entities;
using Unity.Mathematics;

// Типы коллайдера
public enum ColliderType
{
    Sphere = 0,
    Capsule = 1,
    Box = 2
}

public struct ActorColliderData : IComponentData
{
    public ColliderType ColliderType;
    public bool initialTakeOff;

    // Для сферы
    public float3 SphereCenter;
    public float SphereRadius;

    // Для капсулы
    public float3 CapsuleStart;
    public float3 CapsuleEnd;
    public float CapsuleRadius;

    // Для куба
    public float3 BoxCenter;
    public float3 BoxHalfExtents;
    public quaternion BoxOrientation;
}