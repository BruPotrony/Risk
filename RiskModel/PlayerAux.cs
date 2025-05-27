using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskModel
{
    public class PlayerAux
    {

        public PlayerAux(string AvatarUrl, long Id, string Username)
        {
            this.AvatarUrl = AvatarUrl;
            this.Id = Id;
            this.Username = Username;
        }

        public string AvatarUrl { get; set; }
        public long Id { get; set; }
        public string Username { get; set; }
    }
}
