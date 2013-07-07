This example shows how to load a milkshape model. I decided to load this model format because it is well documented.
I know there are plenty of restrictions in using this format (material struct is very limited I think).
However: Koeky 3D does not care where your vertex,normal,texcoord etc. data comes from. So if you feel like it: you can write a loader for any model format you like! (good luck with that :P)

I turned this into a library because I will use it in some other examples as well. Feel free to use this in whatever scenario you like tough!

I reference the OpenTK library because I like to use Vector3 and Vector2 structs to keep things simple