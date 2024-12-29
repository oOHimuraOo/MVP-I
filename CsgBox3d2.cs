using Godot;
using System;

public partial class CsgBox3d2 : CsgBox3D
{
    // variaveis de referencia. Essas variaveis sao exportadas para o inspetor do godot, onde podemos fazer a declaracao de valores diretamente pelo inspetor evitando erros de referencia.
    [Export] private CsgSphere3D bola;
    [Export] private CsgBox3D carta;
    [Export] private Camera3D camera;
    [Export] private Marker3D marker;
    [Export] private Timer temporizador;
    [Export] private Vector3 bola_posicao_inicial;

    // variaveis que determinam a velocidade de movimento no eixo x, y e z. Sendo que o eixo y eh apenas velocidade de rotacao da camera.
    private static readonly Vector3 velocidadeX = new Vector3(1, 0, 0) / 5; 
    private static readonly Vector3 velocidadeY = new Vector3(0, 1, 0) / 10;
    private static readonly Vector3 velocidadeZ = new Vector3(0, 0, 1) / 5;
    private static readonly Vector3 velocidadeR_Carta = new Vector3(0, 0, 1) / 10;
    private static readonly int limitador_de_distancia_movimento = 7;

    // variavel que armazena a velocidade de movimento da bola depois que a direcao do movimento foi devidamente calculada.
    private Vector3 velocidade = new Vector3(0, 0, 0);

    // Variaveis que controlam se o movimento da bola deve ser iniciado, se jah foi iniciado alguma vez e se a bola esta no alcance do jogador.
    private bool bola_dentro = false;
    private bool movendo_bola = false;
    private bool bola_foi_movida = false;

    // variaveis responsaveis por iniciar o movimento da carta e por identificar se a carta esta dentro do alcance do jogador.
    private bool carta_dentro = false;
    private bool movendo_carta = false;

    // A funcao Ready eh chamada apenas uma vez durante a inicializacao do script. Ou seja quando o nodo se torna pronto para execucao dentro da arvore.
    public override void _Ready()
	{
        bola_posicao_inicial = bola.Position;
    }

    // A funcao process eh chamada a cada frame. O valor delta mede o tempo passado entre cada frame, entao ele nao eh uma constante. eh um valor que pode variar de acordo com a quantidade de tempo passado entre cada frame.
    // ou seja se estiver rodando a 120 frames por segundo o valor de delta sera menor do que quando rodado a 30 frames por segundo.
	public override void _Process(double delta)
	{
        resetar_bola_fora_do_limite();
    }

