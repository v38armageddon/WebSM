# ![Logo WebSM](Assets/WebSM.png) WebSM
A UWP App who you can have your Social Media in one application.

## What does it do?
WebSM allows you to have a quick access to your favorite social media.

You can listen to music, watching videos, and even chatting with your friends.

## Features
- All your favorite social media in one place.
- Quick and easy to use.

## Installation
Minimum Windows version required: Windows 10 2004

Recommended Windows version: Windows 10 21H2 / Windows 11 22H2

![https://apps.microsoft.com/store/detail/websm/9NVMBH7W0HXF](https://github.com/v38armageddon/WebSM/blob/master/Assets/Microsoft-store.png)

### Recommended installation
The best method to get WebSM is to install from the Microsoft Store: https://apps.microsoft.com/store/detail/websm/9NVMBH7W0HXF

### Alternate installation
If you want to install without the Microsoft Store, follow theses steps:

1. Download the .msix file and the .cer file.
2. If needed, authorize all sources in settings.
3. Install the certificate, see Q&A for installation.
3. Install the software.
4. Launch the software via the start menu.

## Q&A
Q: The installer tell me to use a certificate to install your application, how can I install it?

A: The certificate is distributed every release of the application. You can follow the instructions in this website: https://wsldl-pg.github.io/ArchW-docs/Install-Certificate/

Q: I can't connect to my Google account...

A: This issue is know due to Google Security, see https://github.com/MicrosoftEdge/WebView2Feedback/issues/1647

Q: Some website is broken, how can I solve this?

A: Depending on your bug, it can be a server-side problem or a problem in the embedded web browser. You can open a issue.

Q: What web technologie is used?

A: WebSM use the Microsoft WebView2 which is a embedded Microsoft Edge browser.
