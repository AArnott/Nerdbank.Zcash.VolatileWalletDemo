# Nerdbank.Zcash.VolatileWalletDemo

This repo demonstrates a trivial Zcash wallet written in C# using the [Nerdbank.Zcash](https://www.nuget.org/packages/nerdbank.zcash) library.

**WARNING**: This is for demo/educational purposes only. 
It doesn't store the spending key anywhere, so once the window is gone, all funds sent to this volatile wallet are lost.
You may want to copy the seed phrase down if you send any funds to the address it creates.

This is a cross-platform GUI app based on Avalonia.
Screenshots below show it running on both Windows and Linux (with the same exact binary build):

On Windows:
![image](https://github.com/AArnott/Nerdbank.Zcash.VolatileWalletDemo/assets/3548/1083b3e9-fd1e-40d5-8e7f-d0d0f453328c)

On Linux (well, WSL actually):
![image](https://github.com/AArnott/Nerdbank.Zcash.VolatileWalletDemo/assets/3548/4c248c53-a242-472a-af01-d265fc9714c8)
