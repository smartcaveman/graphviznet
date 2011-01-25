// GraphVizNet.h

#pragma once

#include <gvc.h>
#include <msclr\marshal.h>

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace msclr::interop;

namespace GraphVizNet {

	public ref class GraphEngine
	{
	private:
		static System::Object^ lockObject;

		static GVC_t *gvc;
		marshal_context ^ context;

		void SetAttributes(void *o, VizBaseEntity^ entity);

		Agraph_t* CreateGraph(VizGraph^ inGraph);
		Agedge_t* CreateEdge(graph_t* g, VizEdge^ inEdge);
		Agnode_t* CreateNode(graph_t* g, VizNode^ inNode);

		void FillAttributes(void * o, VizBaseEntity^ entity);

		void FillGraph(graph_t* g, VizGraph^ inGraph);
		void FillNode(Agnode_t* node, VizNode^ inNode);
		void FillEdge(Agedge_t* edge, VizEdge^ inEdge);

		Agnode_t* FindNode(graph_t* g, VizNode^ inNode);
		Agedge_t* FindEdge(graph_t* g, VizEdge^ inEdge);

	public:

		static GraphEngine(void);
		GraphEngine(void);

		void GraphEngine::DotLayout(VizGraph^ g);

		~GraphEngine();
		!GraphEngine();
	};
}
