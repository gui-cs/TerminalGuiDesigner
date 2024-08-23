# Terminal Gui Designer

![example workflow](https://github.com/tznind/TerminalGuiDesigner/actions/workflows/build.yml/badge.svg) [![NuGet Badge](https://buildstats.info/nuget/TerminalGuiDesigner)](https://www.nuget.org/packages/TerminalGuiDesigner/)
![Code Coverage](https://img.shields.io/endpoint?url=https://gist.githubusercontent.com/tznind/2a31eee1c9151917aa8d17b59bc86633/raw/code-coverage.json)

Cross platform designer for [Terminal.Gui](https://github.com/migueldeicaza/gui.cs) applications.  

Built with CodeDom and Roslyn, TerminalGuiDesigner lets you create complicated Views with drag and drop just like the WinForms designer you know and love (or hate).

Install the tool from NuGet or follow the [Hello World Tutorial](./README.md#usage):
```
dotnet tool install --global TerminalGuiDesigner
```

Update to the latest version using
```
dotnet tool update --global TerminalGuiDesigner
```
This project is in alpha.  See the [feature list](./README.md#features) for progress.

## V2 

If you are targetting Terminal.Gui version 2 (currently pre-alpha) then you will want to install version 2 of the designer
```
dotnet tool install --global TerminalGuiDesigner --prerelease
```
Ensure that you match the Terminal.Gui library version you reference to the Designer version.

## Demo

![long-demo](https://github.com/gui-cs/TerminalGuiDesigner/assets/31306100/5df9f545-8c61-4655-bc0c-1e75d1c149d9)

### Building
----------------
Build using the dotnet 6.0 sdk
```
cd src
dotnet run
```

### Usage
------------------
Install the dotnet sdk and create a new console application with references to Terminal.Gui.  Install the TerminalGuiDesigner and create a new dialog:
```
dotnet new console -n hello
cd hello
dotnet add package Terminal.Gui
dotnet tool install --global TerminalGuiDesigner
TerminalGuiDesigner MyDialog.cs
```

Enter a namespace then add a Button to the view.  Save with Ctrl+S.  Exit the designer with Ctrl+Q.

Enter the following into Program.cs

```csharp
using Terminal.Gui;

Application.Init();

Application.Run(new YourNamespace.MyDialog());

Application.Shutdown();
```

Run your program with 
`dotnet run`

You can add new code to `MyDialog.cs` but avoid making any changes to `MyDialog.Designer.cs` as this will be rewritten when saving.

For example in `MyDialog.cs` after `InitializeComponent()` add the following:

```csharp
button1.Accept += ()=>MessageBox.Query("Hello","Hello World","Ok");
```
Now when run clicking the button will trigger a message box.

![msgbox](https://user-images.githubusercontent.com/31306100/168493639-c7230505-0215-45e3-90a7-b7c24934a8fa.jpg)

You can re-open the designer by running it from the command line with the file you want to edit/create.

```
TerminalGuiDesigner MyDialog.cs
```

You can remove the tool using the following:

```
dotnet tool uninstall --global TerminalGuiDesigner
```

### Troubleshooting 
If when running the tool you have issues seeing the colors add the `--usc` flag:
```
TerminalGuiDesigner --usc
```

The designer is built to be robust and has top level catch blocks but if you are still able to crash it you may find your console blocking input.  If this happens you should be able to fix your console by typing `reset<enter>` but also :heart: [please report it](https://github.com/tznind/TerminalGuiDesigner/issues/new) :heart:

### Keybindings & Controls
----------------
You can change keybindings by copying [Keys.yaml](https://raw.githubusercontent.com/tznind/TerminalGuiDesigner/main/src/Keys.yaml) into your current directory.

To edit MenuBar items use the following controls

| Key          |  Action |
|--------------|------------|
| Shift Up/Down | Move selected menu item up/down|
| Shift Right   | Move selected menu item to a submenu of the one above |
| Shift Left    | Move selected sub menu item up a level |
| Del    | Remove selected menu item |
| Enter    | Add a new menu item |
| Typing    | Edit the Title of the selected item |
| Ctrl + T | Set Shortcut |
| Ctrl + R | Set menu field name |

New root level menus can be added by right clicking the `MenuBar` and selecting 'Add Menu'.

You can create a menu separator by typing `---`

### Features
-------------------------------

The following feature list shows the current capabilities and the roadmap.  Features in
italics are experimental and require passing the `-e` flag when starting application.

- [x] Design classes 
    - [x] Window
    - [x] Dialog
    - [x] View
    - [x] Top level
- [x] Configure root properties (e.g. Window.Width, Title etc)
- [x] Configure subview properties
    - [x] (Name)
    - [x] X/Y
    - [x] Width/Height
    - [x] Text
    - [x] Color Schemes
- [x] Edit multiple Views' property at once (e.g. select 3 views and set all Width to 10)
- [x] Mouse Input
  - [x] Drag to move
  - [x] Drag into/out of sub view
  - [x] Drag to resize
- [x] Undo/Redo
- [x] Direct editing of Text just by typing
- [x] Easy Menu Bar Designing
  - [x] Create new items
  - [x] Move items
  - [x] Move in/out of submenus
  - [x] Add menu bar separators (Type '---')
  - [x] Assign shortcuts
  - [x] Set fieldnames `(Name)`
- [x] Easy Status Bar Designing
- [ ] Create Events e.g. MyButton_OnClick
- [x] View Dependency Management
  - [x] Prevent deleting views where other Views hold RelativeTo dependencies
  - [x] Write out dependant views to `InitializeComponent` before dependers
  - [x] Order Multi Delete operations to delete dependers before dependants
- [x] Mutli select
  - [x] Multi delete
  - [x] Multi keyboard move
  - [x] Multi mouse drag move
  - [x] Multi set Property
  - [x] Multi Copy/Paste
- [x] Copy/Paste 
  - [x] Single simple views
  - [ ] Cut
  - [x] Container views (e.g. TabView)
  - [ ] To OS clipboard (e.g. open one Designer.cs View and copy to another)
  - [x] Retain PosRelative mappings in pasted views (e.g. `A` LeftOf `B`)
- [x] Move views to subviews
  - [x] With mouse
  - [ ] With keyboard
- [ ] Read and present xmldoc comments when editing properties
- [ ] Dev Environment Integration
  - [ ] Visual Studio plugin (e.g. right click a .Designer.cs to open in TerminalGuiDesigner console)
  - [ ] Visual Studio Code plugin 
- [x] Comprehensive Tests
- [x] CI
- [ ] Ability to lock some of the views (prevent changes).  This prevents accidentally dragging a given control
- [ ] Support adding user defined `View` Types e.g. `MyCustomView`
- [ ] Corner Cases
  - [ ] Allow designing `abstract` classes
  - [ ] Allow designing generic classes (e.g. `MyDialog<T>`)
  - [ ] Allow designing classes that inherit from another e.g. `class MyDialog : MyOtherDialog`
     - [ ] Inherited views should be locked to prevent editing
- [x] Create and edit views
  - [x] Button
  - [x] Checkbox
  - [x] ComboBox
  - [x] DateField
  - [x] FrameView
  - [x] GraphView
  - [x] HexView
  - [x] Label
  - [x] LineView
  - [x] ListView
  - [x] MenuBar
    - [ ] Copy/Paste preserve menu entries
  - [x] ProgressBar
  - [x] RadioGroup
  - [ ] [SplitContainer](https://github.com/gui-cs/Terminal.Gui/pull/2258) (Unreleased)
    - [ ] Copy/Paste preserve split content panels
  - [x] StatusBar
    - [ ] Copy/Paste preserve menu entries
  - [x] TableView
    - [x] Add/Remove Columns
    - [x] Copy/Paste preserve table schema
  - [x] TabView
    - [x] Add/Remove new Tabs
    - [x] Reorder Tabs
    - [x] Copy/Paste preserve tabs/content
  - [x] TextField
  - [x] TextValidateField
  - [x] TextView
  - [x] TimeField
  - [x] TreeView
  - [x] View
  - [x] Slider

### Class Diagram
-------------------------------
![Terminal.Gui Class Diagram](./TerminalGuiDesigner.png)
