# BlendShapeHelper

**BlendShapeHelper** is a Script Collection for Unity and VSeeFace that simplifies the workflow for exporting models from VRoid Studio and splitting them into different clothing items, such as toggleable cat ears.


BlendShapeHelper creates blendshapes for your clothing toggles by utilizing the alpha channel of the clothing material. When you create a blendshape with this script, the information about which material was used for each clothing item will be stored in a new GameObject called BSH.


This feature allows you to update the model after creating it once. For example, if you make changes in VRoid Studio and want to keep the old clothing groups, you can simply import the model and override the old one (it's important to keep the paths the same!). In BSH, you can then execute "Update All Clothings," which will recreate your old clothing groups in the new model.

---
## Installation

To install BlendShapeHelper, follow these steps:

* Just copy the content of the Assets folder to your Unity Project. 
* Unity 2019.4.16f1 is the only tested one.
* You also need VSF SDK & UniVRM 0.08 installed.

---

## How to use

1. Import the model of your choice
2. In the Unity Editor Menu you find the BSH Menu. Open BSH -> Clothing
3. Select your model from the dropdown
4. Click "Create New Clothing"
5. Select the materials which should be contained in this toggle
6. Click "Save Clothing"
7. Profit!

Please note that this guide assumes you have basic knowledge of Unity and its interface.



