# AxamlToCodeConvert
This program converts AXAML files into C# code that resembles Winforms' `InitializeComponent()` code.

With this way, you still can edit AXAML files and also don't have to use `FindControl<>()` too many times for each control.

NOTE: This program will **NOT** give you a proper conversion so you mostly have to manually edit the code after.

## Features

 - Convert Axaml to `Init()` code.
 - Support for custom controls (the ones that do provide you with a dictonary file).
 - View code (thanks for [`AvalonEdit`](https://github.com/AvaloniaUI/AvaloniaEdit) for syntax highligting)
 - Convert multiple AXAMLs (thanks to [`FluentAvalonia`](https://github.com/amwx/FluentAvalonia) for tab control)


## Build
Building requires [.NET SDK](https://dotnet.microsoft.com/en-us/download)

To build, get hte source code:
  - [Download ZIP](https://github.com/Haltroy/AxamlToCodeConvert/archive/refs/heads/main.zip) and extract it to a folder.
  - by using [GitHub Desktop](https://desktop.github.com/)
  - via command: `git clone https://haltroy.com/AxamlToCodeConvert`

Then, open a terminal app (CMD, Konsole, Terminal, Powershell etc.) and navigate to the folder and execute any of these commands:
  - To run: `dotnet run`
  - To build: `dotnet build`
     - Executable will be in the `bin\Debug` folder.
  - To publish: `dotnet publish -c Release`
     - Executable will be in the `bin\Release` folder.


## Usage

1. Either [download the app](https://github.com/haltroy/AxamlToCodeConvert/releases) or build the app. Then execute it.
2. Load a AXAML file by either using `File -> Open a File` option or by pressing the `+` in top.
3. It will automatically generate the `Init()` code like a `Designer.cs` file in the `Designer.cs:` field.
4. Save it to the location of AXAML file. Open the `.axaml.cs file.
5. Fix the code.
6. Remove the constructor from your class.
7. Now you can access your controls from just `myControl.` instead of `FindControl<Its type here>("Name of it here").`
