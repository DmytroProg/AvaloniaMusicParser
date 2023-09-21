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
    public class AlbumParserService : ParserService<Album>
    {
        public override async Task<Album> GetAll(string url)
        {
            if (string.IsNullOrEmpty(url) || string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException("Incorrent url", nameof(url));
            if (!url.StartsWith(@"https://music.amazon.com/"))
                throw new ArgumentException("Incorrent url", nameof(url));


            await OpenPage(url);

            var doc = await LoadHtmlDocument();
            Album album = new Album();

            album.Name = doc.DocumentNode.Descendants("music-detail-header").Last().Attributes["headline"].Value;
            album.Description = doc.DocumentNode.Descendants("music-detail-header").Last().Attributes["tertiary-text"].Value;
            album.Avatar = doc.DocumentNode.Descendants("music-detail-header").Last().Attributes["image-src"].Value;
            album.Artist = doc.DocumentNode.Descendants("music-detail-header").Last().Attributes["primary-text"].Value;

            return album;
        }
    }
}
