# Comprehend

Proof of concept: Generating call graphs from profiler traces.

## Prerequisites
This tool uses the 3rd party component Plantuml to generate sequence diagrams.
Download plantuml.jar from https://plantuml.com/de/download and copy it to Binaries\Dependencies

## Description

This application consists of two parts:
1. A minimum profiler to trace method calls in .NET applications. The profiler writes all called methods to a file.
2. A sample application that allows starting a .NET application with the profiler. This application also can load the profile and generate a sequence diagram via Plantuml.

