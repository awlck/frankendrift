# FrankenDrift

A cross-platform frontend for the [ADRIFT Runner](https://adrift.co), built on the
[Eto.Forms](https://github.com/picoe/Eto) library. (Work in progress -- until I get around to
writing a more complete readme file, see [this IntFiction forum thread](https://intfiction.org/t/frankendrift-play-adrift-games-on-mac-and-linux/51528).)

## Prerequisites

### Windows
Starting with Alpha 6, all dependencies are included in the download. (To run Alpha 5 or earlier, you will
need to install the [.NET 5 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/5.0).)

### macOS
Everything (including the runtime) is included in `.dmg` file.

### Linux
You will need to install the [.NET 5 Runtime](https://docs.microsoft.com/en-us/dotnet/core/install/linux)
from Microsoft. If you wish to play games that use graphics, you will also need to install `libgdiplus`
through your distributions's package manager. (Once you do that, enable graphics from the settings.)

## Known limitations

The following features are known not to work:

* in-line graphics (graphics always display in a separate window)
* sound
* the automatic map
* font switching is limited (some common monospace font names are recongnized and the program will attempt to
  substitute one that is available on your system.)
* setting the default text font/size (color works, though)

## License

The frontend (FrankenDrift.Runner) and compatibility glue (FrankenDrift.Glue) are (c) 2021-22 Adrian
Welcker and distributed under the MIT license (see LICENSE). The ADRIFT logic code
(FrankenDrift.Adrift) is (c) Campbell Wild and distributed under a 3-clause BSD license (see
FrankenDrift.Adrift/LICENSE.txt)
