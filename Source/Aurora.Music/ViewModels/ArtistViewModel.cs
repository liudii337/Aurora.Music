﻿using Aurora.Music.Core;
using Aurora.Music.Core.Models;
using Aurora.Music.Core.Storage;
using Aurora.Shared.Extensions;
using Aurora.Shared.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.System.Threading;
using Windows.UI.Xaml.Media.Imaging;

namespace Aurora.Music.ViewModels
{
    class ArtistViewModel : ViewModelBase, IKey
    {
        public string RawName;

        private string description;
        public string Description
        {
            get { return description.IsNullorEmpty() ? Name : description; }
            set { SetProperty(ref description, value); }
        }

        private Uri avatar;
        public Uri Avatar
        {
            get { return avatar; }
            set
            {
                if (avatar?.OriginalString == value?.OriginalString)
                {
                    return;
                }
                SetProperty(ref avatar, value);
                if (avatar == null)
                {
                    AvatarImage = null;
                }
                AvatarImage = new BitmapImage(avatar)
                {
                    DecodePixelHeight = 128,
                    DecodePixelType = DecodePixelType.Logical
                };
                var t = ThreadPool.RunAsync(async x =>
                {
                    await Core.Storage.SQLOperator.Current().UpdateAvatarAsync(RawName, value.OriginalString);
                });
            }
        }

        private BitmapImage avatarImage;
        public BitmapImage AvatarImage
        {
            get { return avatarImage; }
            set { SetProperty(ref avatarImage, value); }
        }

        private string name;
        public string Name
        {
            get { return name; }
            set
            {
                if (value.IsNullorWhiteSpace())
                {
                    SetProperty(ref name, Consts.UnknownArtists);
                    RawName = string.Empty;
                }
                else
                {
                    SetProperty(ref name, value.Replace(Consts.ArraySeparator, Consts.CommaSeparator));
                    RawName = value;
                }
            }
        }

        private int albumCount;
        public int SongsCount
        {
            get { return albumCount; }
            set { SetProperty(ref albumCount, value); }
        }

        public string Key
        {
            get
            {
                if (Name.IsNullorEmpty())
                {
                    return " ";
                }
                if (Name.StartsWith("The ", System.StringComparison.CurrentCultureIgnoreCase))
                {
                    return Name.Substring(4);
                }
                if (Name.StartsWith("A ", System.StringComparison.CurrentCultureIgnoreCase))
                {
                    return Name.Substring(2);
                }
                if (Name.StartsWith("An ", System.StringComparison.CurrentCultureIgnoreCase))
                {
                    return Name.Substring(3);
                }
                return Name;

            }
        }

        public string CountToString(int count)
        {
            return SmartFormat.Smart.Format(Consts.Localizer.GetString("SmartSongs"), count);
        }

        internal async Task<IList<Song>> GetSongsAsync()
        {
            var albums = await FileReader.GetAlbumsAsync(RawName);

            var songs = new List<Song>();
            albums.ForEach(async x =>
            {
                var m = new AlbumViewModel(x);
                songs.AddRange(await m.GetSongsAsync());
            });
            return songs;
        }
    }
}
