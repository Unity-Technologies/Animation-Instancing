
As developers, we’re always aware of performance, both in terms of CPU and GPU. Maintaining good performance gets more challenging as scenes get larger and more complex, especially as we add more and more characters. Me and my colleague in Shanghai come across this problem often when helping customers, so we decided to dedicate a few weeks to a project aimed to improve performance when instancing characters. We call the resulting technique Animation Instancing.
> It needs at least Unity5.4.

# Features:
* Instancing SkinnedMeshRenderer 
* root motion
* attachments
* LOD
* Support mobile platform
* Culling

> Note:
Before running the example, you should select menu Custom Editor -> AssetBundle -> BuildAssetBundle to build asset bundle.

# Attachments:
There's a attachment scene. It shows how to use the attachments. 
How to setup the object which hold the attachment?
* Open the generator menu -> AnimationInstancing -> Animation Generator
* Enable the attachment checkbox 
* Selece the fbx which refrenced by the prefab
* Enable the skeleton's name to generate
* Press the Generate button.