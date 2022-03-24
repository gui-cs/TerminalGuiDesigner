# Terminal Gui Designer

Cross platform designer for [Terminal.Gui](https://github.com/migueldeicaza/gui.cs) applications.

This project is in pre-pre-alpha.  It currently supports:

- Generating .Designer.cs files with CodeDom
- Loading .Designer.cs files with Roslyn
- Undo/Redo
- Adding/Moving views within the designed Window
 
![designer](https://user-images.githubusercontent.com/31306100/158055700-b5ff1848-ee2e-4a0e-9870-c9fbe83ce52f.gif)

Outstanding Features
-------------------------------

Features to add are:

- [ ] Design classes other than `Window` (e.g. View etc)
- [ ] Configure root properties of the class being designed (e.g. Window.Width)
- [ ] Configure and persist properties
    - [x] (Name)
    - [x] X/Y
    - [x] Width/Height
    - [x] Text
    - [ ] CanFocus
    - [ ] Color Schemes
    - [ ] Data Tables
    - [ ] TreeView
    - [x] Tab View
    - [ ] Build Top Bar Menus
- [ ] Create Events e.g. MyButton_OnClick
- [ ] Mutli select (select many views and hit delete or drag move)
- [ ] Copy/Paste selected view(s)
- [ ] Add views to subviews
- [ ] Comprehensive Tests
- [x] CI

Class Diagram
-------------------------------
![Terminal.Gui Class Diagram](./TerminalGuiDesigner.png)
