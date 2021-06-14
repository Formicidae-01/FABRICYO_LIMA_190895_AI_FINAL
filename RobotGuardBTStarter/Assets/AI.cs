using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using Panda;

public class AI : MonoBehaviour
{
    //Corpo do objeto do jogador
    public Transform player;
    //Origem do projétil disparado
    public Transform bulletSpawn;
    //Barra de vida do objeto
    public Slider healthBar; 
    //Projétil que é disparado por objeto  
    public GameObject bulletPrefab;

    //Componente navMesh, que executa a movimentação do objeto até um caminho específico
    NavMeshAgent agent;
    //Destino da movimentação
    public Vector3 destination; // The movement destination.
    //Alvo da rotação (mira)
    public Vector3 target;      // The position to aim to.
    //Valor de vida do objeto
    float health = 100.0f;
    //Velocidade de rotação do objeto
    public float rotSpeed = 5.0f;

    //Distância máxima de visibilidade do objeto, de quão longe ele pode de detectar o jogador
    public float visibleRange = 80.0f;
    //Distância que o jogador precisa estar desse objeto para atirar
    public float shotRange = 40.0f;
    
    //Distância que define se o jogador está próximo ou não desse objeto
    public float closeDistance;

    void Start()
    {
        //Obtendo o navmesh no início da execução
        agent = this.GetComponent<NavMeshAgent>();
        //Determinando a distância que o personagem precisa estar do jogador para deixar de se mover
        agent.stoppingDistance = shotRange - 5; //for a little buffer

        //Invocando constantemente método que verifica a vida do objeto e a impede de ficar menor que 100
        //InvokeRepeating("UpdateHealth",5,0.5f);
    }

    void Update()
    {
        //Administra constantemente a barra de vida
        ManageHealthBar();
    }

    //Método que administra a barra de vida
    void ManageHealthBar()
    {
        //Gerando a posição na qual a barra de vida deve permanecer
        Vector3 healthBarPos = Camera.main.WorldToScreenPoint(this.transform.position);
        //Sincronizando a barra de vida com o valor
        healthBar.value = (int)health;
        //Atualizando a barra de vida do objeto, para que fique sincronizada
        healthBar.transform.position = healthBarPos + new Vector3(0,60,0);
    }

    //Método de colisão
    void OnCollisionEnter(Collision col)
    {
        //Executa função caso tenha colidido com uma bala (Determinada pela tag "bullet")
        if(col.gameObject.tag == "bullet")
        {
            //Subtrai 10 da vida do objeto
            health -= 10;

            //Executa função que impede que a vida do objeto seja menor que 0
            if (health < 0)
            {
                health = 0;
            }
        }
    }

    //Método utilizado com o componente PandaBehaviour
    [Task]
    public void PickRandomDestination()
    {
        //Gera uma posição aleatória no mapa
        Vector3 dest = new Vector3(Random.Range(-100, 100), 0, Random.Range(-100,100));
        //Define o destino do agente NavMesh como a posição gerada acima
        agent.SetDestination(dest);
        //Método que mantém o agente NavMesh ativo
        ResumeAgent();
        //Finaliza a tarefa atual
        Task.current.Succeed();
    }

    //Método utilizado com o componente PandaBehaviour
    [Task]
    public void RandomDestinationOnly()
    {
        //Gera uma posição aleatória no mapa
        Vector3 dest = new Vector3(Random.Range(-100, 100), 0, Random.Range(-100,100));
        //Define o alvo como a posição gerada acima
        target = dest;
        //Método que mantém o agente NavMesh parado
        StopAgent();
        //Finaliza a tarefa atual
        Task.current.Succeed();
    }

    //Método utilizado com o componente PandaBehaviour
    [Task]
    public void PickDestination(int x, int z)
    {
        //Cria um vetor que serve como destino de movimentação do agente navMesh
        Vector3 dest = new Vector3(x,0,z);
        //Aplica o vetor acima ao agente navMesh
        agent.SetDestination(dest);
        //Conclui a tarefa
        Task.current.Succeed();
    }
    
    //Método utilizado com o componente PandaBehaviour
    [Task]
    public void MoveToDestination()
    {
        //Exibe a quanto tempo a tarefa está sendo executada (se estiver sendo executada)
        if (Task.isInspected)
        {
            Task.current.debugInfo = string.Format("t={0:0.00}", Time.time);
        }

        //Conclui a tarefa caso o personagem esteja próximo do local final e não esteja escolhendo um próximo caminho
        if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
        {
            Task.current.Succeed();
        }
    }

    //Método utilizado com o componente PandaBehaviour
    [Task]
    public void TargetPlayer()
    {
        //Determina o vetor "target" como a posição do jogador, significa que o esse objeto estará apontando ao jogador
        target = player.transform.position;
        //Conclui a tarefa
        Task.current.Succeed();
    }

    //Método utilizado com o componente PandaBehaviour
    [Task]
    public void TargetAndFollowPlayer()
    {
        //Define o alvo como a posição do jogador
        target = player.transform.position;
        //Define o alvo do agente NavMesh como a posição gerada acima
        agent.SetDestination(target);
    }

    //Método utilizado com o componente PandaBehaviour
    [Task]
    public bool CloseToPlayer()
    {
        //Verifica se a distância entre o jogador e esse objeto é menor que a distância esperada e retorna esse valor
        return Vector3.Distance(transform.position, player.position) < closeDistance;
    }

