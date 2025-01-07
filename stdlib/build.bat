@echo off
rem Standard library name
set LIB_NAME=rstd.lib

rem Step 1: Compile all .rhm files into .o files
echo Compiling .rhm files to .o files...
for %%f in (*.rhm) do (
    echo Compiling %%f...
    clang -c %%f -o %%~nf.o
)

rem Step 2: Create a .lib file from all .o files
echo Creating %LIB_NAME%...
llvm-ar rcs %LIB_NAME% *.o

echo Build complete!