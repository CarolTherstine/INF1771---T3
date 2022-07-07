using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INF1771
{
    public class Espaco
    {
        public string Name { get; set; }
        public static Espaco PowerUp = new Espaco("PowerUp");
        public static Espaco Poco = new Espaco("Poco");
        public static Espaco Parede = new Espaco("Parede");
        public static Espaco Desconhecido = new Espaco("Desconhecido");
        public static Espaco Nada = new Espaco("Nada");
        public Espaco (string nome)
        {
            Name = nome;    
        }
    }
}
