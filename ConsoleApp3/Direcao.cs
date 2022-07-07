using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INF1771
{
    public class Direcao
    {
        public string Nome { get; set; }
        public static Direcao Norte = new Direcao("Norte");
        public static Direcao Sul = new Direcao("Sul");
        public static Direcao Leste = new Direcao("Leste");
        public static Direcao Oeste = new Direcao("Oeste");
        public Direcao (string nome)
        {
            Nome = nome;
        }
    }
}
