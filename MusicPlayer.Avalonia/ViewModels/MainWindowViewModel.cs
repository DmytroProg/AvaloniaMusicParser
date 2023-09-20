using Avalonia.Controls;
using DynamicData;
using GalaSoft.MvvmLight.Command;
using HtmlAgilityPack;
using MusicPlayer.Avalonia.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Tmds.DBus.Protocol;

namespace MusicPlayer.Avalonia.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private ChromeDriver chromeDriver;
        private readonly string url = @"https://music.amazon.com/playlists/B01M11SBC8";
        private Playlist _currentPlaylist;

        public string Greeting => "Welcome to Avalonia!";

        public ObservableCollection<string> Playlists { get; set; }
        public ObservableCollection<Song> Songs { get; set; }

        public MainWindowViewModel()
        {
            Playlists = new ObservableCollection<string>();
            Songs = new ObservableCollection<Song>();
        }

        public ICommand ParseCommand
        {
            get => new RelayCommand(() =>
            {
                chromeDriver = CreateChromeDriver(url);
                ParseForSongs();
            });
        }

        private ChromeDriver CreateChromeDriver(string url)
        {
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArgument("--window-position=-32000,-32000");
            var chromeDriveService = ChromeDriverService.CreateDefaultService();
            chromeDriveService.HideCommandPromptWindow = true;
            var driver = new ChromeDriver(chromeDriveService, chromeOptions);
            driver.Navigate().GoToUrl(url);
            return driver;
        }

        private async Task<string> ReadHtmlDocument()
        {
            await Task.Delay(1000);
            string dynamicHtml = chromeDriver.PageSource;

            return dynamicHtml;
        }

        private async Task ParseForSongs()
        {

            List<Song> list = new List<Song>();

            while (true)
            {
                var songs = (await GetSongs());
                list.AddRange(songs);
                list = list.Distinct().ToList();
                if (list.Count >= 50)
                    break;
                MoveDownPage();
            }

            Songs.Clear();
            Songs.AddRange(list);
            Playlists.Clear();
            Playlists.AddRange(list.Select(x => x.Album));

            await Task.Run(() => chromeDriver.Close());
        }

        private async Task<HtmlDocument> LoadHtmlDocument()
        {
            var htmlString = await ReadHtmlDocument();

            var doc = new HtmlDocument();
            doc.LoadHtml(htmlString);

            return doc;
        }

        private async Task<List<Song>> GetSongs()
        {
            var doc = await LoadHtmlDocument();

            var details = GetDetails(doc);

            List<Song> songs = new List<Song>();

            await Task.Run(() =>
            {
                for (int i = 0; i < details.Count; i += 3)
                {
                    songs.Add(new Song()
                    {
                        Name = details[i],
                        Artist = details[i + 1],
                        Album = details[i + 2]
                    });
                }
            });

            return songs;
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

        private void MoveDownPage()
        {
            new Actions(chromeDriver)
                .ScrollByAmount(0, 1500)
                .Perform();
        }

        ~MainWindowViewModel()
        {
            try
            {
                chromeDriver?.Close();
            }
            catch (Exception) { }
        }
    }
}