RENDER FEATUES
====================================================================================================
This package is intended to have a bunch of render features to display several GPU operations.

Linear Depth
------------------
The _Linear Depth Render Feature_ contains an integrated render feature to be used in Unity6 URP that
prints into an Editor Window the result of gathering the main camera's depth texture, linearize and
normalize it.

To use it, create a new Unity 6 project, open up the Package Manager and install the UPM package 
from git. The package includes a sample scene that can be used to see the results. You can either
create your own scene but take into account that it requires at least a camera set with "MainCamera"
tag and the "PC Renderer" attached to it into the Camera's rendering settings within the component.

[cover1]
