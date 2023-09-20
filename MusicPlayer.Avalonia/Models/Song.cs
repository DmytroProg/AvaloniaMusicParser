using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicPlayer.Avalonia.Models
{
    public class Song
    {
        public string Name { get; set; } = null!;
        public string Artist { get; set; } = null!;
        public string Album { get; set; } = null!;
        public string Duration { get; set; } = null!;
    }
}
