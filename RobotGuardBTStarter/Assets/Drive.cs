using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Drive : MonoBehaviour {

    // Velocidade de movimentação do objeto
	public float speed = 20.0F;
    // Velocidade de rotação do objeto
    public float rotationSpeed = 120.0F;
    // Prefab do projétil disparado
    public GameObject bulletPrefab;
    // Origem do projétil disparado
    public Transform bulletSpawn;

    //Barra de vida
    public Slider healthBar;
    //Valor de vida
    public float health = 100;

    //Valores que irão pegar os inputs do jogador
    float horizontal,vertical;

    //Componente Rigidbody, que executa a física
    public Rigidbody rb;

    void Update() 
    {
        //Obtendo comando que gerará a rotação do objeto
        horizontal = Input.GetAxis("Horizontal") * speed;
        //Obtendo comando que gerará a movimentação do objeto
        vertical = Input.GetAxis("Vertical") * rotationSpeed;

        //Executa função ao pressionar a tecla de espaço
        if(Input.GetKeyDown("space")) 
        {
            //Instanciando projétil de disparo
            GameObject bullet = GameObject.Instantiate(bulletPrefab, bulletSpawn.transform.position, bulletSpawn.transform.rotation);
            //Adiciona força ao projétil, o movimentando para sua frente
            bullet.GetComponent<Rigidbody>().AddForce(bullet.transform.forward*2000);
        }

        //Administra constantemente a barra de vida
        ManageHealthBar();
    }

    void FixedUpdate() 
    {
        //Adiciona uma força de movimento para a frente desse objeto de acordo com o valor do comando "Vertical"
        rb.AddForce(transform.forward * vertical);
        //Adiciona uma força de rotação ao eixo Y do objeto de acordo com o valor do comando "Horizontal"
        rb.AddTorque(new Vector3(0,horizontal,0));
    }

    void ManageHealthBar()
    {
        //Gerando a posição na qual a barra de vida deve permanecer
        Vector3 healthBarPos = Camera.main.WorldToScreenPoint(this.transform.position);
        //Sincronizando a barra de vida com o valor
        healthBar.value = (int)health;
        //Atualizando a barra de vida do objeto, para que fique sincronizada
        healthBar.transform.position = healthBarPos + new Vector3(0,60,0);
    }

    [ContextMenu("Respawn")]
    void Respawn()
    {
        //Variáveis de posição x e z, serão utilizadas para respawn
        float x = 70, z = 70;

        //Faz um 'sorteio' para definir se a posição X será invertida
        if (Random.Range(0,2) > 0)
        {
            x = -x;
        }

        //Faz um 'sorteio' para definir se a posição Z será invertida
        if (Random.Range(0,2) > 0)
        {
            z = -z;
        }

        //Define a posição de respawn de acordo com os valores sorteados
        transform.position = new Vector3(x,0,z);

        //Reinicia o valor de vida
        health = 100;
    }

    //Método de colisão
    void OnCollisionEnter(Collision col)
    {
        //Subtrai 10 da vida do objeto caso tenha colidido com uma bala (Determinada pela tag "bullet")
        if(col.gameObject.tag == "bullet")
        {
            //Diminui a vida ao acontecer colisão com uma bala
            health -= 10;

            //Executa função caso a vida atual seja meno que 1
            if (health < 1)
            {
                //Método que executa o renascimento do objeto
                Respawn();
            }
        }
    }
}
