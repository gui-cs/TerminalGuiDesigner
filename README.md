# Terminal Gui Designer

![example workflow](https://github.com/tznind/TerminalGuiDesigner/actions/workflows/build.yml/badge.svg)


Cross platform designer for [Terminal.Gui](https://github.com/migueldeicaza/gui.cs) applications.

This project is in pre-pre-alpha.  It currently supports:

- Generating .Designer.cs files with CodeDom
- Loading .Designer.cs files with Roslyn
- Undo/Redo
- Adding/Moving views within the designed Window

![designer](https://user-images.githubusercontent.com/31306100/161325121-c6c03350-5d37-4830-b756-58daf79c972f.gif)

Outstanding Features
-------------------------------

Features to add are:

- [ ] Design classes 
    - [x] Window
    - [x] Dialog
    - [ ] View
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
- [ ] Read and present xmldoc comments when editing properties
- [x] Comprehensive Tests
- [x] CI

Class Diagram
-------------------------------
![Terminal.Gui Class Diagram](./TerminalGuiDesigner.png)
