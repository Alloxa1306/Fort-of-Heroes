using DefaultNamespace.Components.Interfaces;
using UnityEngine;

public class RotateBehaviour : MonoBehaviour, IBehaviour
{
    public PlayerTagComponent player;

    [Header("Настройки ловушки")]
    public float rotationSpeed = 300f;
    public float maxDetectionDistance = 5f;

    private void Start()
    {
        player = FindObjectOfType<PlayerTagComponent>();
    }

    // Тот самый метод оценки из интерфейса IBehaviour.
    public float Evaluate()
    {
        if (player == null) return 0f;

        // Вычисляем расстояние до игрока
        float distance = (this.transform.position - player.transform.position).magnitude;

        if(distance > maxDetectionDistance)
        {
            return 0f;
        }

        return 1f / (distance + 0.001f);
    }

    // То действие что нужно выполнять
    public void Behave()
    {
        // Заставляем крутится нашу сущность
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }
}