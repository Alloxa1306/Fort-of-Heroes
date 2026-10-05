using UnityEngine;

public class PlayerWeaponComponent : MonoBehaviour
{
    [Header("Настройки оружия")]
    public GameObject bulletPrefab; // Префаб Пули
    public Transform firePoint;     // Точка спавна пули
    public float fireRate = 0.2f; // Кулдаун выстрела

    //[Header("Визуал")]

    //public GameObject weaponModel; // МОделька оружия

    [HideInInspector]
    public bool wantsToShoot;       // Флаг выстрела
    public float nextFireTime; // Таймер выстрела

    private PlayerComponent playerData;

    private void Awake()
    {
        playerData = GetComponent<PlayerComponent>();
    }

    private void Start()
    {
        if (playerData != null)
        {
            fireRate = playerData.fireRate;
        }
    }
}