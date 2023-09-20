using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicPlayer.Avalonia.Models
{
    public class Playlist
    {
        public string Name { get; set; } = null!;
        public string Avatar { get; set; } = null!;
        public string Description { get; set; } = null!;
        public List<Song> Songs { get; set; }

        public Playlist()
        {
            Songs = new List<Song>();
        }
    }
}