    //Método utilizado com o componente PandaBehaviour
    [Task]
    public bool NotCloseToPlayer()
    {
        //Retorna o valor oposto do método acima, servindo como um "false" para ele
        return !CloseToPlayer();
    }

    //Método utilizado com o componente PandaBehaviour
    [Task]
    public void FleeFromPlayer()
    {
        //Cria um vetor que é a soma entre a posição do jogador e a posição desse objeto, gerando um vetor que se afasta do jogador
        Vector3 direction = (player.position + transform.position);
        //Define o destino do agente como o vetor criado acima, o multiplicando por 2 para aumentar sua distância
        agent.SetDestination(direction  * 2);
    }

    //Método utilizado com o componente PandaBehaviour
    [Task]
    public void SetStoppingDistance(float value)
    {
        //Define a distância na qual o agente NavMesh deve parar
        agent.stoppingDistance = value;
    }

    //Método utilizado com o componente PandaBehaviour
    [Task]
    public bool Fire()
    {
        //Instancia o projétil
        GameObject bullet = GameObject.Instantiate(bulletPrefab, bulletSpawn.transform.position, bulletSpawn.transform.rotation);
        //Adiciona força ao projétil, fazendo com que ele avance
        bullet.GetComponent<Rigidbody>().AddForce(bullet.transform.forward * 2000);
        //Retorna um valor verdadeiro, indicando que o método foi realizado
        return true;
    }

    //Método utilizado com o componente PandaBehaviour
    [Task]
    void StopAgent()
    {
        //Define que o agente NavMesh está parado
        agent.isStopped = true;
        //Conclui a tarefa
        Task.current.Succeed();
    }

    //Método utilizado com o componente PandaBehaviour
    [Task]
    void ResumeAgent()
    {
        //Define que o agente NavMesh não está parado
        agent.isStopped = false;
        //Conclui a tarefa
        Task.current.Succeed();
    }

    //Método utilizado com o componente PandaBehaviour
    [Task]
    public void LookAtTarget()
    {
        //Cria um vetor cujo valor é a subtração da posição alvo e a posição desse objeto
        Vector3 direction = target - transform.position;
        //Rotaciona esse objeto até o alvo através de uma interpolação
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * rotSpeed);
        //Executa função enquanto a tarefa está sendo executada
        if (Task.isInspected)
        {
            //Exibe a quanto tempo a tarefa está sendo executada
            Task.current.debugInfo = string.Format("angle={0}", Vector3.Angle(transform.forward, direction));

            //Executa método caso a rotação esteja cumprida ("Vector3.Angle" é bem similar a "Vector3.Distance")
            if (Vector3.Angle(transform.forward, direction) < 5.0f)
            {
                //Conclui a tarefa
                Task.current.Succeed();
            }
        }
    }

    //Método utilizado com o componente PandaBehaviour
    [Task]
    bool SeePlayer()
    {
        //Vetor que será utilizado como alvo de um raycast
        Vector3 distance = player.transform.position - transform.position;
        //Variável que guarda as informações de um raycast
        RaycastHit hit;
        //boolean que determina se o objeto está observando ou não uma parede
        bool seeWall = false;
        //Desenha uma linha que vai da posição do objeto até o destino do raycast, criando um traço visual (somente visível com gizmos ativados)
        Debug.DrawRay(transform.position, distance, Color.red);
        //Dispara um raycast da posição do jogador até a posição do vetor "distance", executa método se o raio atingir algum colisor
        if (Physics.Raycast(transform.position, distance, out hit))
        {
            //Se o objeto atingido for uma parede (identificado pela tag "Wall", o boolean "seeWall" se torna verdadeiro)
            if (hit.collider.gameObject.tag == "wall")
            {
                seeWall = true;
            }
        }

        //Exibe a quanto tempo a tarefa está sendo executada
        if (Task.isInspected)
        {
            Task.current.debugInfo = string.Format("wall={0}", seeWall);
        }
        
        //Se o tamanho do vetor for menor que a distância máxima de visão e esse objeto não estiver olhando para uma parede, o valor retornado é verdadeiro, significa que o objeto viu o personagem
        if (distance.magnitude < visibleRange && !seeWall)
        {
            return true;
        }
        //Se a condição acima não for cumprida, o valor retornado é falso, significa que esse objeto não vê o jogador
        else;
        {
            return false;
        }
    }

    //Método utilizado com o componente PandaBehaviour
    [Task]
    bool NotSeePlayer()
    {
        //Retorna o valor oposto do método acima (SeePlayer), servindo como um "false"
        return !SeePlayer();
    }

    //Método utilizado com o componente PandaBehaviour
    [Task]
    bool Turn(float angle)
    {
        //Cria uma variável que será o alvo para qual o objeto irá rotacionar
        var p = this.transform.position + Quaternion.AngleAxis(angle, Vector3.up) * transform.forward;
        //Determina o alvo da rotação igual ao valor acima
        target = p;
        //Retorna um valor verdadeiro
        return true;
    }

    //Método utilizado com o componente PandaBehaviour
    [Task]
    bool IsHealthLessThan(float value)
    {
        //Verifica se a vida atual do objeto é menor que o valor declarado e retorna essa condição
        return health < value;
    }

    //Método utilizado com o componente PandaBehaviour
    [Task]
    bool Explode()
    {
        //Destrói a barra de vida desse objeto
        Destroy(healthBar.gameObject);
        //Destrói esse objeto
        Destroy(gameObject);
        //Retorna um valor verdadeiro, indicando que a tarefa foi concluída
        return true;
    }
}

