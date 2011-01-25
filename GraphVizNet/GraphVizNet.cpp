// This is the main DLL file.

#include "stdafx.h"
#include <gvc.h>

#include "GraphVizNet.h"
#include <msclr\marshal.h>
#include <msclr\lock.h>

using namespace System::Collections;
using namespace System::Runtime::InteropServices;
using namespace msclr::interop;

namespace GraphVizNet {

	static GraphEngine::GraphEngine(void)
	{
		lockObject = gcnew System::Object();
		gvc = gvContext();
	}

	GraphEngine::GraphEngine(void)
	{
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
		if(System::String::IsNullOrEmpty(inNode->Name))
		{
			throw gcnew System::Exception("Node name must be non empty");
		}
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

		char* edgeKey = (char*)Marshal::StringToHGlobalAnsi(inEdge->Id).ToPointer();
		try {
			Agedge_t* edge;
			for(edge = agfstedge(g, headNode);
				edge;
				edge = agnxtedge(g, edge, headNode))
			{
				Agsym_t* keyAtr = agfindattr(edge, "id");
				if(keyAtr)
				{
					char* key = agxget(edge, keyAtr->index);
					if(strcmp(key, edgeKey) == 0)
					{
						return edge;
					}
				}
			}
			return 0;
		}
		finally {
			Marshal::FreeHGlobal((IntPtr)edgeKey);
		}
	}

	void GraphEngine::DotLayout(VizGraph^ inGraph)
	{
		// Многопоточность не наш конек
		msclr::lock lk(lockObject);

		graph_t *g = CreateGraph(inGraph);
		try {
			IEnumerator^ nodeE = inGraph->Nodes->GetEnumerator();
			try {
				while(nodeE->MoveNext())
				{
					VizNode^ inNode = (VizNode^)nodeE->Current;
					this->CreateNode(g, inNode);
				}
			}
			finally
			{
				delete nodeE;
			}

			IEnumerator^ edgeE = inGraph->Edges->GetEnumerator();
			try {
				while(edgeE->MoveNext())
				{
					VizEdge^ inEdge = (VizEdge^)edgeE->Current;
					this->CreateEdge(g, inEdge);
				}
			}
			finally
			{
				delete edgeE;
			}
			
			try {
				int layoutRes = gvLayout(gvc, g, "dot");

				int renderRes = gvRender(gvc, g, "dot", 0);

				this->FillGraph(g, inGraph);

				IEnumerator^ nodeE = inGraph->Nodes->GetEnumerator();
				try {
					while(nodeE->MoveNext())
					{
						VizNode^ inNode = (VizNode^)nodeE->Current;
						Agnode_t* node = this->FindNode(g, inNode);
						this->FillNode(node, inNode);
					}
				}
				finally
				{
					delete nodeE;
				}

				IEnumerator^ edgeE = inGraph->Edges->GetEnumerator();
				try {
					while(edgeE->MoveNext())
					{
						VizEdge^ inEdge = (VizEdge^)edgeE->Current;
						Agedge_t* edge = this->FindEdge(g, inEdge);
						this->FillEdge(edge, inEdge);
					}
				}
				finally
				{
					delete edgeE;
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