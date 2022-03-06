# Terminal Gui Designer

Cross platform designer for [Terminal.Gui](https://github.com/migueldeicaza/gui.cs) applications.

This project is in pre-pre-alpha.  It currently supports:

- Generating .Designer.cs files with CodeDom
- Loading .Designer.cs files with Roslyn
- Undo/Redo
- Adding/Moving views within the designed Window


![designer](https://user-images.githubusercontent.com/31306100/156942451-966f5bb6-a53d-450f-92fc-dae3ee1355f4.gif)

Outstanding Features
-------------------------------

Features to add are:

- [ ] Design classes other than `Window` (e.g. View etc)
- [ ] Configure and persist properties
    - [x] (Name)
    - [x] X/Y
    - [x] Width/Height
    - [x] Text
    - [ ] CanFocus
    - [ ] Color Schemes
    - [ ] Data Tables
    - [ ] TreeView
    - [ ] Tab View
- [ ] Add views to subviews
- [ ] Comprehensive Tests
- [ ] CI

Class Diagram
-------------------------------
![Terminal.Gui Class Diagram](./TerminalGuiDesigner.png)
