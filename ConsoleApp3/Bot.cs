using INF1771_GameAI.Map;
using INF1771_GameClient.dto;
using INF1771_GameClient.Socket;
using System.Drawing;
using System.Timers;

namespace INF1771
{
    public class Bot
    {
        private string name = "INF1771 Bot Example1";
        private string host = "atari.icad.puc-rio.br";

        HandleClient client = new HandleClient();
        Dictionary<long, PlayerInfo> playerList = new Dictionary<long, PlayerInfo>();
        List<ShotInfo> shotList = new List<ShotInfo>();
        List<ScoreBoard> scoreList = new List<ScoreBoard>();
        Estado estadoAtual;
        Tile[,] mapa = new Tile[59, 34];


        GameAI gameAi = new GameAI();

        private System.Timers.Timer timer1 = new System.Timers.Timer();
        long time = 0;

        String gameStatus = "";
        String sscoreList = "";

        List<String> msg = new List<String>();
        double msgSeconds = 0;
        public Bot()
        {
            InicializaMapa();
            timer1.Enabled = true;
            timer1.Elapsed += new ElapsedEventHandler(IfStarted);
            timer1.Interval = 1000;
            HandleClient.CommandEvent += ReceiveCommand;
            HandleClient.ChangeStatusEvent += SocketStatusChange;
            estadoAtual = Estado.Desconhecido;
            client.connect(host);
            timer1.Start();
        }
        public void InicializaMapa()
        {
            for (var i = 0; i < 59; i++)
            {
                for (var y = 0; y < 34; y++)
                {
                    mapa[i, y] = new Tile();
                }
            }
        }
        #region Logicas_Pre_Implementadas
        private Color convertFromString(String c)
        {
            var p = c.Split(new char[] { ',', ']' });

            int A = Convert.ToInt32(p[0].Substring(p[0].IndexOf('=') + 1));
            int R = Convert.ToInt32(p[1].Substring(p[1].IndexOf('=') + 1));
            int G = Convert.ToInt32(p[2].Substring(p[2].IndexOf('=') + 1));
            int B = Convert.ToInt32(p[3].Substring(p[3].IndexOf('=') + 1));

            return Color.FromArgb(A, R, G, B);
        }
        private void sendMsg(string msg)
        {
            if (msg.Trim().Length > 0)
                client.sendSay(msg);
        }

