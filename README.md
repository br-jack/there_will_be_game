# there_will_be_game
"I have a competition in me. I want no one to succeed. I hate most people"

## Notes

Make sure to install git LFS: 
https://docs.github.com/en/repositories/working-with-files/managing-large-files/installing-git-large-file-storage.


Motion controls: 
- Hammer must be controlled with a connected wii remote with motion plus. 
- You can test that Unity is recieving motion control data from the wii remote by using the 'hammerTest' scene
- Code to control the hammer with the Unity Remote app is outdated. We can re-add it soon - it works very similarly to a wii remote
- Wiimote connections are managed by two scripts attached to the Hammer object - HammerBehaviour and DebugHammer. The WiimoteApi is mostly stored in Scripts/WiimoteApi, but there are also hidapi.dll files for using wiimotes with different OSs stored in Plugins. They should in theory work on linux, mac, and both x32 and x64 windows, but I think there are lots of differences between Bluetooth drivers on different devices/OS versions and so they may not.
- Project version is now iOS, which was done in order for Unity Remote to work. This doesn't appear to affect the game in any way and does make it much easier to test the motion controls with Unity Remote