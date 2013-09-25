## Overview

This project is a straight Monotouch port of the Google Analytics SDK for Windows to hosted on Codeplex.

The library assumes that you have a file "analytics.xml" in the root of your main project with its build action set to "Content". Please refer to the [documentation the Google Analytics SDK for Windows](https://googleanalyticssdk.codeplex.com/wikipage?title=Getting%20Started&referringTitle=Documentation) for further information about this file and for instructions on how to get started with the library. 

### Source Code Structure

Everything under **GoogleAnalytics.Common** has been copied verbatim from the original project and should be updated and maintained accordingly.  
## Caveats

Since UIAppDelegate does not expose any .Net style application lifecycle events, you will need to call the associated methods (OnApplicationActivated and OnApplicationDeactivated) of the EasyTracker object yourself if you want to track the lifecycle using Google Analytics. 

