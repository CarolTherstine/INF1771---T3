using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INF1771
{
    public class Tile
    {
        public Espaco espaco { get; set; }
        public Espaco GetTipoEspaco()
        {
            return this.espaco;
        }
        public Tile()
        {
            espaco = Espaco.Desconhecido;
        }
    }
}
