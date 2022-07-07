using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INF1771
{
    public class Estado
    {
        public string Nome { get; set; }
        public static Estado Bloqueado = new Estado("Bloqueado");
        public static Estado Passos = new Estado("Passos");
        public static Estado Brisa = new Estado("Brisa");
        public static Estado Flash = new Estado("Flash");
        public static Estado BlueLight = new Estado("BlueLight");
        public static Estado RedLight = new Estado("RedLight");
        public static Estado GreenLight = new Estado("GreenLight");
        public static Estado WeakLight = new Estado("WeakLight");
        public static Estado Dano = new Estado("Damage");
        public static Estado Hit = new Estado("Hit");
        public static Estado Desconhecido = new Estado("Unknown");

        public Estado (string nome)
        {
            Nome = nome;
        }
    }
}