        public void ReceiveCommand(object sender, EventArgs args)
        {
            CommandEventArgs cmdArgs = (CommandEventArgs)args;
            if (cmdArgs.cmd != null)
                if (cmdArgs.cmd.Length > 0)
                    try
                    {
                        switch (cmdArgs.cmd[0])
                        {

                            case "o":
                                if (cmdArgs.cmd.Length > 1)
                                {
                                    if (cmdArgs.cmd[1].Trim() == "")
                                        gameAi.GetObservationsClean();

                                    else
                                    {
                                        List<String> o = new List<String>();

                                        if (cmdArgs.cmd[1].IndexOf(",") > -1)
                                        {
                                            String[] os = cmdArgs.cmd[1].Split(',');
                                            for (int i = 0; i < os.Length; i++)
                                                o.Add(os[i]);
                                        }
                                        else
                                            o.Add(cmdArgs.cmd[1]);

                                        this.estadoAtual = gameAi.GetObservations(o);
                                        Console.WriteLine($"{estadoAtual.Nome} ");
                                    }
                                }
                                else
                                    gameAi.GetObservationsClean();

                                break;
                            case "s":
                                if (cmdArgs.cmd.Length > 1)
                                {
                                    gameAi.SetStatus(int.Parse(cmdArgs.cmd[1]),
                                                        int.Parse(cmdArgs.cmd[2]),
                                                        cmdArgs.cmd[3],
                                                        cmdArgs.cmd[4],
                                                        long.Parse(cmdArgs.cmd[5]),
                                                        int.Parse(cmdArgs.cmd[6]));
                                }
                                break;

                            case "player":
                                lock (playerList)
                                {
                                    if (cmdArgs.cmd.Length == 8)
                                        if (!playerList.ContainsKey(long.Parse(cmdArgs.cmd[1])))
                                            playerList.Add(long.Parse(cmdArgs.cmd[1]), new PlayerInfo(
                                                long.Parse(cmdArgs.cmd[1]),
                                                cmdArgs.cmd[2],
                                                int.Parse(cmdArgs.cmd[3]),
                                                int.Parse(cmdArgs.cmd[4]),
                                                (PlayerInfo.Direction)int.Parse(cmdArgs.cmd[5]),
                                                (PlayerInfo.State)int.Parse(cmdArgs.cmd[6]),
                                               convertFromString(cmdArgs.cmd[7])));
                                        else
                                        {
                                            playerList[long.Parse(cmdArgs.cmd[1])] = new PlayerInfo(
                                                long.Parse(cmdArgs.cmd[1]),
                                                cmdArgs.cmd[2],
                                                int.Parse(cmdArgs.cmd[3]),
                                                int.Parse(cmdArgs.cmd[4]),
                                                (PlayerInfo.Direction)int.Parse(cmdArgs.cmd[5]),
                                                (PlayerInfo.State)int.Parse(cmdArgs.cmd[6]),
                                               convertFromString(cmdArgs.cmd[7]));

                                        }
                                }

                                break;

                            case "g":
                                if (cmdArgs.cmd.Length == 3)
                                {
                                    if (gameStatus != cmdArgs.cmd[1])
                                        playerList.Clear();

                                    if (gameStatus != cmdArgs.cmd[1])
                                        Console.WriteLine("New Game Status: " + cmdArgs.cmd[1]);

                                    gameStatus = cmdArgs.cmd[1];
                                    time = long.Parse(cmdArgs.cmd[2]);
                                }
                                break;
                            case "u":
                                if (cmdArgs.cmd.Length > 1)
                                {
                                    for (int i = 1; i < cmdArgs.cmd.Length; i++)
                                    {
                                        String[] a = cmdArgs.cmd[i].Split('#');

                                        if (a.Length == 4)
                                            scoreList.Add(new ScoreBoard(
                                                a[0],
                                                (a[1] == "connected"),
                                                int.Parse(a[2]),
                                                int.Parse(a[3]), System.Drawing.Color.Black));
                                        else if (a.Length == 5)
                                            scoreList.Add(new ScoreBoard(
                                                a[0],
                                                (a[1] == "connected"),
                                                int.Parse(a[2]),
                                                int.Parse(a[3]), convertFromString(a[4])));
                                    }
                                    sscoreList = "";
                                    foreach (ScoreBoard sb in scoreList)
                                    {
                                        sscoreList += sb.name + "\n";
                                        sscoreList += (sb.connected ? "connected" : "offline") + "\n";
                                        sscoreList += sb.energy + "\n";
                                        sscoreList += sb.score + "\n";
                                        sscoreList += "---\n";
                                    }
                                    scoreList.Clear();
                                }
                                break;
                            case "notification":
                                if (cmdArgs.cmd.Length > 1)
                                {
                                    if (msg.Count == 0)
                                        msgSeconds = 0;
                                    msg.Add(cmdArgs.cmd[1]);
                                    Console.WriteLine($"{cmdArgs.cmd[1]}");
                                }

                                break;
                            case "hello":
                                if (cmdArgs.cmd.Length > 1)
                                {
                                    if (msg.Count == 0)
                                        msgSeconds = 0;

                                    msg.Add(cmdArgs.cmd[1] + " has entered the game!");
                                    Console.WriteLine($"{cmdArgs.cmd[1]} has entered the game!");
                                }

                                break;

                            case "goodbye":
                                if (cmdArgs.cmd.Length > 1)
                                {
                                    if (msg.Count == 0)
                                        msgSeconds = 0;

                                    msg.Add(cmdArgs.cmd[1] + " has left the game!");
                                    Console.WriteLine($"{cmdArgs.cmd[1]} has left the game!");
                                }

                                break;


                            case "changename":
                                if (cmdArgs.cmd.Length > 1)
                                {
                                    if (msg.Count == 0)
                                        msgSeconds = 0;
                                    msg.Add(cmdArgs.cmd[1] + " is now known as " + cmdArgs.cmd[2] + ".");
                                    Console.WriteLine($"{cmdArgs.cmd[1]} is now known as {cmdArgs.cmd[2]}.");
                                }

                                break;
                            case "h":
                                if (cmdArgs.cmd.Length > 1)
                                {
                                    List<String> o = new List<String>();
                                    o.Add("hit");
                                    gameAi.GetObservations(o);
                                    msg.Add("you hit " + cmdArgs.cmd[1]);
                                    Console.WriteLine("You Hit");
                                }
                                break;
                            case "d":
                                if (cmdArgs.cmd.Length > 1)
                                {
                                    List<String> o = new List<String>();
                                    o.Add("damage");
                                    gameAi.GetObservations(o);
                                    msg.Add(cmdArgs.cmd[1] + " hit you");
                                    Console.WriteLine($"{cmdArgs.cmd[1]} hit you");
                                }
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
        }
        private void SocketStatusChange(object sender, EventArgs e)
        {
            if (client.connected)
            {
                Console.WriteLine("Connected");
                client.sendName(name);
                client.sendRequestGameStatus();
                client.sendRequestUserStatus();
                client.sendRequestObservation();

            }
            else
                Console.WriteLine("Disconnected");
        }
        #endregion Logicas_Pre_Implementadas

        private void IfStarted(object sender, EventArgs e)
        {
            msgSeconds += timer1.Interval;
            client.sendRequestGameStatus();
            if (gameStatus == "Game")
            {
                Console.WriteLine("Started");
                client.sendRequestPosition();
                ProcuraArtigos();
            }
            else if (msgSeconds >= 5000)
            {
                Console.WriteLine(gameStatus);
                //Console.WriteLine(GetTime());
                Console.WriteLine("-----------------");
                Console.WriteLine(sscoreList);

                client.sendRequestScoreboard();
            }

            if (msgSeconds >= 5000)
            {
                if (msg.Count > 0)
                {
                    foreach (String s in msg)
                        Console.WriteLine(s);
                    msg.Clear();
                }
                msgSeconds = 0;
            }
        }

        private void ProcuraArtigos()
        {
            var posAtual = gameAi.GetPlayerPosition();
            if (mapa[posAtual.x, posAtual.y].espaco.Equals(Espaco.Desconhecido))
            {
                mapa[posAtual.x, posAtual.y].espaco = Espaco.Nada;
            }
            var posicao = gameAi.NextPosition();
            if (gameAi.EsseTileExiste(posicao.x, posicao.y))
            {
                if ((mapa[posicao.x, posicao.y].GetTipoEspaco().Equals(Espaco.Desconhecido)))
                {
                    AndaParaFrenteEObservar();
                    if (estadoAtual.Equals(Estado.Bloqueado))
                    {
                        mapa[posicao.x, posicao.y].espaco = Espaco.Parede;
                        var posicoes = gameAi.GetAllAdjacentPositions();
                        foreach (var posicaoProx in posicoes)
                        {
                            if (gameAi.EsseTileExiste(posicaoProx.x, posicaoProx.y))
                            {
                                if (!mapa[posicaoProx.x, posicaoProx.y].espaco.Equals(Espaco.Poco) || !mapa[posicaoProx.x, posicaoProx.y].espaco.Equals(Espaco.Parede))
                                {
                                    MudaDirecao(posicao.x - posicaoProx.x, posicao.y - posicaoProx.y);
                                    AndaParaFrente();
                                    break;
                                }
                            }
                        }
                    }
                    else if (estadoAtual.Equals(Estado.BlueLight))
                    {
                        mapa[posicao.x, posicao.y].espaco = Espaco.PowerUp;
                        Console.WriteLine("Tentou pegar item");
                        client.sendGetItem();
                        MudaEstado(Estado.Desconhecido);
                    }
                    else if (estadoAtual.Equals(Estado.RedLight))
                    {
                        mapa[posicao.x, posicao.y].espaco = Espaco.PowerUp;
                        Console.WriteLine("Tentou pegar item");
                        client.sendGetItem();
                        MudaEstado(Estado.Desconhecido);
                    }
                    else if (estadoAtual.Equals(Estado.Passos))
                    {
                        LutaComInimigos();
                    }
                    else if (estadoAtual.Equals(Estado.Dano))
                    {
                        LutaComInimigos();
                    }
                    else if (estadoAtual.Equals(Estado.Brisa))
                    {

                    }
                }
                else if (mapa[posicao.x, posicao.y].GetTipoEspaco().Equals(Espaco.Poco))
                {
                    DesviaPoco();
                }
                else
                {
                    if (mapa[posicao.x, posicao.y].GetTipoEspaco().Equals(Espaco.PowerUp))
                    {
                        Console.WriteLine("Tentou pegar item");
                        client.sendGetItem();
                    }
                    else if (mapa[posicao.x, posicao.y].GetTipoEspaco().Equals(Espaco.Nada))
                    {
                        AndaParaFrente();
                    }
                    else if (mapa[posicao.x, posicao.y].GetTipoEspaco().Equals(Espaco.Parede))
                    {
                        var posicoes = gameAi.GetAllAdjacentPositions();
                        foreach (var posicaoProx in posicoes)
                        {
                            if (gameAi.EsseTileExiste(posicaoProx.x, posicaoProx.y))
                            {
                                if (!mapa[posicaoProx.x, posicaoProx.y].espaco.Equals(Espaco.Poco) || !mapa[posicaoProx.x, posicaoProx.y].espaco.Equals(Espaco.Parede))
                                {
                                    MudaDirecao(posicao.x - posicaoProx.x, posicao.y - posicaoProx.y);
                                    AndaParaFrente();
                                    if (mapa[posicaoProx.x, posicaoProx.y].espaco.Equals(Espaco.Desconhecido))
                                    {
                                        client.sendRequestUserStatus();
                                        client.sendRequestObservation();
                                    }
                                    MudaEstado(Estado.Desconhecido);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                var posicoes = gameAi.GetAllAdjacentPositions();
                foreach (var posicaoProx in posicoes)
                {
                    if (gameAi.EsseTileExiste(posicaoProx.x, posicaoProx.y))
                    {
                        if (!mapa[posicaoProx.x, posicaoProx.y].espaco.Equals(Espaco.Poco) || !mapa[posicaoProx.x, posicaoProx.y].espaco.Equals(Espaco.Parede))
                        {
                            MudaDirecao(posicao.x - posicaoProx.x, posicao.y - posicaoProx.y);
                            AndaParaFrente();
                            if (mapa[posicaoProx.x, posicaoProx.y].espaco.Equals(Espaco.Desconhecido))
                            {
                                client.sendRequestUserStatus();
                                client.sendRequestObservation();
                            }
                            MudaEstado(Estado.Desconhecido);
                            break;
                        }
                    }
                }
            }
        }
        private void LutaComInimigos()
        {
            //se eu receber q acertei continuo a atirar ate receber q n acertei mais
            client.sendShoot();
            client.sendRequestObservation();
            if (estadoAtual.Equals(Estado.Hit))
            {
                ContinuaAtirar();
            }
            else
            {
                var posicoesPossiveis = gameAi.GetAllAdjacentPositions();
                var pos = gameAi.GetPlayerPosition();
                foreach (var posicao in posicoesPossiveis)
                {
                    if (gameAi.EsseTileExiste(posicao.x, posicao.y))
                    {
                        if (mapa[posicao.x, posicao.y].GetTipoEspaco() != Espaco.Parede && mapa[posicao.x, posicao.y].GetTipoEspaco() != Espaco.Poco)
                        {
                            if (posicao.x != pos.x)
                            {
                                if (posicao.x > pos.x)
                                {
                                    MudaDirecao(1, 0);
                                    break;
                                }
                                else
                                {
                                    MudaDirecao(-1, 0);
                                    break;
                                }
                            }
                            if (posicao.y != pos.y)
                            {
                                if (posicao.y > pos.y)
                                {
                                    MudaDirecao(0, 1);
                                    break;
                                }
                                else
                                {
                                    MudaDirecao(0, -1);
                                    break;
                                }
                            }
                        }
                    }
                }
                client.sendShoot();
                client.sendRequestObservation();
                if (estadoAtual.Equals(Estado.Hit))
                {
                    ContinuaAtirar();
                }
                else
                {
                    ProcuraArtigos();
                }
            }
        }
        public void ContinuaAtirar()
        {
            for (int i = 0; i < 4; i++)
            {
                client.sendShoot();
            }
            client.sendRequestObservation();
            if (estadoAtual.Equals(Estado.Hit))
            {
                ContinuaAtirar();
            }
            ProcuraArtigos();
        }
        public void MudaDirecao(int x, int y)
        {
            //se tou andando pro norte ou sul e quero mudar minha direcao em x+ viro p direita
            //se tou andando pro norte ou sul e quero mudar minha direcao em x- viro p esquerda
            //se tou andando pro norte ou sul e quero mudar minha direcao em y- viro 2x p direita
            if (gameAi.direcao.Equals(Direcao.Norte) || gameAi.direcao.Equals(Direcao.Sul))
            {
                if (x < 0)
                {
                    client.sendTurnLeft();
                    if (gameAi.direcao.Equals(Direcao.Norte))
                    {
                        gameAi.AtualizaDirecao(Direcao.Oeste);
                    }
                    else
                    {
                        gameAi.AtualizaDirecao(Direcao.Leste);
                    }
                }
                else if (x > 0)
                {
                    client.sendTurnRight();
                    if (gameAi.direcao.Equals(Direcao.Norte))
                    {
                        gameAi.AtualizaDirecao(Direcao.Leste);
                    }
                    else
                    {
                        gameAi.AtualizaDirecao(Direcao.Oeste);
                    }
                }
                else
                {
                    client.sendTurnRight();
                    client.sendTurnRight();
                    if (gameAi.direcao.Equals(Direcao.Norte))
                    {
                        gameAi.AtualizaDirecao(Direcao.Sul);
                    }
                    else
                    {
                        gameAi.AtualizaDirecao(Direcao.Norte);
                    }
                }
            }
            //se tou andando pro leste ou oeste e quero mudar minha direcao em x- viro 2x p direita
            //se tou andando pro leste ou oeste e quero mudar minha direcao em y+ viro p direita
            //se tou andando pro leste ou oeste e quero mudar minha direcao em y- viro p esquerda
            else if (gameAi.direcao.Equals(Direcao.Leste) || gameAi.direcao.Equals(Direcao.Oeste))
            {
                if (y < 0)
                {
                    client.sendTurnLeft();
                    if (gameAi.direcao.Equals(Direcao.Leste))
                    {
                        gameAi.AtualizaDirecao(Direcao.Norte);
                    }
                    else
                    {
                        gameAi.AtualizaDirecao(Direcao.Sul);
                    }
                }
                else if (y > 0)
                {
                    client.sendTurnRight();
                    if (gameAi.direcao.Equals(Direcao.Leste))
                    {
                        gameAi.AtualizaDirecao(Direcao.Sul);
                    }
                    else
                    {
                        gameAi.AtualizaDirecao(Direcao.Norte);
                    }
                }
                else
                {
                    client.sendTurnRight();
                    client.sendTurnRight();
                    if (gameAi.direcao.Equals(Direcao.Leste))
                    {
                        gameAi.AtualizaDirecao(Direcao.Oeste);
                    }
                    else
                    {
                        gameAi.AtualizaDirecao(Direcao.Leste);
                    }
                }
            }
        }
        private void DesviaPoco()
        {
            var posAtual = gameAi.GetPlayerPosition();
            var posicoesPossiveis = gameAi.GetAllAdjacentPositions();
            List<Position> posicoesPosiveis2 = new List<Position>();
            foreach (var posicao in posicoesPossiveis)
            {
                if (gameAi.EsseTileExiste(posicao.x, posicao.y))
                {
                    if (mapa[posicao.x, posicao.y].espaco.Equals(Espaco.Desconhecido))
                    {
                        posicoesPosiveis2.Add(posicao);
                    }
                    else if (mapa[posicao.x, posicao.y].espaco.Equals(Espaco.Nada) || mapa[posicao.x, posicao.y].espaco.Equals(Espaco.PowerUp))
                    {
                        if (posicao.x != posAtual.x)
                        {
                            if (posicao.x > posAtual.x)
                            {
                                MudaDirecao(1, 0);
                            }
                            else
                            {
                                MudaDirecao(-1, 0);
                            }
                        }
                        if (posicao.y != posAtual.y)
                        {
                            if (posicao.y > posAtual.y)
                            {
                                MudaDirecao(0, 1);
                            }
                            else
                            {
                                MudaDirecao(0, -1);
                            }
                        }
                        AndaParaFrenteEObservar();
                    }
                    else if (mapa[posicao.x, posicao.y].espaco.Equals(Espaco.RiscoDePoco))
                    {
                        mapa[posicao.x, posicao.y].espaco = Espaco.Poco;
                    }
                }
            }
            foreach (var posicao in posicoesPosiveis2)
            {
                mapa[posicao.x, posicao.y].espaco = Espaco.RiscoDePoco;
            }
            ProcuraArtigos();
        }
        private void MudaEstado(Estado estado)
        {
            this.estadoAtual = estado;
        }
        private void AndaParaFrente()
        {
            var nextPos = gameAi.NextPosition();
            gameAi.SetPlayerPosition(nextPos.x,nextPos.y);
            client.sendForward();
//            Console.WriteLine("Andou");
        }
        private void AndaParaFrenteEObservar()
        {
            var nextPos = gameAi.NextPosition();
            gameAi.SetPlayerPosition(nextPos.x,nextPos.y);
            client.sendForward();
            client.sendRequestUserStatus();
            client.sendRequestObservation();
//            Console.WriteLine("Andou");
        }
    }
   
}
