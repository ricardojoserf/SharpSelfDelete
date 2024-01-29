# SharpSelfDelete

PoC to self-delete a binary in C#. 

It uses the APIs [GetModuleFileName](https://learn.microsoft.com/en-us/windows/win32/devnotes/-getmodulefilename), [CreateFileW](https://learn.microsoft.com/en-us/windows/win32/api/fileapi/nf-fileapi-createfilew) and [SetFileInformationByHandle](https://learn.microsoft.com/en-us/windows/win32/api/fileapi/nf-fileapi-setfileinformationbyhandle) to rename the alternate data stream $DATA from the binary to a random new one and then delete the file.

Screenshot:

![img](https://raw.githubusercontent.com/ricardojoserf/ricardojoserf.github.io/master/images/sharpselfdelete/Screenshot_1.png)


### Source

This is a port from code in one lesson of [Maldev Academy](https://maldevacademy.com/), which was written in C 
