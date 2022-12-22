# AxamlToCodeConvert
This program converts AXAML files into C# code that resembles Winforms' `InitializeComponent()` code.

With this way, you still can edit AXAML files and also don't have to use `FindControl<>()` too many times for each control.

NOTE: This program will **NOT** give you a proper conversion so you mostly have to manually edit the code after.

## Features

 - Convert Axaml to `Init()` code.
 - Support for custom controls.
 - Support for bindings.


## Build
Building requires [.NET SDK](https://dotnet.microsoft.com/en-us/download)

To build, get hte source code:
  - [Download ZIP](https://github.com/Haltroy/AxamlToCodeConvert/archive/refs/heads/main.zip) and extract it to a folder.
  - by using [GitHub Desktop](https://desktop.github.com/)
  - via command: `git clone https://haltroy.com/AxamlToCodeConvert`

Then, open a terminal app (CMD, Konsole, Windows Terminal, Powershell etc.) and navigate to the folder and execute any of these commands:
  - To run: `dotnet run`
  - To build: `dotnet build`
     - Executable will be in the `bin\Debug` folder.
  - To publish: `dotnet publish -c Release`
     - Executable will be in the `bin\Release` folder.


## Usage

1. Either [download the app](https://github.com/haltroy/AxamlToCodeConvert/releases) or build the app. Then execute it.
2. Load a AXAML file by either using `Open a File` option in top or pasting the AXAMl code into the `AXAML:` field.
3. It will automatically generate the `Init()` code in the `Init():` field.
4. Select and copy all of the generated text and head back to your code editor. Open the `.axaml.cs file.
5. Paste it somewhere inside the window/usercontrol class.
6. Fix the code.
7. In the `InitializeComponent()` void of your window/usercontrol, replace `AvaloniaXamlLoader.Load(this);` with `Init();` .
    - Alternatively, you can remove the `InitializeComponent()` void and replace `InitializeComponent();` in the constructor (`public YourWindow()`) with `Init();`.
8. Now you can access your controls from just `myControl.` instead of `FindControl<Its type here>("Name of it here").`
