# FrankenDrift

A cross-platform frontend for the [ADRIFT Runner](https://adrift.co), built on the
[Eto.Forms](https://github.com/picoe/Eto) library. (Work in progress -- until I get around to
writing a more complete readme file, see [this IntFiction forum thread](https://intfiction.org/t/frankendrift-play-adrift-games-on-mac-and-linux/51528).)

## Known limitations

The following features are known not to work:

* in-line graphics (graphics always display in a separate window)
* sound
* the automatic map
* font switching
* setting the default text font/size/color
* save/restore menu entries (typing `save` or `restore` on the prompt works, though)

## License

The frontend (FrankenDrift.Runner) and compatibility glue (FrankenDrift.Glue) are (c) 2021 Adrian
Welcker and distributed under the MIT license (see LICENSE). The ADRIFT logic code
(FrankenDrift.Adrift) is (c) Campbell Wild and distributed under a 3-clause BSD license (see
FrankenDrift.Adrift/LICENSE.txt)
