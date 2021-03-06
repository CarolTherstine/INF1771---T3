using INF1771_GameAI.Map;
using INF1771_GameClient.dto;
using INF1771_GameClient.Socket;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Timers;

namespace INF1771
{
    public class Bot
    {
        #region Logicas_Pre_Implementadas
        private string name = "INF1771 Bot Example1";
        private string host = "192.168.0.42";

        HandleClient client = new HandleClient();
        Dictionary<long, PlayerInfo> playerList = new Dictionary<long, PlayerInfo>();
        List<ShotInfo> shotList = new List<ShotInfo>();
        List<ScoreBoard> scoreList = new List<ScoreBoard>();
        Estado estadoAtual;
        Tile[,] mapa = new Tile[59, 34];


        GameAI gameAi = new GameAI();

        private System.Timers.Timer timer1 = new System.Timers.Timer();
        long time = 0;

        bool FirstTime = true;

        String gameStatus = "";
        String sscoreList = "";

        List<String> msg = new List<String>();
        double msgSeconds = 0;
        double TimeElapsed = 0;
        int TimeClock = 0;
        bool AchouItem = false;
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
            client.sendName("Testinho");
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

                                        //gameAi.GetObservations(o);
                                        this.estadoAtual = gameAi.GetObservations(o);
                                        Console.WriteLine($"{estadoAtual.Nome.ToString()}");
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
                                    Console.WriteLine($"Notification: {cmdArgs.cmd[1]}");
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
                                   
