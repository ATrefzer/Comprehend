# Comprehend

Proof of concept: Generating sequence diagrams from profiler traces.

## Prerequisites
This tool uses the 3rd party component Plantuml to generate sequence diagrams. This is a java application. Threfore you need a Java Runtime installed and in your search path.

## Description

This application consists of two parts:
1. A minimum profiler to trace method calls in .NET applications. The profiler writes all called methods to a file.
2. A sample application that allows starting a .NET application with the profiler. This application also can load the profile and generate a sequence diagram via Plantuml.



## Usage



### 1. Collect traces

Start launcher.exe and run the application to be profiled.

Be aware that a large amount of profiling information is written to the hard disk. Starting a larger application can soon result in 5+ GB. Therefore you can decide when to start collecting data with the "Enable Profiling" checkbox. You can only defer profiling to a later time but you cannot disable it after that. And you cannot attach to a running application.

![image-20200125151207753](Images/image-20200125151207753.png)



### 2. Create a sequence diagram

Go to the tab "Create sequence diagram" and select the profile you collected.

![image-20200125151422908](Images/image-20200125151422908.png)

"Edit Pre-Filter" opens a text file that filters a lot of the default .NET namespaces you don't want to see in a sequence diagram. You can add further elements here.

Clicking on "Generate" opens a dialog that lets you define your start function for the sequence diagram. Use the context menu to select the function.



![image-20200125152251307](Images/image-20200125152251307.png)

By clicking OK a call tree explorer is openend that shows you all execution traces of the selected start function.

Here you can define you sequence diagram by checking which functions are included and which are not.

Use the context menu to generate the UML diagram. The call tree explorer is kept open. You you can refine the diagram and try again.

<img src="Images\2022-04-16 15_09_50.png" style="zoom: 67%;" />



## Notes and limitations

The result is shown in an svg viewer. The input plantuml file is written to the output directory.

![image-20200125153650798](Images/image-20200125153650798.png)

You can exclude functions in the call tree explorer. If you hide a call that results in a call you want to see the arrow is drawn in blue color. Meaning the method CycleA calls CycleA indirectly and hiding calls in between.

![image-20200125152916959](Images/image-20200125152916959.png)
