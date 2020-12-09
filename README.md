# DotNetVersion

The tool is used for some DEVOPS operations. Here is a simple list that applies to development process

- increment: manage versioning of a solution handling the project dependencies
- epoch: generate an epoch number
- number: generate a version number

## HOWTO: Install 

To install the tool you have to manually copy the folder in the program files directory and then add the folder to the PATH variable:

1) Install .NET 5.0 runtime as a prerequisite from https://dotnet.microsoft.com/download

2) Create a folder named "DotNetVersion" in the following path:

`
%PROGRAMFILES(X86)%\DotNetVersion
`

3) Copy the full content of the ZIP into the folder. You need to have administrator privileges. If you do not have admin rights please choose an alternate folder in an accessible directory then use it in the next steps. Be careful of copy files in the root without any other folder.

4) Check that this path exists: 

`
%PROGRAMFILES(X86)%\DotNetVersion\dotnetversion.exe
`

5) Open from control panel the "Environment Variables applet" and add "%PROGRAMFILES(X86)%\DotNetVersion" to the PATH system environment variable. (lower panel)

6) Save and close confirming the change

7) Open a cmd.exe or powershell.exe

8) Types "dotnetversion" and "ENTER". An help screen should show up. If this do not happen double check the previous steps

9) The tool is not installed and ready to be used.

## HOWTO: Upgrade version of a project in solution

This part starts assuming you have a solution where each project have been versioned using the `<Version>` or `<VersionPrefix>` tag. The pattern to use s the following:

1) Use "VersionPrefix" for projects that will be published ad Nuget packages
2) Use "Version" for projects published as executables or website. This usually happens for docker contained apps also.
3) DO NOT specify any "Version" or "VersionPrefix" for assemblies that contains tests and other artifacts that do not need to be published

### Steps to increase version number

Assume that we are publishing a project created in with this Hierarchy

- Example.Core.Web
- Example.Core.Worker
- Example.Core.Common
- Example.Core.Bus
- Example.Core.Bus.Kafka
- Example.Core.Bus.Rabbit
- Example.Core.Settings
- Example.Core.Logging
- Example.Core.Logging.Serilog
- Example.Core.sln <= SOLUTION FILE

Assume also thet we have made some changes to the "Example.Core.Common" library. This is a root assembly that is used by other projects. When we make any change to this assembly we need to version also dependent assemblies, also if they do not hanve any change.

Here is the pattern:

1) Enter the directory where the solution file is located
2) Enter the following command with the given casing 

``
dotnetversion increment -s .\Example.Core.sln -p Example.Core.Common -l Build
``

3) The tool will present a json output that lists the changed project 

4) Check that there is not any error in versioning

5) Enter again the previous command using the -c switch. It will apply the changes to the projects

``
dotnetversion increment -s .\Example.Core.sln -p Example.Core.Common -l Build -c
``

6) Now check the project in visual studio and you'll see that version has changes