                                    this.estadoAtual = gameAi.GetObservations(o);
                                    msg.Add("you hit " + cmdArgs.cmd[1]);
                                    Console.WriteLine("You Hit");
                                }
                                break;
                            case "d":
                                if (cmdArgs.cmd.Length > 1)
                                {
                                    List<String> o = new List<String>();
                                    o.Add("damage");
                                    
                                    this.estadoAtual = gameAi.GetObservations(o);
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
                client.sendColor(Color.HotPink);
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
                if (!AchouItem)
                {
                    ProcuraUmArtigoParaCamperar();
                }
                else
                {
                    CampaItem();
                }
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
        private void ProcuraUmArtigoParaCamperar()
        {
            Console.WriteLine(estadoAtual.Nome);
            if (FirstTime)
            {
                //Implementar uma funcao q anda ate encontrar um artigo e fica pegando ele enquanto respawna e ganhaluta c qm vier matar ele
                client.sendRequestPosition();
                client.sendRequestObservation();
                client.sendRequestUserStatus();
                client.sendRequestGameStatus();
                FirstTime = false;
            }
            if (estadoAtual.Equals(Estado.Hit) || estadoAtual.Equals(Estado.Dano) || estadoAtual.Equals(Estado.Passos))
            {
                for (var i =0; i<= 10; i++)
                {
                    client.sendShoot();
                }
                estadoAtual = Estado.Desconhecido;
            }
            var proxPos = gameAi.NextPosition();
            if (gameAi.EsseTileExiste(proxPos.x, proxPos.y) && estadoAtual.Equals(Estado.Desconhecido))
            {
                AndaParaFrenteEObservar();
                //AndaParaFrente();
                var posAtual = gameAi.GetPlayerPosition();
                mapa[posAtual.x, posAtual.y].espaco = Espaco.Nada;
            }
            else if (!gameAi.EsseTileExiste(proxPos.x, proxPos.y) && estadoAtual.Equals(Estado.Desconhecido))
            {
                var posicoes = gameAi.GetObservableAdjacentPositions();
                foreach (var posicaoProx in posicoes)
                {
                    //Se n for fora do mapa
                    var posicaoAtual = gameAi.GetPlayerPosition();
                    if (gameAi.EsseTileExiste(posicaoProx.x, posicaoProx.y))
                    {
                        if (!mapa[posicaoProx.x, posicaoProx.y].espaco.Equals(Espaco.Poco) && !mapa[posicaoProx.x, posicaoProx.y].espaco.Equals(Espaco.Parede))
                        {
                            MudaDirecao(posicaoAtual.x - posicaoProx.x, posicaoAtual.y - posicaoProx.y);
                            Console.WriteLine($"a direcao atual eh {gameAi.direcao}");
                            AndaParaFrenteEObservar();
                            //AndaParaFrente();
                            estadoAtual = Estado.Desconhecido;
                            break;
                        }
                    }
                }
            }
            else if (gameAi.EsseTileExiste(proxPos.x, proxPos.y) && mapa[proxPos.x, proxPos.y].espaco.Equals(Espaco.Nada))
            {
                AndaParaFrente();
            }
            else if (estadoAtual.Equals(Estado.Bloqueado) || estadoAtual.Equals(Estado.Brisa))
            {
                var posicao = gameAi.NextPosition();
                var posicaoAtual = gameAi.GetPlayerPosition();
                //desviar
                if (gameAi.EsseTileExiste(posicao.x, posicao.y))
                {
                    mapa[posicao.x, posicao.y].espaco = Espaco.Parede;
                }
                var posicoes = gameAi.GetObservableAdjacentPositions();
                foreach (var posicaoProx in posicoes)
                {
                    //Se n for fora do mapa
                    if (gameAi.EsseTileExiste(posicaoProx.x, posicaoProx.y))
                    {
                        if (!mapa[posicaoProx.x, posicaoProx.y].espaco.Equals(Espaco.Poco) && !mapa[posicaoProx.x, posicaoProx.y].espaco.Equals(Espaco.Parede))
                        {
                            MudaDirecao(posicaoAtual.x - posicaoProx.x, posicaoAtual.y - posicaoProx.y);
                            if (!mapa[gameAi.NextPosition().x, gameAi.NextPosition().y].espaco.Equals(Espaco.Desconhecido))
                            {
                                AndaParaFrenteEObservar();
                            }
                            else
                            {
                                //mudar p nao ficar em loop
                                MudaDirecao(0, 1);
                            }
                            estadoAtual = Estado.Desconhecido;
                            break;
                        }
                    }
                }
            }

            else if (estadoAtual.Equals(Estado.WeakLight) || estadoAtual.Equals(Estado.GreenLight) || estadoAtual.Equals(Estado.BlueLight))
            {
                //achou item
                TimeClock = 0;
                AchouItem = true;
                var nextPos = gameAi.NextPosition();
                var pos = gameAi.GetPlayerPosition();
                //Evita ficar virado p parede
                if (!gameAi.EsseTileExiste(nextPos.x, nextPos.y) || (gameAi.EsseTileExiste(nextPos.x, nextPos.y) && (mapa[nextPos.x, nextPos.y].espaco.Equals(Espaco.Poco) || mapa[nextPos.x, nextPos.y].espaco.Equals(Espaco.Parede))))
                {
                    MudaDirecao(pos.x - nextPos.x, pos.y - nextPos.y);
                }    
            }
            else
            {
                Console.WriteLine($"bugo {estadoAtual.Nome}");
                estadoAtual = Estado.Desconhecido;
            }
        }
        private void CampaItem()
        {
            TimeClock += 1;
            if (TimeClock >= 10)
            {
                client.sendRequestObservation();
                client.sendRequestScoreboard();
                TimeClock = 0;
            }
            //Lutar ou esperar o tempo
            if (estadoAtual.Equals(Estado.WeakLight) || estadoAtual.Equals(Estado.GreenLight) || estadoAtual.Equals(Estado.BlueLight))
            {
                client.sendGetItem();
                estadoAtual = Estado.Desconhecido;
            }
            if (estadoAtual.Equals(Estado.Hit) || estadoAtual.Equals(Estado.Dano) || estadoAtual.Equals(Estado.Passos))
            {
                var nextPos = gameAi.NextPosition();
                if (!mapa[nextPos.x, nextPos.y].espaco.Equals(Espaco.Parede) && !mapa[nextPos.x, nextPos.y].espaco.Equals(Espaco.Poco))
                {
                    for (var i = 0; i < 10; i++)
                    {
                        client.sendShoot();
                    }
                    estadoAtual = Estado.Desconhecido;
                }
                else
                {
                    var posicoesPossiveis = gameAi.GetObservableAdjacentPositions();
                    var posAtual = gameAi.GetPlayerPosition();
                    foreach (var pos in posicoesPossiveis)
                    {
                        if (!mapa[pos.x, pos.y].espaco.Equals(Espaco.Parede) && !mapa[pos.x, pos.y].espaco.Equals(Espaco.Poco))
                        {
                            MudaDirecao(posAtual.x - pos.x, posAtual.y - pos.y);
                            for (var i = 0; i < 10; i++)
                            {
                                client.sendShoot();
                            }
                        }
                    }
                }
                client.sendRequestObservation();
            }
        }
        #region Metodos_De_Apoio
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
        private void MudaEstado(Estado estado)
        {
            this.estadoAtual = estado;
        }
        private void AndaParaFrente()
        {
            client.sendForward();
            var nextPos = gameAi.NextPosition();
            gameAi.SetPlayerPosition(nextPos.x, nextPos.y);
                        Console.WriteLine("Andou");
        }
        private void AndaParaFrenteEObservar()
        {
            var nextPos = gameAi.NextPosition();
            gameAi.SetPlayerPosition(nextPos.x, nextPos.y);
            client.sendForward();
            client.sendRequestUserStatus();
            client.sendRequestObservation();
            //            Console.WriteLine("Andou");
        }
        private void ObservarEAndarPraFrente()
        {
            client.sendRequestObservation();
            if (!estadoAtual.Equals(Estado.Brisa) && !estadoAtual.Equals(Estado.Bloqueado))
            {
                var nextPos = gameAi.NextPosition();
                gameAi.SetPlayerPosition(nextPos.x, nextPos.y);
                client.sendForward();
            }
            else
            {
                var nextPositions = gameAi.GetObservableAdjacentPositions();
                foreach (var pos in nextPositions)
                {
                    if (gameAi.EsseTileExiste(pos.x, pos.y) && !mapa[pos.x, pos.y].espaco.Equals(Espaco.Poco) && mapa[pos.x, pos.y].espaco.Equals(Espaco.Poco))
                    {
                        var posAtual = gameAi.GetPlayerPosition();
                        MudaDirecao(posAtual.x - pos.x, posAtual.y - pos.y);
                        gameAi.SetPlayerPosition(pos.x, pos.y);
                        client.sendForward();
                    }
                }
            }
        }
        #endregion Metodos_De_Apoio
        #region Implementacoes_Antigas_Descartadas
        private void ProcuraArtigos()
        {
            //Pego a posicao atual para poder andar
            var posAtual = gameAi.GetPlayerPosition();
            //Se n for fora do mapa
            if (gameAi.EsseTileExiste(posAtual.x, posAtual.y) && mapa[posAtual.x, posAtual.y].espaco.Equals(Espaco.Desconhecido))
            {
                mapa[posAtual.x, posAtual.y].espaco = Espaco.Nada;
            }
            //Pego pra onde andar
            var posicao = gameAi.NextPosition();
            //Checo se n esta fora do mapa
            if (gameAi.EsseTileExiste(posicao.x, posicao.y))
            {
                if ((mapa[posicao.x, posicao.y].GetTipoEspaco().Equals(Espaco.Desconhecido)))
                {
                    if (!estadoAtual.Equals(Estado.Bloqueado) && !estadoAtual.Equals(Estado.Brisa) && !estadoAtual.Equals(Estado.Hit) && !estadoAtual.Equals(Estado.Dano))
                    {
                        if (mapa[posicao.x, posicao.y].espaco.Equals(Espaco.Desconhecido))
                        {
                            AndaParaFrenteEObservar();
                        }
                        else
                        {
                            AndaParaFrente();
                        }
                    }
                    if (estadoAtual.Equals(Estado.Bloqueado))
                    {
                        //mapa[posicao.x, posicao.y].espaco = Espaco.Parede;
                        mapa[posicao.x, posicao.y].espaco = Espaco.Parede;
                        var posicoes = gameAi.GetAllAdjacentPositions();
                        foreach (var posicaoProx in posicoes)
                        {
                            //Se n for fora do mapa
                            if (gameAi.EsseTileExiste(posicaoProx.x, posicaoProx.y))
                            {
                                if (!mapa[posicaoProx.x, posicaoProx.y].espaco.Equals(Espaco.Poco) && !mapa[posicaoProx.x, posicaoProx.y].espaco.Equals(Espaco.Parede))
                                {
                                    //MudaDirecao(posicao.x - posicaoProx.x, posicao.y - posicaoProx.y);
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
                    else if (estadoAtual.Equals(Estado.WeakLight))
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
                        DesviaPoco();
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
                        var posicoes2 = posicoes.Where(x => x != posicao);
                        foreach (var posicaoProx in posicoes)
                        {
                            if (gameAi.EsseTileExiste(posicaoProx.x, posicaoProx.y))
                            {
                                if (!mapa[posicaoProx.x, posicaoProx.y].espaco.Equals(Espaco.Poco) && !mapa[posicaoProx.x, posicaoProx.y].espaco.Equals(Espaco.Parede))
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
        /* private void ProcurarArtigos2()
         {
             var posAtual = gameAi.GetPlayerPosition();
             if (estadoAtual.Equals(Estado.Bloqueado))
             {
                 mapa[posAtual.x, posAtual.y].espaco = Espaco.Nada;
                 var posPossiveis = gameAi.GetObservableAdjacentPositions();
                 foreach (var pos in posPossiveis)
                 {
                     if (mapa[pos.x, pos.y].espaco.Equals(Espaco.Nada) || mapa[pos.x, pos.y].espaco.Equals(Espaco.PowerUp))
                     {
                         MudaDirecao(posAtual.x - pos.x, posAtual.y - pos.y);
                         AndaParaFrente();
                         if (mapa[pos.x, pos.y].espaco.Equals(Espaco.PowerUp))
                         {
                             client.sendGetItem();
                         }
                         break;
                     }
                 }
             }
             else if (estadoAtual.Equals(Estado.WeakLight) || estadoAtual.Equals(Estado.GreenLight) || estadoAtual.Equals(Estado.BlueLight))
             {
                 AndaParaFrente();
                 client.sendGetItem();
                 estadoAtual = Estado.Desconhecido;
             }
             else if (estadoAtual.Equals(Estado.Desconhecido))
             {
                 //client.sendRequestObservation();
                 //AndaParaFrenteEObservar();
                 ObservarEAndarPraFrente();
             }
             else if (estadoAtual.Equals(Estado.Hit) || estadoAtual.Equals(Estado.Dano))
             {
                 LutaComInimigos();
             }
             else if (estadoAtual.Equals(Estado.Brisa))
             {
                 DesviaPoco();
             }
             else
             {
                 Console.WriteLine("Error");
             }
         }
        */
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
                    //Nao esta fora do mapa
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
                    MudaEstado(Estado.Desconhecido);
                    ProcuraArtigos();
                }
            }
        }
        public void ContinuaAtirar()
        {
            for (int i = 0; i < 10; i++)
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
        #endregion Implementacoes_Antigas_Descartadas
    }
}