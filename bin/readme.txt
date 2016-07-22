=========== OLEG ENGINE ============
FOOHY MADE THIS
https://github.com/Foohy/gravity_car
http://foohy.net
====================================

1) The settings file
	The file is stored in a simple human readable-editable JSON format. If it doesn't exist, the engine will recreate it with default settings. Else, it'll load it up and use the settings described

	WHAT EACH SETTING DOES (THERE ISN'T A LOT):
	VSync: Sets the vsync mode.
		0 - off
		1 - on
		2 - adaptive

	WindowMode: How the window will appear
		0 - Normal (windowed)
		1 - Minimized
		2 - Maximized
		3 - Fullscreen

	NoBorder: Remove the border of the window (Useful for appearing like fullscreen but acting like a window)
	Width/Height: The resolution of the screen. This is rather self explanatory
	ShadowMapSize: The size (width and height) of the shadowmap. Higher values will result in a better looking map but is more perfomance intensive.
	Samples: MSAA samples for antialiasing
	AnisotropicFiltering: How much anisotropic filtering should be applied to textures. This makes textures that aren't perpendicular to the camera look better.
	GlobalVolume: Global volume override for all audio.
	ShowFPS: Draw an FPS indicator ingame
	ShowConsole: Show the debug console. Do this if bad things are happening.

2) I'm not responsible if it breaks your everything
	I was nowhere near isle 7
