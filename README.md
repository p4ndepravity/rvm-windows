# RVM For Windows

This repository contains the source code for my implementation of the Ruby
Version Manager for Windows. The application itself is written in C#, and
targets the .NET Framework Version 4.5.1. That said, it should work on all
versions of Windows from 7 and up.

## Table of Contents

*   Section 1 [Installation](#Installation)
*   Section 2 [Configuration](#Configuration)
*   Section 3 [Disclosures](#Disclosures)

## Installation

Installation comes in 2 major flavors...installer or DIY. Both methods work
equally well. However, the DIY method does come with some extra tweaking. I
will outline the full steps required for both methods though.

One thing that is common to both routes is you need to install the .NET
Framework yourself. If for some reason you don't have the .NET Framework
installed on your PC yet, you can grab the [online installer][.net-online] or
[offline installer][.net-offline] and install them, and you should be good to
move on.

### Method 1 - Installer

So, by this point, I assume you have the .NET Framework installed? Good,
because the installer checks for it. All you need to do now is head to the
[Releases Page][.releases] and grab whatever version suits your fancy. Then,
just install and refresh your path...and boom...it works. You are now ready to
head to the [configuration section](#Configuration) of this page.

### Method 2 - DIY

Again, I assume by this point you have the .NET Framework installed...it's kind
of required. Anyhow, once you have that sorted out...head over to the
[releases page][.releases] and snag the release of your choice, except instead
of the MSI installer, grab the zip file.

Now here is where things get tricky, wherever you extract the contents of this
ZIP file to, is going to be your RVM Home. This is standard for the
application, and poses no problems at all...if you use the installer. So, let's
just assume you ignore my warning here, and extract this ZIP file to your
downloads directory, so `rvm.exe` is located at:

```directory
C:\Users\Bob Smith\Downloads\rvm.exe
```

Well, everytime you install a new version of Ruby, say 2.2.3-x64 (not sure if
that version even exists, I pulled it from my a**), then it will create a new
directory in your Downloads folder called 2.2.3-x64, and then when you delete
that folder later because you realized what you did, well guess what? You
killed your Ruby install...they are symbolically linked...so think about your
location before you run RVM. That said, RVM also needs to be in your path.
Given that you are installing this manually, I'm going to assume you can update
your own path.

All that rant out of the way, you should now be up and running...so you can now
head on down to the [configuration section](#Configuration).

## Configuration

Guess what? You now have yet another app on your PC that uses JSON
configuration...I'm gonna be honest...I'm lazy and JSON is easy to implement in
.NET now that .NET Core came along. Plus, I hate XML with a PASSION! That said,
in that lovely configuration file, there are only 2 values you should want to
change, unless you plan on breaking things, but don't go submitting a bug
because you changed your config file and now Ruby is broken. I warned you.

Anyhow, the 2 values of interest are

```JSON
//User config values
"defaultArch": null,
"RUBY_HOME": null
```

**NOW, an IMPORTANT message about RUBY_HOME**
RUBY_HOME, **UNDER NO CIRCUMSTANCES**, can contain **ANY** spaces in the path.
Ruby **WILL** break if there are any spaces in this path...you have been
warned. If this value is null, it defaults to C:\\Ruby (generic, I know
:disappointed:, sadly I lack imagination...lol).

Anyhow, the other value on that list can be either `x64` or `i386`. That's it,
and it will only work on a 64-bit version of Windows, duh.

## Disclosures

### Did you build all those versions of Ruby yourself

No, no I most certainly did NOT compile the versions of Ruby used for install.
For that you can thank the guys over at [Ruby Installer][.ruby-installer], I am
using their builds...which are built using MSYS (hence the no spaces in the
path to Ruby).

### Sweet icon bro, where'd ya get it

Your mom. No really, I actually downloaded the gemstone graphic from
[Flaticon][.flaticon]. It was actually made by a user named [Freepik][.freepik]
. It is licensed under a [CC-3.0-BY License][.cc-30-by].

[.net-online]: https://www.microsoft.com/en-us/download/details.aspx?id=49981
[.net-offline]: https://www.microsoft.com/en-us/download/details.aspx?id=49982
[.releases]: https://github.com/Eagerestwolf/rvm-windows/releases
[.ruby-installer]: https://github.com/oneclick/rubyinstaller
[.flaticon]: http://www.flaticon.com
[.freepik]: http://www.freepik.com
[.cc-30-by]: http://creativecommons.org/licenses/by/3.0
