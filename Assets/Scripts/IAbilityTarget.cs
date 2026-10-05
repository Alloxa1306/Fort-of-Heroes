using System.Collections.Generic;
using UnityEngine;

public interface IAbilityTarget : IAbility
{
    // Список целей, к которым будет применяться реакции
    List<GameObject> Targets { get; set; }
}