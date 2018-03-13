Motivation
In our developing, we always want to save both CPU and GPU. As larger and larger scenes, more and more characters, the cost rises faster and faster. We often implement outdoors scene with GPU Instancing, such as grasses and trees. But for SkinnedMeshRenderer(for example characters), we can’t use instancing. Because the skinning is calculated on CPU, and submitted to GPU one by one. In case there are lots of SkinnedMeshRenderer in the scene, there are lots of Drawcalls and animation calculations. The case is usually occurred in our developing. This technique, Animation Instancing, is designed to instance Characters. 


Goals
Our initial goals for this experimental project were:
Instancing SkinnedMeshRenderer 
Implement mostly animation features *1
LOD
Support mobile platform *2
Culling

Not all goals were reached due to time constraints. However, we felt that the experiment was a success in that parts of the vision proved out. Let’s dig into some of the details.

# Animation-Instancing
