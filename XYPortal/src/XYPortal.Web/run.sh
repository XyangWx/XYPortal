#!/bin/bash
cd ../../../XYPortal.PasswordBook || exit 1
dotnet build
cd ../XYPortal.LinkBoard || exit 2
dotnet build
cd ../XYPortal.RandomStringProvider || exit 3
dotnet build
cd ../XYPortal/src/XYPortal.Web || exit 4
dotnet run