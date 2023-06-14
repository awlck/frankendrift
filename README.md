# FrankenDrift

[![Build status](https://ci.appveyor.com/api/projects/status/pulo20gx0tt2alhn/branch/master?svg=true)](https://ci.appveyor.com/project/awlck/frankendrift/branch/master)

A cross-platform frontend for the [ADRIFT Runner](https://adrift.co), built on the
[Eto.Forms](https://github.com/picoe/Eto) library. (Work in progress -- until I get around to
writing a more complete readme file, see [this IntFiction forum thread](https://intfiction.org/t/frankendrift-play-adrift-games-on-mac-and-linux/51528).)

## Prerequisites

### Windows, macOS
All dependencies are in the download.

### Linux
You will need to install the [.NET 6 Runtime](https://docs.microsoft.com/en-us/dotnet/core/install/linux)
from Microsoft.

(Versions prior to 0.3.0 use the .NET 5 runtime instead, and require the `libgdiplus` package for graphics.)

## Known limitations

The following features are known not to work:

* in-line graphics (graphics always display in a separate window)
* sound
* auto-map support is currently experimental.

## License

The frontend (FrankenDrift.Runner) and compatibility glue (FrankenDrift.Glue) are &copy;&nbsp;2021-22 Adrian
Welcker and distributed under the MIT license (see LICENSE). The ADRIFT logic code
(FrankenDrift.Adrift) is &copy;&nbsp;Campbell Wild and distributed under a 3-clause BSD license (see
FrankenDrift.Adrift/LICENSE.txt)
