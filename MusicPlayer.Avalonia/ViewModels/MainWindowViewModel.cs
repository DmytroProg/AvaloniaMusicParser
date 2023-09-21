using Avalonia.Controls;
using BusinessLogicLayer.Services;
using DynamicData;
using GalaSoft.MvvmLight.Command;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using MusicPlayer.BusinessLogicLayer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using OpenQA.Selenium.Interactions;
using ReactiveUI;
using System.Net;
using Avalonia.Media.Imaging;
using System.Text.RegularExpressions;

namespace MusicPlayer.Avalonia.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly SongParserService _songParserService;
        private readonly AlbumParserService _albumParserService;
        private Album _currentAlbum;
        private Bitmap _avatar = null;
        private bool _isEnabled;
        private string _parsingProcess;

        public string Url { get; set; }

        public Album CurrentAlbum {
            get => _currentAlbum;
            set => this.RaiseAndSetIfChanged(ref _currentAlbum, value);
        }

        public bool IsEnable {
            get => _isEnabled;
            set => this.RaiseAndSetIfChanged(ref _isEnabled, value);
        }

        public string ParsingProcess {
            get => _parsingProcess; 
            set => this.RaiseAndSetIfChanged(ref _parsingProcess, value);
        }
        
        public Bitmap Avatar
        {
            get => _avatar;
            set => this.RaiseAndSetIfChanged(ref _avatar, value);
        }

        public ObservableCollection<Song> Songs { get; set; }

        public MainWindowViewModel()
        {
            ParsingProcess = "";
            IsEnable = true;
            Songs = new ObservableCollection<Song>();

            _songParserService = new SongParserService();
            _albumParserService = new AlbumParserService();
        }

        public ICommand ParseCommand
        {
            get => new RelayCommand(() => ParseForSongs());
        }

        public void DownloadImage(string url)
        {
            using (WebClient client = new WebClient())
            {
                client.DownloadDataAsync(new Uri(url));
                client.DownloadDataCompleted += DownloadComplete;
            }
        }

        private void DownloadComplete(object sender, DownloadDataCompletedEventArgs e)
        {
            try
            {
                byte[] bytes = e.Result;

                Stream stream = new MemoryStream(bytes);

                var image = new Bitmap(stream);
                Avatar = image;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                Avatar = null;
            }

        }

        private void ClearAll()
        {
            CurrentAlbum = null;
            Songs.Clear();
            Avatar = null;
        }

        private async Task ParseForSongs()
        {
            ParsingProcess = "Parsing...";
            IsEnable = false;
            ClearAll();
            List<Song> songs = new List<Song>();

            try
            {
                CurrentAlbum = await _albumParserService.GetAll(Url);
                DownloadImage(CurrentAlbum.Avatar);

                _songParserService.SongsCount = null;
                if(int.TryParse(Regex.Match(CurrentAlbum.Description, @"^(\d+) ").Groups[1].Value, out int count))
                {
                    _songParserService.SongsCount = count;
                }
                songs.AddRange(await _songParserService.GetAll(Url));

                if (Url.Contains("/albums/"))
                {
                    songs.ForEach(song =>
                    {
                        song.Album = CurrentAlbum.Name;
                        song.Artist = CurrentAlbum.Artist;
                    });
                }
            }
            catch (ArgumentNullException)
            {
                var message = MessageBoxManager.GetMessageBoxStandard("Message",
                    "Please input a url",
                ButtonEnum.Ok);

                await message.ShowAsync();
            }
            catch (ArgumentException){
                var message = MessageBoxManager.GetMessageBoxStandard("Message", 
                    "Please input a url from amazon music in the category 'playlist'",
                ButtonEnum.Ok);

                await message.ShowAsync();
            }
            catch(Exception) {
                var message = MessageBoxManager.GetMessageBoxStandard("Message",
                        "Something went wrong while parsing the site, please try again later...",
                    ButtonEnum.Ok);

                await message.ShowAsync();
            }
            finally
            {
                IsEnable = true;
                ParsingProcess = "Done!";
                _albumParserService.CloseDriver();
                _songParserService.CloseDriver();
            }

            Songs.AddRange(songs);
        }
    }
}