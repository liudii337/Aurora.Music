﻿using Aurora.Shared.Extensions;
using Aurora.Shared.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aurora.Music.Core.Storage;
using Aurora.Music.Core.Models;
using Windows.UI;
using Windows.UI.Xaml.Media;
using Aurora.Music.Core;

namespace Aurora.Music.ViewModels
{
    class GenericMusicItemViewModel : ViewModelBase
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Addtional { get; set; }

        public Uri Artwork
        { get; set; }

        private bool isOnline;
        public bool IsOnline
        {
            get { return isOnline; }
            set { SetProperty(ref isOnline, value); }
        }

        private bool isAvaliable;
        public bool IsAvaliable
        {
            get { return isAvaliable; }
            set { SetProperty(ref isAvaliable, value); }
        }

        public Color MainColor { get; set; }

        public int[] IDs { get; set; }
        public string[] OnlineIDs { get; set; }
        public string OnlineAlbumID { get; }
        public int ContextualID { get; set; }

        public GenericMusicItemViewModel()
        {

        }

        public string OnlineToSymbol(bool b)
        {
            return b ? "\uE93E" : "\uEC4F";
        }

        public MediaType InnerType { get; set; }


        public static void ColorToHSV(System.Drawing.Color color, out double hue, out double saturation, out double value)
        {
            int max = Math.Max(color.R, Math.Max(color.G, color.B));
            int min = Math.Min(color.R, Math.Min(color.G, color.B));

            hue = color.GetHue();
            saturation = (max == 0) ? 0 : 1d - (1d * min / max);
            value = max / 255d;
        }


        public static Color ColorFromHSV(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            var v = Convert.ToByte(value);
            var p = Convert.ToByte(value * (1 - saturation));
            var q = Convert.ToByte(value * (1 - f * saturation));
            var t = Convert.ToByte(value * (1 - (1 - f) * saturation));

            if (hi == 0)
                return Color.FromArgb(255, v, t, p);
            else if (hi == 1)
                return Color.FromArgb(255, q, v, p);
            else if (hi == 2)
                return Color.FromArgb(255, p, v, t);
            else if (hi == 3)
                return Color.FromArgb(255, p, q, v);
            else if (hi == 4)
                return Color.FromArgb(255, t, p, v);
            else
                return Color.FromArgb(255, v, p, q);
        }

        public SolidColorBrush GetMainColorBrush(double d)
        {
            System.Drawing.Color color = System.Drawing.Color.FromArgb(MainColor.R, MainColor.G, MainColor.B);
            ColorToHSV(color, out var h, out var s, out var v);
            v *= d;

            return new SolidColorBrush(ColorFromHSV(h, s, v));
        }

        public Color GetMainColor(double d)
        {
            System.Drawing.Color color = System.Drawing.Color.FromArgb(MainColor.R, MainColor.G, MainColor.B);
            ColorToHSV(color, out var h, out var s, out var v);
            v *= d;

            return ColorFromHSV(h, s, v);
        }

        public GenericMusicItemViewModel(Album album)
        {
            InnerType = MediaType.Album;
            ContextualID = album.ID;
            Title = album.Name;
            Addtional = string.Join(Consts.CommaSeparator, album.AlbumArtists);
            Description = SmartFormat.Smart.Format(Consts.Localizer.GetString("SmartSongs"), (album.Songs.Length + album.Songs.Length));
            Artwork = album.PicturePath.IsNullorEmpty() ? null : new Uri(album.PicturePath);
            IDs = album.Songs;
        }

        public GenericMusicItemViewModel(Song song)
        {
            InnerType = MediaType.Song;
            ContextualID = song.ID;
            Title = song.Title;
            Addtional = song.Performers.IsNullorEmpty() ? Consts.UnknownArtists : string.Join(Consts.CommaSeparator, song.Performers);
            Description = song.Album;
            Artwork = song.PicturePath.IsNullorEmpty() ? null : new Uri(song.PicturePath);
            IDs = new int[] { song.ID };
        }

        public GenericMusicItemViewModel(GenericMusicItem item)
        {
            // TODO: online pic path
            if (item is OnlineMusicItem o)
            {
                IsOnline = true;
                OnlineIDs = o.OnlineID;
                OnlineAlbumID = o.OnlineAlbumId;
            }
            else
            {
                ContextualID = item.ContextualID;
                IDs = item.IDs;
            }
            InnerType = item.InnerType;
            Title = item.Title;
            Description = item.Description;
            Addtional = item.Addtional;
            Artwork = item.PicturePath.IsNullorEmpty() ? null : new Uri(item.PicturePath);
        }

        internal virtual async Task<IList<Song>> GetSongsAsync()
        {
            if (IsOnline)
            {
                if (MainPageViewModel.Current.OnlineMusicExtension == null)
                {
                    return null;
                }
                var list = new List<Song>();
                foreach (var item in OnlineIDs)
                {
                    var s = await MainPageViewModel.Current.GetOnlineSongAsync(item);
                    if (s == null)
                        continue;
                    list.Add(s);
                }
                return list;
            }
            else
            {
                var opr = SQLOperator.Current();

                var s = await opr.GetSongsAsync(IDs);
                var s1 = s.OrderBy(x => x.Track);
                s1 = s1.OrderBy(x => x.Disc);
                return s1.ToList();
            }
        }

        public override string ToString()
        {
            if (Title.IsNullorEmpty())
            {
                return string.Empty;
            }
            if (Description.IsNullorEmpty())
            {
                return Title;
            }
            var title = Title;
            if (title.Length > 20)
            {
                title = title.Substring(0, 20);
                title += "…";
            }
            return $"{title} - {Description}";
        }

        internal async Task<AlbumViewModel> FindAssociatedAlbumAsync()
        {
            var opr = SQLOperator.Current();
            switch (InnerType)
            {
                case MediaType.Song:
                    if (Description.IsNullorEmpty())
                    {
                        return null;
                    }
                    if (IsOnline)
                    {
                        if (OnlineAlbumID.IsNullorEmpty())
                            return null;
                        return new AlbumViewModel(await MainPageViewModel.Current.GetOnlineAlbumAsync(OnlineAlbumID));
                    }
                    return new AlbumViewModel(await opr.GetAlbumByNameAsync(Description, ContextualID));
                case MediaType.Album:
                    if (ContextualID == default(int))
                    {
                        return new AlbumViewModel(await opr.GetAlbumByNameAsync(Title));
                    }
                    return new AlbumViewModel(await opr.GetAlbumByIDAsync(ContextualID));
                case MediaType.PlayList:
                    throw new NotImplementedException();
                case MediaType.Artist:
                    throw new InvalidCastException("This GenericMusicItemViewModel is artist");
                default:
                    return null;
            }
        }
    }
}
