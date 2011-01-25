Tested with Graphviz version 2.24,2.26.3

Install graphviz and include C:\Program Files\Graphviz\bin into PATH or just copy graphviz dll's into project output directory, required dll's:
cdt.dll
graph.dll
gvc.dll
gvcplugin_core.dll
gvcplugin_dot_layout.dll
libexpat.dll
ltdl.dll
Pathplan.dll
regex_win32.dll
and with dll's required configuration file "config6"(see config6.sample)

GraphVizNet is Visual C++ 2008 project, if you don't have installed Visual C++, use precompiled version of GraphVizNet.dll in GraphVizNet folder.
Add C:\Program Files\Graphviz\include\graphviz to include search path and C:\Program Files\Graphviz\lib\release\lib to library search path(Tools->Options->Projects and Solutions->VC++ Directories).