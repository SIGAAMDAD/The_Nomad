# SimpleMultiMesh
A quick and easy way to get multimeshes running. Access by adding a new child node of type "SimpleMultiMesh2D" or "SimpleMultiMesh3D".

The system takes care of managing instance and visible count automatically, so you can add and remove nodes from the assigned group without worrying about showing false positives.

Group Name: The name of the group whose transforms you want to use. The system will grab all nodes in that group and use their transforms to render the multimesh.

Update Mode: Which process is responsible for updating the mesh transforms.
- Once: Only updates on _ready(), disables processing
- Process: Updates every frame
- Physics: Updates every physics frame
