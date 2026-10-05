using UnityEngine;

public class BulletMoveComponent : MonoBehaviour
{
    public float speed = 20f; //  Скорость пули
    public float lifeTime = 3f; // Время жизни пули
    public bool canBounce = false; // Умеет ли эта пуля отскакивать
    public GameObject explosionPrefab; // Префаб VFX взрыва пули

    private bool _isQuitting = false;

    private void OnApplicationQuit()
    {
        // Помечаем, что игра закрывается
        _isQuitting = true;
    }
}