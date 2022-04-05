# Terminal Gui Designer

![example workflow](https://github.com/tznind/TerminalGuiDesigner/actions/workflows/build.yml/badge.svg) [![NuGet Badge](https://buildstats.info/nuget/TerminalGuiDesigner)](https://www.nuget.org/packages/TerminalGuiDesigner/)

Cross platform designer for [Terminal.Gui](https://github.com/migueldeicaza/gui.cs) applications.  

Built with CodeDom and Roslyn, TerminalGuiDesigner lets you create complicated Views with drag and drop just like the WinForms designer you know and love (or hate).

Install the tool via NuGet:
```
dotnet tool install --global TerminalGuiDesigner
```

This project is in pre-alpha.  See the feature list for progress.

![designer](https://user-images.githubusercontent.com/31306100/161325121-c6c03350-5d37-4830-b756-58daf79c972f.gif)

Building
----------------
Build using the dotnet 6.0 sdk
```
cd src
dotnet run
```

Keybindings
----------------
You can change keybindings by creating copying [Keys.yaml](https://raw.githubusercontent.com/tznind/TerminalGuiDesigner/main/src/Keys.yaml) into your current directory.

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
    - [ ] Color Schemes
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
- [ ] Create and edit all views
  - [x] Button
  - [x] Checkbox
  - [ ] ComboBox
  - [x] DateField
  - [x] FrameView
  - [x] GraphView
  - [x] HexView
  - [x] Label
  - [x] LineView
  - [x] ListView
  - [ ] MenuBar
  - [ ] PanelView
  - [x] ProgressBar
  - [x] RadioGroup
  - [ ] StatusBar
  - [x] TableView
  - [x] TabView
  - [x] TextField
  - [x] TextValidateField
  - [x] TextView
  - [x] TimeField
  - [ ] TreeView
  - [ ] View

Class Diagram
-------------------------------
![Terminal.Gui Class Diagram](./TerminalGuiDesigner.png)