    // A funcao physicsProcess funciona de uma forma semelhante a process, porem nesse caso o godot vai tentar transformar o valor de delta em uma constante. Ele faz isso reduzindo a quantidade de frames por segundo para sincronizar
    // com o processamento de fisica. Ou seja se o seu processamento de fisica for regulado para ser passado em 30 frames por segundo o phisics process tentara executar com essa mesma frequencia.
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        lidar_movimento_jogador();
        lidar_movimento_camera();
        lidar_acao();
        lidar_movimento_objeto();
    }

    //essa funcao so serve para resetar o posicionamento da bola caso ela passe de um limite especificado pela variavel limitador_de_distancia_movimento.
    private void resetar_bola_fora_do_limite()
    {
        if (bola_foi_movida && (bola.Position.X > limitador_de_distancia_movimento || bola.Position.X < -limitador_de_distancia_movimento || bola.Position.Z > limitador_de_distancia_movimento || bola.Position.Z < -limitador_de_distancia_movimento))
        {
            bola.Position = bola_posicao_inicial;
        }
    }

    // essa funcao eh a funcao responsavel por lidar com toda a movimentacao do jogador.
    private void lidar_movimento_jogador()
    {
        if (Input.IsActionPressed("MOVE_LEFT"))
        {
            Position -= velocidadeZ;
        }
        else if (Input.IsActionPressed("MOVE_RIGHT"))
        {
            Position += velocidadeZ;
        }
        else if (Input.IsActionPressed("MOVE_FRONT"))
        {
            Position += velocidadeX;
        }
        else if (Input.IsActionPressed("MOVE_BACK"))
        {
            Position -= velocidadeX;
        }
    }

    // essa funcao eh a responsavel por lidar com toda a movimentacao da camera.
    private void lidar_movimento_camera()
    {
        if (Input.IsActionPressed("ROTATE_LEFT"))
        {
            marker.Rotation -= velocidadeY;
        }
        else if (Input.IsActionPressed("ROTATE_RIGHT"))
        {
            marker.Rotation += velocidadeY;
        }
    }

    // essa funcao eh a responsavel por lidar com a interacao do jogador com os objetos.
    private void lidar_acao()
    {
        if (Input.IsActionJustPressed("ACAO"))
        {
            if (bola_dentro)
            {
                iniciar_movimento("Bola");
            }
            else if (carta_dentro) 
            {
                iniciar_movimento("Carta");
            }

        }
    }

    // essa funcao eh a responsavel por iniciar o movimento da bola ou da carta.
    private void iniciar_movimento(string type)
    {
        switch (type)
        {
            case "Bola":
                temporizador.Start();
                movendo_bola = true;
                bola_foi_movida = true;
                velocidade = calcular_direcao_da_bola(Position, bola.Position) / 10;
                break;
            case "Carta":
                temporizador.Start();
                movendo_carta = true;
                break;
            default:
                GD.Print("objeto com movimento invalido");
                break;
        }
    }

    // essa funcao eh a responsavel por movimentar a bola ou carta caso o movimento esteja liberado para ser iniciado.
    private void lidar_movimento_objeto()
    {
        if (movendo_bola)
        {
            bola.Position += velocidade;
        }
        
        if (movendo_carta)
        {
            carta.Rotation += velocidadeR_Carta;
        }
    }

    // essa funcao eh a responsavel por detectar o sinal emitido quando um objeto do tipo Area3d entra no alcance do jogador. quando o sinal eh detectado ele entao identifica o que foi que entrou no alcance e trata as devidas acoes.
    private void _on_area_3d_area_entered(Area3D area) 
    {
        atualizar_estado_objeto(area.GetParent().Name, true);
    }

    // essa funcao eh a responsavel por detectar o sinal emitido quando um objeto do tipo area3d sai do alcance do jogador. quando o sinal eh detectado ele entaoo identifica o que foi que saiu do alcance e trata as devidas acoes.
    private void _on_area_3d_area_exited(Area3D area)
    {
        atualizar_estado_objeto(area.GetParent().Name, false);
    }

    //  essa funcao serve para modificar o estado do objeto atual ou seja se ele esta dentro ou fora do alcance do jogador e qual objeto esta dentro do alcance do jogador.
    private void atualizar_estado_objeto(string nome_objeto, bool estado)
    {
        switch (nome_objeto) 
        {
            case "Bola":
                bola_dentro = estado;
                break;
            case "Carta":
                carta_dentro = estado;
                break;
            default:
                GD.Print("Nome de objeto invalido");
                break;
        }
    }

    // Essa funcao controla o tempo de movimento da carta e da bola. Ela faz isso ao detectar que o sinal emitido por um timer chegou ao final. Ao fazer isso ela reseta o estado de movimento dos objetos para o estado padrao.
    private void _on_timer_timeout()
    {
        movendo_bola = false;
        movendo_carta = false;
    }

    
    // essa funcao eh responsavel por calcular a diferenca entre a posicao atual do jogador e da bola e entao normalizar os valores e so entao retornar esses valores para serem utilizados como modificador de velocidade da bola.
    // com isso a bola sera capaz de se movimentar na direcao oposta a que o jogador esta.
    // a velocidade de movimento sera sempre um valor inteiro entre -1 e 1. ou seja o vector3 ficara algo como 1, 0, -1 ou 0, 0, 1.
    private Vector3 calcular_direcao_da_bola(Vector3 posicao_jogador, Vector3 posicao_bola)
    {
        var direcao = (posicao_bola - posicao_jogador);
        direcao = new Vector3(MathF.Sign(direcao.X), 0, MathF.Sign(direcao.Z));
        return direcao;
    }
}
