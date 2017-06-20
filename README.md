# SymLinker
---
This repository contains a cross-platform .NET Core application for creating symbolic links on a computer system.

### Use
---
Visit the Releases tab to download only the necessary deployables or download and build this repository. 

##### Your Code
Use SymLinker.Linker in your code to generate symbolic links. 

```
var linker = new Linker();
linker.CreateLink(sourceFilePath, destFilePath);
```

Subscribe to the ```OnError```, ```OnWarn```, and ```OnInfo``` events to capture SymLinker's built in error handling and warnings.

##### Windows
To run the sample application, open a command prompt and type:
```
dotnet run SymLinker.Console.dll 
```

**Currently untested on Linux and OSX**

### License
SymLinker is released under the MIT license and is free for use with proper attribution
