using BusinessLogicLayer.Interfaces;
using HtmlAgilityPack;
using MusicPlayer.BusinessLogicLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.Services
{
    public class SongParserService : ParserService<List<Song>>
    {
        public int? SongsCount;

        public override async Task<List<Song>> GetAll(string url)
        {
            if(string.IsNullOrEmpty(url) || string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException("Incorrent url", nameof(url));
            if (!url.StartsWith(@"https://music.amazon.com/"))
                throw new ArgumentException("Incorrent url", nameof(url));

            List<Song> list = new List<Song>();

            await OpenPage(url);

            bool isAlbum = url.Contains("/albums/");

            while (true)
            {
                var songs = (await GetSongsOnPage(isAlbum));
                list.AddRange(songs);
                list = list.DistinctBy(song => song.Name + song.Artist).ToList();

                MoveDownPage();

                if (SongsCount.HasValue)
                {
                    if (SongsCount.Value <= list.Count)
                    {
                        break;
                    }
                }
                else if (IsEndOfSite())
                {
                    break;
                }
            }

            int index = 1;
            list.ForEach(item => item.Index = index++);

            return list;
        }

        private async Task<List<Song>> GetSongsOnPage(bool isAlbum)
        {
            var doc = await LoadHtmlDocument();

            var details = isAlbum? GetDetailsOfAlbum(doc) : GetDetails(doc);

            List<Song> songs = new List<Song>();

            await Task.Run(() =>
            {
                for (int i = 0; i < details.Count; i += 3)
                {
                    if (!isAlbum)
                    {
                        songs.Add(new Song()
                        {
                            Name = details[i],
                            Artist = details[i + 1],
                            Album = details[i + 2],
                        });
                    }
                    else
                    {
                        songs.Add(new Song()
                        {
                            Name = details[i]
                        });
                        i -= 2;
                    }
                    
                }
                var durations = GetDurations(doc);
                for (int i = 0; i < songs.Count; i++)
                {
                    songs[i].Duration = durations[i];
                }
            });

            return songs;
        }

        private List<string> GetDetailsOfAlbum(HtmlDocument doc)
        {
            return doc.DocumentNode.Descendants("div")
                .Where(div => div.HasClass("content"))
                .Select(div => div.Descendants("span").First())
                .Select(span => span.InnerText)
                .Where(text => !string.IsNullOrEmpty(text))
                .ToList();
        }

        private List<string> GetDetails(HtmlDocument doc)
        {
            return doc.DocumentNode.Descendants("div")
                .Where(div => div.HasClass("content"))
                .Select(div => div.Descendants("a"))
                .SelectMany(a => a)
                .Select(a => a.InnerText)
                .ToList();
        }

        private List<string> GetDurations(HtmlDocument doc)
        {
            return doc.DocumentNode.Descendants("div")
                .Where(div => div.HasClass("col4"))
                .Select(div => div.Descendants("span").First())
                .Select(span => span.InnerText)
                .Where(text => !string.IsNullOrEmpty(text))
                .ToList();
        }
    }
}
