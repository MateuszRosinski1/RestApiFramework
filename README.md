# RestApiFramework 
📌 Overview 
This repository contains four interconnected projects designed to provide a lightweight framework for handling HTTP requests in a .NET environment. 

🧩 Projects 

1 RestApiFramework
  - A console application that handles HTTP requests and invokes endpoints based on the requested URL. It provides custom attribiutes as Controller, Route, FromRoute, FromBody. And resolve the URL param ?param=123 to match function parameters. Also provide a simple Dependency Injection Mechanism for injecting services in controller constructor, those services are there in 3 states, Transient, Scoped and Singleton just like in ASP.

2 RestApp
  - A project that references RestApiFramework. It exposes its functionality in a manner similar to ASP.NET Web API, making it accessible for users. 

3 RestAPIAnalyzer
  - A Roslyn Analyzer that provides compile-time warnings in Visual Studio when {param} placeholders in RouteAttribute do not match any actual method parameters.

4 RouteHighlightExtension 
  - A Visual Studio Extension (VSIX) that highlights route parameters (e.g., {param}) in Route Attribiute (works only if csproj contains a property IsRestApp set on true), improving visibility.

# Note 
There is only one commit because this project was developed in an    environment where I had no access to a private GitHub repository. The code was exported from a work device to a private device and then uploaded to GitHub.
