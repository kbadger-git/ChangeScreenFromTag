# ChangeScreenFromTag
This example shows how to change the displayed page based on a value coming from the PLC or a Model variable
## Usage
1. Create as many instances of MyBaseScreen as desired into the Screens folder (or subfolder)
2. Configure an unique ID for each instance (must be greater than zero)
3. Execute project
## Testing
1. Change the value of the tag using the SpinBox, if value is bigger than zero and a page with that ID exists, the PanelLoader is refreshed
