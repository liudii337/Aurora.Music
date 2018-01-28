﻿// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace Aurora.Music.Controls
{
    public sealed partial class SleepTimer : ContentDialog
    {
        public SleepTimer()
        {
            this.InitializeComponent();
            Time.Time = DateTime.Now.TimeOfDay + TimeSpan.FromMinutes(10);
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var t = DateTime.Today;
            if (Time.Time < DateTime.Now.TimeOfDay)
            {
                t = t.AddDays(1) + Time.Time;
            }
            else
            {
                t += Time.Time;
            }

            SleepAction a;

            if ((bool)PlayPause.IsChecked)
            {
                a = SleepAction.Pause;
            }
            else if ((bool)Stop.IsChecked)
            {
                a = SleepAction.Stop;
            }
            else
            {
                a = SleepAction.Shutdown;
            }

            MainPage.Current.SetSleepTimer(t, a);
        }
    }
}
