using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    //Corpo que esse objeto precisa seguir
    public Transform bodyToFollow;

    //Velocidade em que esse objeto vai seguir seu alvo
    public float speed;

    private void FixedUpdate() 
    {
        //Cria um vetor que é resultado de uma interpolação da posição desse objeto com a do objeto a ser seguido
        //Dessa maneira é executada uma movimentação suave até o destino
        Vector3 position = Vector3.Slerp(transform.position, bodyToFollow.position, speed);
        //Define a posição desse objeto de acordo com o vetor criado acima
        transform.position = position;    
    }
}
