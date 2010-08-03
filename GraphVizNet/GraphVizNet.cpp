// This is the main DLL file.

#include "stdafx.h"
#include <gvc.h>

#include "GraphVizNet.h"
#include <msclr\marshal.h>

using namespace System::Collections;
using namespace System::Runtime::InteropServices;
using namespace msclr::interop;

namespace GraphVizNet {

	GraphEngine::GraphEngine(void)
	{
		if(gvc == 0)
		{
			gvc = gvContext();
		}
		//context = gcnew marshal_context();
	}
	
	GraphEngine::~GraphEngine()
	{
		this->!GraphEngine();
	}

	GraphEngine::!GraphEngine()
	{
		//int r = gvFreeContext(gvc);
	}

	void GraphEngine::SetAttributes(void *o, VizBaseEntity^ entity)
	{
		IEnumerator^ e = entity->SourceAttributes->Keys->GetEnumerator();
		while(e->MoveNext())
		{
			String^ name = (String^)e->Current;
			String^ value = entity->SourceAttributes[name];

			char* heapName = (char*)Marshal::StringToHGlobalAnsi(name).ToPointer();
			char* heapValue = (char*)Marshal::StringToHGlobalAnsi(value).ToPointer();

			agsafeset(o, heapName, heapValue, 0);

			Marshal::FreeHGlobal((IntPtr)heapName);
			Marshal::FreeHGlobal((IntPtr)heapValue);
		}
	}

	Agedge_t* GraphEngine::CreateEdge(graph_t* g, VizEdge^ inEdge)
	{
		Agnode_t* tailNode = this->FindNode(g, inEdge->Tail);
		Agnode_t* headNode = this->FindNode(g, inEdge->Head);

		Agedge_t* edge = agedge(g, tailNode, headNode);
		SetAttributes(edge, inEdge);
		return edge;
	}

	Agnode_t* GraphEngine::CreateNode(graph_t* g, VizNode^ inNode)
	{
		char* name = (char*)Marshal::StringToHGlobalAnsi(inNode->Name).ToPointer();
		Agnode_t* node = agnode(g, name);
		Marshal::FreeHGlobal((IntPtr)name);

		SetAttributes(node, inNode);
		return node;
	}

	Agraph_t* GraphEngine::CreateGraph(VizGraph^ inGraph)
	{
		int type = -1;

		switch(inGraph->Type)
		{
			case VizGraphType::StrictGraph:
				type = AGRAPHSTRICT;
				break;
			case VizGraphType::StrictDiGraph:
				type = AGDIGRAPHSTRICT;
				break;
			case VizGraphType::Graph:
				type = AGRAPH;
				break;
			case VizGraphType::DiGraph:
				type = AGDIGRAPH;
				break;
		}

		graph_t *g;
		if(!String::IsNullOrEmpty(inGraph->Name))
		{
			char* graphName = (char*)context->marshal_as<const char*>(inGraph->Name);
			g = agopen(graphName, type);
		}
		else {
			g = agopen("g", type);
		}

		SetAttributes(g, inGraph);
		return g;
	}

	void GraphEngine::FillAttributes(void *o, VizBaseEntity^ entity)
	{
		Agsym_t *atr = agfstattr(o);
		if(atr != 0)
		{
			do
			{
				char *value = agxget(o, atr->index);
				entity->SetAttribute(gcnew String(atr->name), gcnew String(value));
			}
			while(atr = agnxtattr(o, atr));
		}
	}

	void GraphEngine::FillGraph(graph_t* g, VizGraph^ inGraph)
	{
		FillAttributes(g, inGraph);
	}

	void GraphEngine::FillNode(Agnode_t* node, VizNode^ inNode)
	{
		FillAttributes(node, inNode);
	}

	void GraphEngine::FillEdge(Agedge_t* edge, VizEdge^ inEdge)
	{
		FillAttributes(edge, inEdge);
	}

	Agnode_t* GraphEngine::FindNode(graph_t* g, VizNode^ inNode)
	{
		char* name = (char*)Marshal::StringToHGlobalAnsi(inNode->Name).ToPointer();
		Agnode_t* node = agfindnode(g, name);
		Marshal::FreeHGlobal((IntPtr)name);
		return node;
	}

	Agedge_t* GraphEngine::FindEdge(graph_t* g, VizEdge^ inEdge)
	{
		Agnode_t* tailNode = this->FindNode(g, inEdge->Tail);
		Agnode_t* headNode = this->FindNode(g, inEdge->Head);

		return agfindedge(g, tailNode, headNode);
	}

	void GraphEngine::DotLayout(VizGraph^ inGraph)
	{
		graph_t *g = CreateGraph(inGraph);
		try {
			for(int i=0; i < inGraph->Nodes->Count;i++)
			{
				this->CreateNode(g, inGraph->Nodes[i]);
			}

			for(int i=0; i < inGraph->Edges->Count;i++)
			{
				this->CreateEdge(g, inGraph->Edges[i]);
			}

			//char* args[3];
			//args[0] = (char*)Marshal::StringToHGlobalAnsi("dot").ToPointer();
			//args[1] = (char*)Marshal::StringToHGlobalAnsi("-Tdot").ToPointer();
			//args[2] = (char*)Marshal::StringToHGlobalAnsi("-odebug.dot").ToPointer();

			//gvParseArgs(gvc, sizeof(args)/sizeof(char*), args);

			//int layoutRes = gvLayoutJobs(gvc, g);
			//int renderRes = gvRenderJobs(gvc, g);
			
			try {
				int layoutRes = gvLayout(gvc, g, "dot");
				//attach_attrs(g);

				int renderRes = gvRender(gvc, g, "dot", 0);

				this->FillGraph(g, inGraph);

				for(int i=0; i < inGraph->Nodes->Count;i++)
				{
					VizNode^ inNode = inGraph->Nodes[i];
					Agnode_t* node = this->FindNode(g, inNode);
					this->FillNode(node, inNode);
				}

				for(int i=0; i < inGraph->Edges->Count;i++)
				{
					VizEdge^ inEdge = inGraph->Edges[i];
					Agedge_t* edge = this->FindEdge(g, inEdge);
					this->FillEdge(edge, inEdge);
				}
			}
			finally {
				gvFreeLayout(gvc, g);
			}
		}
		finally {
			agclose(g);
		}
		return;
	}
}