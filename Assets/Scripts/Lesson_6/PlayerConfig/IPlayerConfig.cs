using System;
using UnityEngine;

public interface IPlayerConfig
{

    // �������� ��� ������
    int HeroHealth { get; }
    float MoveSpeed { get; }
    float FireRate { get; }
    float DashDistance { get; }
    float TurnSpeed { get; }
}

[SerializeField]
public class PlayerConfig : IPlayerConfig
{
    public int heroHealth = 200;
    public float moveSpeed = 5f;
    public float fireRate = 0.05f;
    public float dashDistance = 1f;
    public float turnSpeed = 15f;

    public int HeroHealth => heroHealth;
    public float MoveSpeed => moveSpeed;
    public float FireRate => fireRate;
    public float DashDistance => dashDistance;
    public float TurnSpeed => turnSpeed;

}