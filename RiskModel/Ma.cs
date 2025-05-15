using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskModel
{
    public class MA
    {
        public long CartaId { get; set; }
        public long JugadorId { get; set; }
        public Carta Carta { get; set; }
        public Jugador Jugador { get; set; }
    }

}
