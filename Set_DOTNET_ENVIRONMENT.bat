@ECHO OFF

REM Set the environment variable DOTNET_ENVIRONMENT to a stage value
REM /m sets the variable on system level, default is user level
REM SETX <variableName> <Value> [<Params>]

SETX DOTNET_ENVIRONMENT Development /m