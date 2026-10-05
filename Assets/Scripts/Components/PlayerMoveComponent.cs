using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMoveComponent : MonoBehaviour
{
   public float moveSpeed = 1f; // Скорость персонажа
   public Vector3 direction;

    [Header("Система телепорта")]

    public float dashDistanсe = 3f; // Дистанция прыжка
    public bool wantsToDash; // Флаг для прыжка

    [Header("Система VFX")]

    public GameObject dashStartVfx; // Эффект прыжка, который остается на стартовом месте
    public GameObject dashEndVfx; // Эффект прыжка, который появляется на новом месте 

    [HideInInspector] public CharacterController characterController;
    private PlayerComponent playerData;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        playerData = GetComponent<PlayerComponent>();
    }

    private void Start()
    {
        if(playerData != null)
        {
            moveSpeed = playerData.moveSpeed;
            dashDistanсe = playerData.dashDistance;
        }
    }
}
