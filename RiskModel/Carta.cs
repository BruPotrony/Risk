using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskModel
{
    public class Carta
    {
        public long Id { get; set; }
        public long Tipus { get; set; }
        public long? PaisId { get; set; }
        public TipusCarta TipusCarta { get; set; }
        public Pais Pais { get; set; }
    }

}
