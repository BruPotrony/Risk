using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskModel
{
    public class DiceAtack
    {
        public Jugador Atacant { get; set; }
        public Jugador Defensor { get; set; }
        
        public List<Dice> tirades { get; set; } = new List<Dice>();
    }

    public class Dice
    {
        public int ResultatAtak { get; set; }
        public int ResultatDefense { get; set; }
    }
}
