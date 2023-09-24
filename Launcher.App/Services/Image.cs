﻿using Launcher.Abstractions;
using System.IO;
using System.Windows.Media.Imaging;

namespace Launcher.App.Services;

internal class Image : IImage
{
    public BitmapImage BitmapImage { get; }

    public Image(Stream stream)
    {
        BitmapImage = new BitmapImage();

        BitmapImage.BeginInit();
        BitmapImage.CacheOption = BitmapCacheOption.OnLoad;
        BitmapImage.StreamSource = stream;
        BitmapImage.EndInit();

        BitmapImage.Freeze();
    }
}
