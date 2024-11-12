Two Isues :

-Prune is not efficient
-OverConnection among SOM Layer



Bad Ideas : 
1. Instead of depolarizing every single neuron for temporal and apical inputs , check if there is already a depolarized neuron and add voltage. // Helps with better cleanup
-> For temporal inputs acorss of 100's of neurons this might throw of firing to a lot of neurons which might affect apical depolarizations.