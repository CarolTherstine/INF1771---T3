using INF1771_GameAI.Map;

namespace INF1771
{
    public class GameAI
    {
        Position player = new Position();
        String state = "ready";
        String dir = "north";
        long score = 0;
        int energy = 0;
        public const int MaxX = 59;
        public const int MaxY = 34;
        public Direcao direcao = Direcao.Norte;

        public void SetStatus(int x, int y, String dir, String state, long score, int energy)
        {
            player.x = x;
            player.y = y;
            this.dir = dir.ToLower();
            this.direcao = GetDirecao(dir);

            this.state = state;
            this.score = score;
            this.energy = energy;
        }

        public Direcao GetDirecao (string dir)
        {
            if (dir.Equals("north", StringComparison.OrdinalIgnoreCase))
                return Direcao.Norte;
            else if (dir.Equals("south", StringComparison.OrdinalIgnoreCase))
                return Direcao.Sul;
            else if (dir.Equals("east", StringComparison.OrdinalIgnoreCase))
                return Direcao.Leste;
            else
                return Direcao.Oeste;
        }

        public List<Position> GetObservableAdjacentPositions()
        {
            List<Position> ret = new List<Position>();

            ret.Add(new Position(player.x - 1, player.y));
            ret.Add(new Position(player.x + 1, player.y));
            ret.Add(new Position(player.x, player.y - 1));
            ret.Add(new Position(player.x, player.y + 1));

            return ret;
        }

        public List<Position> GetAllAdjacentPositions()
        {
            List<Position> ret = new List<Position>();

            ret.Add(new Position(player.x - 1, player.y - 1));
            ret.Add(new Position(player.x, player.y - 1));
            ret.Add(new Position(player.x + 1, player.y - 1));

            ret.Add(new Position(player.x - 1, player.y));
            ret.Add(new Position(player.x + 1, player.y));

            ret.Add(new Position(player.x - 1, player.y + 1));
            ret.Add(new Position(player.x, player.y + 1));
            ret.Add(new Position(player.x + 1, player.y + 1));

            return ret;
        }

        public bool EsseTileExiste (int x, int y)
        {
            if (x < MaxX && y < MaxY && x >= 0 && y >= 0)
            {
                return true;
            }
            return false;
        }
        public void AtualizaDirecao (Direcao direcaoNova)
        {
            this.direcao = direcaoNova;
        }

        public Position NextPosition()
        {
            Position ret = null;
            switch (dir)
            {
                case "north":
                    ret = new Position(player.x, player.y - 1);
                    break;
                case "east":
                    ret = new Position(player.x + 1, player.y);
                    break;
                case "south":
                    ret = new Position(player.x, player.y + 1);
                    break;
                case "west":
                    ret = new Position(player.x - 1, player.y);
                    break;
            }

            return ret;
        }

        public Position GetPlayerPosition()
        {
            return player;
        }

        public void SetPlayerPosition(int x, int y)
        {
            player.x = x;
            player.y = y;

        }

        public Estado GetObservations(List<String> o)
        {
            String cmd = "";
            Console.WriteLine("Observacoes recebidas: ");
            foreach (String s in o)
            {
                switch (s)
                {
                    case "blocked":
                        //Console.WriteLine("Blocked");
                        return Estado.Bloqueado;
                    case "steps":
                        //Console.WriteLine("Steps");
                        return Estado.Passos;
                    case "breeze":
                        //Console.WriteLine("Breeze");
                        return Estado.Brisa;
                    case "flash":
                        //Console.WriteLine("Flash");
                        return Estado.Flash;
                    case "blueLight":
                        //Console.WriteLine("BlueLight");
                        return Estado.BlueLight;
                    case "redLight":
                        //Console.WriteLine("RedLight");
                        return Estado.RedLight;
                    case "greenLight":
                        //Console.WriteLine("GreenLight");
                        return Estado.GreenLight;
                    case "weakLight":
                        //Console.WriteLine("WeakLight");
                        return Estado.WeakLight;
                    case "damage":
                        return Estado.Dano;
                    case "hit":
                        return Estado.Hit;
                }
            }
            return Estado.Desconhecido;
        }

        public void GetObservationsClean()
        {

        }

        public string GetDecision()
        {
            Random rand = new Random();

            int n = rand.Next(0, 8);
            switch (n)
            {
                case 0:
                    return "virar_direita";
                case 1:
                    return "virar_esquerda";
                case 2:
                    return "andar";
                case 3:
                    return "atacar";
                case 4:
                    return "pegar_ouro";
                case 5:
                    return "pegar_anel";
                case 6:
                    return "pegar_powerup";
                case 7:
                    return "andar_re";
            }

            return "";
        }


    }
}
