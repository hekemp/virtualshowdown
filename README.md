# virtualshowdown
Virtual Showdown (eyes-free Virtual Reality game)

Requirements:
- Windows OS 
- Kinect 2.0 
- [Kinect 2.0 SDK](https://www.microsoft.com/en-us/download/details.aspx?id=44561)
- [Kinect for Windows SDK](http://www.microsoft.com/en-us/download/details.aspx?id=40278)
- Nintendo Switch Joy-Con
- Unity

Optional Requirements: 
- Bluetooth Enabled Computer

Setting up the Kinect:
1. Place the Kinect at least 3 feet off of the ground by placing it on a table, mantel, or bookshelf. If there is no tall surface to place the Kinect, rotate the box that packaged the Kinect sensor so it is at it's tallest. Place this box on a stable surface, and place the Kinect at the top of the box. 
2. The front of the Kinect will have 3 circular indents. The Kinect should face an open space so you have plenty of room to move safely. While we recommend at least 6 feet of open space for your safety, the device can be used in smaller spaces, so long as you can stand approximately 3 feet away from the device.
3. Once the Kinect has been placed, plug the power cable for the Kinect sensor into the wall. Depending on how your Kinect came packaged, you may need to attatch a wire from your Kinect into the power pack of this cable to provide it power.
4. Plug the USB cable for the Kinect sensor into your computer's USB port. Your computer should automatically download and install the required files. This may take several minutes.

Running VR Showdown (From Unity):

Running VR Showndown (From .exe):

Pairing the Nintendo Switch Joy-Con to Your Computer:

1. Open your PC's settings menu (Windows Key + i)
2. Select "Devices"
3. Ensure that you are currently on the "Bluetooth & other devices" tab. You should be, as this is the default tab.
4. If your Bluetooth toggle isn't on, be sure to turn it on at this time.
5. On the top of the menu, select "Add Bluetooth or other device". You will want to select the "Bluetooth" option.
![A picture of the menu selecting Bluetooth.](https://www.windowscentral.com/sites/wpcentral.com/files/styles/xlarge/public/field/image/2017/10/add-device-bluetooth.jpg?itok=BR2OeEP4)
6. Hold down your Joy-Con's sync button, which is the tiny black dot located between the SL and SR buttons.
![A picture of the sync button, which is between the SL and SR buttons.](https://img.purch.com/fullsizerender-jpg/o/aHR0cDovL21lZGlhLmJlc3RvZm1pY3JvLmNvbS9BL0IvNjkxMTM5L29yaWdpbmFsL0Z1bGxTaXplUmVuZGVyLmpwZw==)
7. Select Joy-Con (L) or Joy-Con (R) from the menu. The name will differ whether you are using the left or right Joy-Con. Your controller is now paired!

Troubleshooting "Pairing the Nintendo Switch Joy-Con to Your Computer":

My Joy-Con's syncing lights continue to flash even after following these steps!

This is, unfortunately, just how the Joy-Con is programmed to work when synced with a computer. Provided that you ran into no errors during these steps, your device will be paired and connected correctly, though.

During syncing, the computer asks for a pin!

Thankfully, the Switch uses the default pins for Bluetooth devices. Try "0000", and if this doesn't work, try "1234". If neither of these work, please consult your Joy-Con manual or provider, as they may have changed the pin for the device.

Troubleshooting:

Unity Services Error:

Initially, this project was created in Unity teams. Consequently, there occasionally, when first loading the project for the first time, be some errors regarding the legacy bits of configs for Unity Services. The way the team has managed to get around this is by clicking the "New Link" option to invoke a new set-up menu session and then leaving the menu. This will prevent these errors in the project, at least until a suitable solution can be achieved to remove this bug.

DllNotFoundException: Kinect Unity Addin:

If you run into this error on the console of Unity, this means that there is something wrong with your instalation of the [Kinect for Windows SDK](http://www.microsoft.com/en-us/download/details.aspx?id=40278) or that it may be missing altogether. Be sure to go through the installation again and ensure there was no errors.


Developer References:

Sample Kinect Files: https://developer.microsoft.com/en-us/windows/kinect (Scroll down to "NuGet and Unity Pro add-ons" and download "Unity Pro packages" for sample projects
