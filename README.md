# there_will_be_game
"I have a competition in me. I want no one to succeed. I hate most people"

## Notes

Make sure to install git LFS: 
https://docs.github.com/en/repositories/working-with-files/managing-large-files/installing-git-large-file-storage.


## Controls: 
### Caligula: 
The hammer is intended to be controlled with our custom arduino controller. However, for development, you can connect an iOS or Android phone to your computer with a wire in order to test the motion controls. To do this: 
- Connect your phone to your computer
- Install the Unity Remote 5 app on your phone and open it
- Go to File > Build Profiles in the Unity Editor and change the platform to iOS or Android
- (If you have an iPhone and a windows computer, you'll need to install the 'Apple Devices' app on your computer)
- Go to Edit > Project Settings > Editor and change the 'Device' option to your device. 

### Incitatus
By default, Incitatus can be controlled with WASD or the arrow keys (Space to jump). 
STEERING WHEEL EXPLANATION GOES HERE!

## Unity Editor Game View Screenshots

The recorder package should be installed when you open the project in Unity.

You can use it to create screenshots (or video recordings) of the game view that get saved to the Assets/Recordings folder.

To enable quick screenshots:
1. go to Window -> General -> Recorder -> Recorder Settings
2. Set Recording Mode to Single Frame
3. (Optionally) Disable Exit Play Mode so that you can take multiple screenshots in one play
4. Add Recorder -> Image Sequence (for screenshots)
5. Set the Output Resolution to whatever you desire
6. Now you should be able to use F10 to quickly take screenshots.
You may want to move the Recorder window to the side so that it doesn't get in the way when taking multiple screenshots in succession.