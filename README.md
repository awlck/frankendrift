# FrankenDrift

A cross-platform frontend for the [ADRIFT Runner](https://adrift.co), built on the
[Eto.Forms](https://github.com/picoe/Eto) library. (Work in progress -- until I get around to
writing a more complete readme file, see [this IntFiction forum thread](https://intfiction.org/t/frankendrift-play-adrift-games-on-mac-and-linux/51528).)

## Prerequisites

### Windows, macOS
All dependencies are in the download.

### Linux
You will need to install the [.NET 6 Runtime](https://docs.microsoft.com/en-us/dotnet/core/install/linux)
from Microsoft. If you wish to play games that use graphics, you will also need to install `libgdiplus`
through your distribution's package manager. (Once you do that, enable graphics from the settings.)

(Versions prior to 0.3.0 use the .NET 5 runtime instead.)

## Known limitations

The following features are known not to work:

* in-line graphics (graphics always display in a separate window)
* sound
* the automatic map
* font switching is limited (some common monospace font names are recongnized and the program will attempt to
  substitute one that is available on your system.)
* setting the default text font/size (color works, though)

## License

The frontend (FrankenDrift.Runner) and compatibility glue (FrankenDrift.Glue) are &copy;&nbsp;2021-22 Adrian
Welcker and distributed under the MIT license (see LICENSE). The ADRIFT logic code
(FrankenDrift.Adrift) is &copy;&nbsp;Campbell Wild and distributed under a 3-clause BSD license (see
FrankenDrift.Adrift/LICENSE.txt)
