# Unity3D Client connecting to the Neurorobotics Platform (http://www.neurorobotics.net/)

## Installation

To install the NRP backend + web frontend, follow instructions at http://neurorobotics.net/local_install.html

Once this repository is cloned
- clone the models repository at https://bitbucket.org/hbpneurorobotics/models as well
- in a terminal, go to Unity3D-Client/Assets/Models/scripts and run 
```
python ./copy_flat_hierarchy.py <path_to_models_repository>
```

For the user avatar model, make sure to set scale factor for all geometry (visuals and collision) in Unity to 0.01 and disable "use file scale".

## Connecting to an experiment

Open the project in Unity Editor, then load the NRPClient scene.

Use services -> BackendConfigService to enter IP and port config of your backend.

## VR Re-Embodiment / Spawning your own virtual body

Have your VR setup running (HTC Vive tested). Add additional trackers for feet and body as desired.

Use the "Grip" gesture of your input controller to spawn your virtual body.

TIP: make sure the NRP simulation is running (not paused). No running physics engine means no movement of your robot body.
