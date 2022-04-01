# Terminal Gui Designer

![example workflow](https://github.com/tznind/TerminalGuiDesigner/actions/workflows/build.yml/badge.svg)

Cross platform designer for [Terminal.Gui](https://github.com/migueldeicaza/gui.cs) applications.  

Built with CodeDom and Roslyn, TerminalGuiDesigner lets you create complicated Views with drag and drop just like the WinForms designer you know and love (or hate).

This project is in pre-alpha.  See the feature list for progress.

![designer](https://user-images.githubusercontent.com/31306100/161325121-c6c03350-5d37-4830-b756-58daf79c972f.gif)

Features
-------------------------------

The following feature list shows the current capabilities and the roadmap

- [ ] Design classes 
    - [x] Window
    - [x] Dialog
    - [ ] View
    - [ ] Top level (with statusbar and or menu)
- [x] Configure root properties (e.g. Window.Width, Title etc)
- [ ] Configure subview properties
    - [x] (Name)
    - [x] X/Y
    - [x] Width/Height
    - [x] Text
    - [ ] CanFocus
    - [ ] Color Schemes
    - [x] Data Tables
    - [ ] TreeView
    - [x] Tab View
    - [ ] Build Top Bar Menus
- [x] Mouse Input
  - [x] Drag to move
  - [x] Drag to resize
- [x] Undo/Redo
- [x] Direct editing of Text just by typing
- [ ] Easy Menu Bar Designing
- [ ] Easy Status Bar Designing
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
