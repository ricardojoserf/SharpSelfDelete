# SharpSelfDelete

This is a PoC code to self-delete a binary in C#. It is specially useful for malware as under normal conditions it is not possible to delete a binary on Windows while it is running. In my case I needed it for the [SharpCovertTube](https://github.com/ricardojoserf/SharpCovertTube) project, so the binary can delete itself from disk.

It uses the APIs [GetModuleFileName](https://learn.microsoft.com/en-us/windows/win32/devnotes/-getmodulefilename), [CreateFileW](https://learn.microsoft.com/en-us/windows/win32/api/fileapi/nf-fileapi-createfilew) and [SetFileInformationByHandle](https://learn.microsoft.com/en-us/windows/win32/api/fileapi/nf-fileapi-setfileinformationbyhandle) to rename the Alternate Data Stream $DATA (the default one) in the binary to a random new one and then delete the file.

![img](https://raw.githubusercontent.com/ricardojoserf/ricardojoserf.github.io/master/images/sharpselfdelete/Screenshot_1.png)
-------------------------------------------

### Source

This is a port from the code in a lesson of [Maldev Academy](https://maldevacademy.com/), which was originally written in C.
