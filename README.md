# Tilt Unlocker

### Summary

KSP Mod enabling to change the rotation axis of non solid bodies (scaled space only), every CelestialBodies that have no PQS can be edited (Gas giants, stars)

The mod requires the corresponding version of Kopernicus as your game as and is compatible with Scatterer and EVE.

### Installation

Download the last version corresponding to your game version such as

* TiltUnlocker_v0.1-1.8.1 for the 1.8.1 version
* TiltUnlocker_v0.1-1.7.3 for the 1.7.3 version

and *extract* the **content** of the GameData/ folder into your KSP GameData/

Don't forget to install the last corresponding Kopernicus version [here](https://github.com/Kopernicus/Kopernicus/releases)
 

### Configuration

Here is a configuration example to implement into a Kopernicus configuration file:

```
@Kopernicus:AFTER[Kopernicus]
{
    @Body[Jool]
    {
        Inclination
        {
            // Rotation of the axis on the x axis (degrees), Earth is 27.5°
            obliquity = 48.0

            // Rotation of the axis on the y axis (degrees), more information: https://en.wikipedia.org/wiki/Right_ascension
            rightAscension = 20.0
        }
    }
}
```