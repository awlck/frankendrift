# FrankenDrift

[![Build status](https://ci.appveyor.com/api/projects/status/pulo20gx0tt2alhn/branch/master?svg=true)](https://ci.appveyor.com/project/awlck/frankendrift/branch/master)

A cross-platform frontend for the [ADRIFT Runner](https://adrift.co), built on the
[Eto.Forms](https://github.com/picoe/Eto) library. (Work in progress -- until I get around to
writing a more complete readme file, see [this IntFiction forum thread](https://intfiction.org/t/frankendrift-play-adrift-games-on-mac-and-linux/51528).)

## Prerequisites and Installation

### Windows
All dependencies are included in the download. Simply unpack the ZIP file somewhere and run `FrankenDrift.Runner.Win.exe`.

### macOS
All dependencies are included in the download. Simply copy `FrankenDrift.Runner.Mac.app` to your
Applications folder, right-click it, and select "Open".

### Linux
Starting with release 0.7.0, we have stand-alone downloads available for x64 and arm64 systems.
These are still somewhat experimental, but you should be able to simply extract the .tar.gz file
for your architecture to somewhere and run `FrankenDrift.Runner.Gtk`.

If those aren't working for you, the traditional, framework-dependent version of the Gtk runner is still available as "`frankendrift-v0.x.y-gtk.any.zip`". You will need to install the [.NET 8 Runtime](https://docs.microsoft.com/en-us/dotnet/core/install/linux) from Microsoft, then unzip the download and run `FrankenDrift.Runner.Gtk`.

Both versions require the GTK3 libraries to be installed on your system.

*(The framework-dependent version can theoretically be run on Windows, albeit with some effort. It is currently not possible to run this version on macOS.)*


## Known limitations

The following features are known not to work:

* in-line graphics (graphics always display in a separate window)
* sound
* auto-map support is currently experimental.

## License

The frontends (FrankenDrift.Runner, FrankenDrift.GlkRunner, et al.) and compatibility glue (FrankenDrift.Glue) are &copy;&nbsp;2021-24 Adrian
Welcker and distributed under the MIT license (see LICENSE). The ADRIFT logic code
(FrankenDrift.Adrift) is &copy;&nbsp;Campbell Wild and distributed under a 3-clause BSD license (see
FrankenDrift.Adrift/LICENSE.txt)
