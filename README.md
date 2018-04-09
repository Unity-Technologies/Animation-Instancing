
# Motivation
In our developing, we always want to save both CPU and GPU. As larger and larger scenes, more and more characters, the cost rises faster and faster. We often implement outdoors scene with GPU Instancing, such as grasses and trees. But for SkinnedMeshRenderer(for example characters), we can’t use instancing. Because the skinning is calculated on CPU, and submitted to GPU one by one. In case there are lots of SkinnedMeshRenderer in the scene, there are lots of Drawcalls and animation calculations. The case is usually occurred in our developing. This technique, Animation Instancing, is designed to instance Characters. 
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

