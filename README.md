<img style="height: 5rem;image-rendering: pixelated;" src="https://cdn.discordapp.com/attachments/848562143120850944/1107344010726609026/pngsmasher_icon.gif"> <img style="max-height: 5.5rem;" src="https://cdn.discordapp.com/attachments/848562143120850944/1107344010349129928/pngsmasher_logo.gif">

# pngsmasher v2.1
(unfortunately, because github decided for absolutely no reason to break all APNGs, you'll have to press the download links under animated images instead of downloading the animated images, since those are apngs converted to gifs)

## What is this?
pngsmasher is my unique take on making an image glitching tool!
this is a C# rewrite of its predecessor, [pngf\*\*\*er](https://github.com/andrew-eathan/pngfucker), which was written in NodeJS.

to see what pngsmasher can do, check out its examples, both in the README and in the wiki!

pngsmasher is also fast, and even on my weak Intel i3-2120, it only takes ~20-30ms for most images! (assuming average parameters and image sizes like in the examples)
for reference, running the animated cube example takes ~500ms for 100 frames for me

pngsmasher only exclusively supports the PNG and APNG formats, because i haven't found decent image processing libraries for C# that were both fast and low-sized (looking at you, ImageMagick.NET, with your 35 mb dlls, lol)

## Why would i want this?
have you gotten tired of seeing this generic [JPEG glitch effect](https://cdn.discordapp.com/attachments/701973402973634681/916383906243178496/glitch.jpeg) that everyone uses, and you want something new? then pngsmasher is for you!

## Setup
download the [latest release](https://github.com/andrew-eathan/pngsmasher/releases) (pick whatever released binary fits your system) and try running some of the examples below in the README!

## Usage
for a full usage guide check out the [pngsmasher wiki](https://github.com/andrew-eathan/pngsmasher/wiki), which is filled with examples explaining every parameter you can use in pngsmasher!

if you wish to use pngsmasher in your own project, you must abide by the LICENSE in this repository
other than that, you can download the latest pngsmasher.core library release for use in your own .NET project

## Contents
pngsmasher is currently made out of four projects:
- #### pngsmasher.cli
	a full command line program replica of pngf\*\*\*er's command-line interface, that lets anyone glitch PNGs and APNGs (animated PNGs)
- #### pngsmasher.core
	this is the heart of pngsmasher, containing all of the corruption and RGBA manipulation functions i have written for this project! it contains some util functions i have made myself, and some that are borrowed and rewritten in C# from online sources
- #### APNGLib
	this is a version of [APNGManagement](https://github.com/murrple-1/APNGManagement "APNGManagement"), an APNG encoder/decoder, that i modified for use in pngsmasher
- ##### pngsmasher.tests
	basic pngsmasher function tests by Marioalexsan

## Examples
### Default images used in the examples below:
### Image #1: scientist.png  

<img src="https://cdn.discordapp.com/attachments/515580681707847702/1059898949617594459/scientist.png" style="height: 8rem;">  

### Image #2: cube.png  

<img src="https://cdn.discordapp.com/attachments/848562143120850944/1107347423812128868/cube.gif" style="height: 8rem;">  

### Image #3: apngtest.png  

<img src="https://cdn.discordapp.com/attachments/848562143120850944/1107348282847539200/apngtest.gif" style="height: 8rem;">  

### Image #4: lucy.png (random pfp)  

<img src="https://cdn.discordapp.com/attachments/515580681707847702/1059928316443889723/lucy.png" style="height: 8rem;">  

the original png/apng input files:  
[originals.zip](https://github.com/Andrew-Eathan/pngsmasher/files/11472442/originals.zip)

#
#
# NOTE: some of the animated examples are probably too flashy for people with epilepsy, so uh yeah
#
#

### Example #1: `pngsmasher.cli -input scientist.png -splits 6 -seed 2 -crunch 60 +bg +o`
the image was crunched to 60% of its size, and "split"-corrupted in 6 different parts, and a (by default black) background was underlayed, due to the corruption causing parts of the image to go transparent
+o toggles the overwrite switch, by default pngsmasher tries not to overwrite existing output images (by appending _(number) at the end)

![output.png](https://cdn.discordapp.com/attachments/515580681707847702/1059898654703493139/output.png)

### Example #2: `pngsmasher.cli -input scientist.png -regions 2 -rmin -4 -rmax -3 -shift -1 -crunch 40 -contrast 0.4 +bg -bgclamp +ntsc -xblur 8 -xblurpower 0.75 -seed 4122`
`+ntsc` enables my attempt at an NTSC effect, with -xblur and -xblurpower to control the horizontal blurring

"regional corruption" was applied to the image twice: -rmin -4 means that the minimum height of regions will be 256 / 4, and -rmax -3 means that the maximum will be 256 / 3
image crunching & contrasting is also applied (-1 to +1 range)

![output.png](https://cdn.discordapp.com/attachments/515580681707847702/1059910763529121812/output.png)

### Example #3: `pngsmasher.cli -input cube.png -output outputcube.png -splits 3 -randshift 1 -regions 1 -rmin -5 -rmax -3 -breaks 20 -crwidth 50 -crheight 90 -frames 60 -fps 15 -seed 2089938189 +ntsc -fringe 1 -xblur 4 -bgred 8 -bggreen 0 -bgblue 16 +bg`
has NTSC, a black background, and instead of being crushed to a percentage, this image is crushed to a specific width and height, corrupted, and returned back to its default size
the seed is random number keyboard mash

![outputcube.png](https://cdn.discordapp.com/attachments/848562143120850944/1107348789234253965/outputcube.gif)

### Example #4: `pngsmasher.cli -input apngtest.png -seed 4 -regions 3 -splits 2 -crunch 70 +ntsc -fringe 5 -xblur 8 -xblurpower 1 +bg +bgred 64 +bggreen 0 +bgblue 128`

![output.png](https://cdn.discordapp.com/attachments/848562143120850944/1107349195037360230/output.gif)

### Example #5: `pngsmasher.cli -input apngtest.png -seed 10 -regions 1 -crunch 20 +grayabb -grayabbsize 2 -grayabbdetune 1 -splits 2 -randshift 2 +ntsc -fringe 1 -xblur 3 -crwidth 50 -crheight 90 +bg +o -div 2 +bg -bgclamp +clamp`
![output.png](https://cdn.discordapp.com/attachments/848562143120850944/1107349441863766176/output.gif)

### Example #6: `pngsmasher.cli -input lucy.png -output lucy_pngsmashed.png -seed 10 -regions 1 -crunch 20 +grayabb -grayabbsize 2 -grayabbdetune 1 -splits 2 -randshift 2 -frames 40 -fps 10 -breaks 40 +ntsc -xblur 3 -crwidth 50 -crheight 90 +bg +o -div 2 +bg -bgclamp`
this "grayscale chroma abberation" effect was an accident that turned out to look pretty cool, so i kept it in

![lucy_pngsmashed.png](https://cdn.discordapp.com/attachments/848562143120850944/1107349702896259103/lucy_pngsmashed.gif)

### Example #7: `pngsmasher.cli -input lucy.png -seed 10 -regions 1 -crunch 20 +grayabb -grayabbsize 2 -grayabbdetune 1 -splits 2 -randshift 2 -breaks 40 +ntsc -fringe 1 -xblur 3 -crwidth 50 -crheight 90 +bg +o -div 2 +bg -bgclamp`
same as #6, just no animation (no -frames and -fps)

![output.png](https://media.discordapp.net/attachments/515580681707847702/1059925145168392363/output.png)

### Example #8: `pngsmasher.cli -input lucy.png -seed 10 -regions 1 -crunch 20 +grayabb -grayabbsize 2 -grayabbdetune 1 -splits 2 -randshift 2 -breaks 40 +ntsc -fringe 0 -xblur 2 -xblurpower 1 -crwidth 50 -crheight 90 +bg +o -div 2 +bg -bgclamp`
same as example #7, without the NTSC effect

![output.png](https://cdn.discordapp.com/attachments/515580681707847702/1059925984448622643/output.png)

#

originally, there should have been more animated examples, but while making these examples i found out a couple of bugs, and while i did fix all of them, `+clamp` doesn't work for some reason  
anyway, here are the options used for the pngsmasher logo:  

pslogo.png:  
![pslogo](https://github.com/Andrew-Eathan/pngsmasher/assets/42521608/18d937ec-104f-45c9-b493-07775a20987a)  
`pngsmasher.cli -input pslogo.png -output pngsmasher_logo.png -splits 6 -regions 2 -splitmin 20 -crunch 80 -seed 10 -frames 30 -fps 10 +grayabb -grayabbwidth 2 -grayabbpower 0.75 +bg -bgred 0 -bggreen 0 -bgblue 0 +ntsc -fringe 1 -breaks 30`
