using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RiskModel
{
    public class Jugador : INotifyPropertyChanged
    {
        public long Id { get; set; }
        public long SkfUserId { get; set; }
        public Color ColorJugador { get; set; }
        public long SkfPartidaId { get; set; }
        public int SkfNumero { get; set; }
        public Usuari SkfUser { get; set; }
        public Partida SkfPartida { get; set; }
        public System.Windows.Media.Brush ColorBrush { get; set; }




        private bool _isHisTurn;
        public bool isHisTurn
        {
            get => _isHisTurn;
            set
            {
                if (_isHisTurn != value)
                {
                    _isHisTurn = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        

    }

}
