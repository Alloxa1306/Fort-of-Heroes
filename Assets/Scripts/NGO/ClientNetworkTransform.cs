using Unity.Netcode.Components;
using UnityEngine;

[DisallowMultipleComponent]
public class ClientNetworkTransform : NetworkTransform
{
    // ”казываем NGO, что координаты задаютс€ клиентом-владельцем
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}