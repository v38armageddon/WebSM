# ![WebSM logo](Assets/WebSM.png) WebSM
A simple minimalist web browser on Uno Platform.

## What does it do?
WebSM allows you to have a simple web browser with a minimalist design.

## Features
- Simple web browser.
- Easy to use.

## Installation
### Windows
Minimum Windows version required: Windows 11 24H2

Recommended Windows version: Windows 11 Latest build

![https://apps.microsoft.com/store/detail/websm/9NVMBH7W0HXF](Assets/Microsoft-store.png)

#### Recommended installation
The best method to get WebSM is to install from the Microsoft Store: https://apps.microsoft.com/store/detail/websm/9NVMBH7W0HXF

#### Alternate installation
If you want to install without the Microsoft Store, follow theses steps:
1. Download the desktop version file from the releases page.
2. Launch the installer.

### Linux
Depending on your distribution you can install WebSM via different methods.

#### Vincent OS
Vincent OS repository includes all version of WebSM, you can install it via the following command:
```bash
pacman -S websm
# Or if you prefer the Lite version
pacman -S websm-lite
```

#### Flatpak
For other distributions, you can install the flatpak version:
```bash
# Add our Flatpak repostiory
flatpak remote-add --if-not-exists v38armageddon https://repo.v38armageddon.net/flatpak/v38armageddon.flatpakrepo

flatpak install net.v38armageddon.WebSM
# For the Lite version
flatpak install net.v38armageddon.WebSM.Lite
```

### Android (Experimental)
You can download the signed ``.apk`` file on the [Release](https://github.com/v38armageddon/WebSM/releases) page for all versions.

Keep in mind that this is still experimental, unexpected bugs and crashes can occur.

---------
## Build from Source
### Visual Studio 2026
You need to have the [Uno Platform extension](https://marketplace.visualstudio.com/items?itemName=unoplatform.uno-platform-addin-2022) installed on your IDE.

1. Open ``WebSM.slnx`` | ``WebSM.Lite.slnx``
2. Press F5
3. That's it!

### dotnet CLI
1. Run ``dotnet build ./WebSM.slnx`` | ``dotnet build ./WebSM.Lite.slnx``
2. That's it!

---------
## Contributions
WebSM is open to all contributions! You can read more at the CONTRIBUTE.md file.
