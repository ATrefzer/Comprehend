#include "pch.h"
#include "Profiler.h"
#include <corhlpr.h>
#include <corprof.h>
#include <string>
#include <unordered_map>
#include "Common/TextFileWriter.h"
#include "ProfilerApi.h"
#include "CallGraphExporter.h"
#include "Common/BinaryWriter.h"
#include "Common\Encodings.h"
#include "Callbacks.h"

// http://www.blong.com/conferences/dcon2003/internals/profiling.htm

// https://github.com/microsoft/clr-samples/blob/master/ProfilingAPI/ELTProfiler/CorProfiler.cpp

// https://github.com/maybattle/Profiler1/blob/master/Profiler1/Profiler.cpp

// https://github.com/OpenCover/opencover/blob/master/main/OpenCover.Profiler/CodeCoverage_ProfilerInfo.cpp

// http://read.pudn.com/downloads64/sourcecode/windows/system/228104/leave_x86.cpp__.htm



UINT_PTR __stdcall FunctionIDMapperFunc(FunctionID funcId, void* clientData, BOOL* pbHookFunction)
{
	// This is called only once the funcId is created.
	// Same function have arrive with different ids.
	_callTrace->AddFunctionInfo(funcId);
	return funcId;
}


#ifndef WIN32
#else
#define PROFILER_CALLTYPE EXTERN_C void STDMETHODCALLTYPE
#endif


#ifndef _WIN64


void  __stdcall  EnterNakedFunc(FunctionIDOrClientID functionIDOrClientID, COR_PRF_ELT_INFO eltInfo);
void  __stdcall  LeaveNakedFunc(FunctionIDOrClientID functionIDOrClientID, COR_PRF_ELT_INFO eltInfo);
void  __stdcall  TailCallNakedFunc(FunctionIDOrClientID functionIDOrClientID, COR_PRF_ELT_INFO eltInfo);


void  __stdcall  EnterNakedFuncOld(FunctionIDOrClientID functionIDOrClientID, COR_PRF_ELT_INFO eltInfo);
void  __stdcall  LeaveNakedFuncOld(FunctionIDOrClientID functionIDOrClientID, COR_PRF_ELT_INFO eltInfo);
void  __stdcall  TailCallNakedFuncOld(FunctionIDOrClientID functionIDOrClientID, COR_PRF_ELT_INFO eltInfo);


#endif


Profiler::Profiler() : _referenceCounter(0)
{
    _writer = nullptr;
    _api = nullptr;
}

Profiler::~Profiler()
{
	if (_callTrace != nullptr)
	{
		_callTrace->Release();
		_callTrace = nullptr;
	}
}

HRESULT STDMETHODCALLTYPE Profiler::Initialize(IUnknown* pICorProfilerInfo)
{
  
   /* FunctionIDOrClientID id;
    id.functionID = 3;
    COR_PRF_ELT_INFO info = 6;
    EnterNakedFunc(id, info);*/

	/// Passed in from CLR when Initialize is called
	ICorProfilerInfo8* corProfilerInfo;

	HRESULT result = pICorProfilerInfo->QueryInterface(__uuidof(ICorProfilerInfo8),
	                                                   reinterpret_cast<void**>(&corProfilerInfo));

	if (FAILED(result))
	{
		return E_FAIL;
	}

	DWORD eventMask = COR_PRF_MONITOR_ENTERLEAVE | COR_PRF_ENABLE_FUNCTION_ARGS | COR_PRF_ENABLE_FUNCTION_RETVAL |
		COR_PRF_ENABLE_FRAME_INFO | COR_PRF_MONITOR_THREADS;

	_api = new ProfilerApi(corProfilerInfo);

	auto stream = new CppEssentials::OutputFileStream(4 * 1024 * 1024);
	stream->Open(L"d:\\output.bin", CppEssentials::CreateNew);
	_writer = new CppEssentials::BinaryWriter(stream, true);


	// No Ownership
	_callTrace = new CallGraphExporter(_api, _writer);

	corProfilerInfo->SetFunctionIDMapper2(FunctionIDMapperFunc, nullptr);

	auto hr = corProfilerInfo->SetEventMask(eventMask);
	if (hr != S_OK)
	{
		printf("ERROR: Profiler SetEventMask failed (HRESULT: %d)", hr);
	}

 	//hr = corProfilerInfo->SetEnterLeaveFunctionHooks3WithInfo(EnterNakedFuncOld, LeaveNakedFuncOld, TailCallNakedFuncOld);
	hr = corProfilerInfo->SetEnterLeaveFunctionHooks3WithInfo(EnterNakedFunc, LeaveNakedFunc, TailCallNakedFunc);
	

	if (hr != S_OK)
	{
		printf("ERROR: Profiler SetEnterLeaveFunctionHooks3WithInfo failed (HRESULT: %d)", hr);
	}

	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::Shutdown()
{
    ::OutputDebugString(L"\nProfiler::Shutdown");

    CppEssentials::TextFileWriter writer;
    writer.Open(L"d:\\index.txt", CppEssentials::FileOpenMode::CreateNew, CppEssentials::UTF16LittleEndianEncoder());
    _callTrace->WriteIndexFile(writer);
    writer.Close();

    ::OutputDebugString(L"\nIndex written");

	if (_callTrace != nullptr)
	{
		_callTrace->Release();
		_callTrace = nullptr;
	}

	if (_writer != nullptr)
	{
		delete _writer;
		_writer = nullptr;
	}

	if (_api != nullptr)
	{
		_api->Release();
		delete _api;
		_api = nullptr;
	}

	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::AppDomainCreationStarted(AppDomainID appDomainId)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::AppDomainCreationFinished(AppDomainID appDomainId, HRESULT hrStatus)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::AppDomainShutdownStarted(AppDomainID appDomainId)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::AppDomainShutdownFinished(AppDomainID appDomainId, HRESULT hrStatus)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::AssemblyLoadStarted(AssemblyID assemblyId)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::AssemblyLoadFinished(AssemblyID assemblyId, HRESULT hrStatus)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::AssemblyUnloadStarted(AssemblyID assemblyId)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::AssemblyUnloadFinished(AssemblyID assemblyId, HRESULT hrStatus)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ModuleLoadStarted(ModuleID moduleId)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ModuleLoadFinished(ModuleID moduleId, HRESULT hrStatus)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ModuleUnloadStarted(ModuleID moduleId)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ModuleUnloadFinished(ModuleID moduleId, HRESULT hrStatus)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ModuleAttachedToAssembly(ModuleID moduleId, AssemblyID AssemblyId)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ClassLoadStarted(ClassID classId)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ClassLoadFinished(ClassID classId, HRESULT hrStatus)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ClassUnloadStarted(ClassID classId)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ClassUnloadFinished(ClassID classId, HRESULT hrStatus)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::FunctionUnloadStarted(FunctionID functionId)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::JITCompilationStarted(FunctionID functionId, BOOL fIsSafeToBlock)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::JITCompilationFinished(FunctionID functionId, HRESULT hrStatus, BOOL fIsSafeToBlock)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::JITCachedFunctionSearchStarted(FunctionID functionId, BOOL* pbUseCachedFunction)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::JITCachedFunctionSearchFinished(FunctionID functionId, COR_PRF_JIT_CACHE result)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::JITFunctionPitched(FunctionID functionId)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::JITInlining(FunctionID callerId, FunctionID calleeId, BOOL* pfShouldInline)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ThreadCreated(ThreadID threadId)
{
	_callTrace->OnThreadCreated(threadId);
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ThreadDestroyed(ThreadID threadId)
{
	_callTrace->OnThreadDestroyed(threadId);
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ThreadAssignedToOSThread(ThreadID managedThreadId, DWORD osThreadId)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::RemotingClientInvocationStarted()
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::RemotingClientSendingMessage(GUID* pCookie, BOOL fIsAsync)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::RemotingClientReceivingReply(GUID* pCookie, BOOL fIsAsync)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::RemotingClientInvocationFinished()
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::RemotingServerReceivingMessage(GUID* pCookie, BOOL fIsAsync)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::RemotingServerInvocationStarted()
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::RemotingServerInvocationReturned()
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::RemotingServerSendingReply(GUID* pCookie, BOOL fIsAsync)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::UnmanagedToManagedTransition(FunctionID functionId,
                                                                 COR_PRF_TRANSITION_REASON reason)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ManagedToUnmanagedTransition(FunctionID functionId,
                                                                 COR_PRF_TRANSITION_REASON reason)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::RuntimeSuspendStarted(COR_PRF_SUSPEND_REASON suspendReason)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::RuntimeSuspendFinished()
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::RuntimeSuspendAborted()
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::RuntimeResumeStarted()
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::RuntimeResumeFinished()
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::RuntimeThreadSuspended(ThreadID threadId)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::RuntimeThreadResumed(ThreadID threadId)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::MovedReferences(ULONG cMovedObjectIDRanges, ObjectID oldObjectIDRangeStart[],
                                                    ObjectID newObjectIDRangeStart[], ULONG cObjectIDRangeLength[])
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ObjectAllocated(ObjectID objectId, ClassID classId)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ObjectsAllocatedByClass(ULONG cClassCount, ClassID classIds[], ULONG cObjects[])
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ObjectReferences(ObjectID objectId, ClassID classId, ULONG cObjectRefs,
                                                     ObjectID objectRefIds[])
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::RootReferences(ULONG cRootRefs, ObjectID rootRefIds[])
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ExceptionThrown(ObjectID thrownObjectId)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ExceptionSearchFunctionEnter(FunctionID functionId)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ExceptionSearchFunctionLeave()
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ExceptionSearchFilterEnter(FunctionID functionId)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ExceptionSearchFilterLeave()
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ExceptionSearchCatcherFound(FunctionID functionId)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ExceptionOSHandlerEnter(UINT_PTR __unused)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ExceptionOSHandlerLeave(UINT_PTR __unused)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ExceptionUnwindFunctionEnter(FunctionID functionId)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ExceptionUnwindFunctionLeave()
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ExceptionUnwindFinallyEnter(FunctionID functionId)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ExceptionUnwindFinallyLeave()
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ExceptionCatcherEnter(FunctionID functionId, ObjectID objectId)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ExceptionCatcherLeave()
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::COMClassicVTableCreated(ClassID wrappedClassId, REFGUID implementedIID,
                                                            void* pVTable, ULONG cSlots)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::COMClassicVTableDestroyed(ClassID wrappedClassId, REFGUID implementedIID,
                                                              void* pVTable)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ExceptionCLRCatcherFound()
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ExceptionCLRCatcherExecute()
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ThreadNameChanged(ThreadID threadId, ULONG cchName, WCHAR name[])
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::GarbageCollectionStarted(int cGenerations, BOOL generationCollected[],
                                                             COR_PRF_GC_REASON reason)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::SurvivingReferences(ULONG cSurvivingObjectIDRanges, ObjectID objectIDRangeStart[],
                                                        ULONG cObjectIDRangeLength[])
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::GarbageCollectionFinished()
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::FinalizeableObjectQueued(DWORD finalizerFlags, ObjectID objectID)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::RootReferences2(ULONG cRootRefs, ObjectID rootRefIds[],
                                                    COR_PRF_GC_ROOT_KIND rootKinds[], COR_PRF_GC_ROOT_FLAGS rootFlags[],
                                                    UINT_PTR rootIds[])
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::HandleCreated(GCHandleID handleId, ObjectID initialObjectId)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::HandleDestroyed(GCHandleID handleId)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::InitializeForAttach(IUnknown* pCorProfilerInfoUnk, void* pvClientData,
                                                        UINT cbClientData)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ProfilerAttachComplete()
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ProfilerDetachSucceeded()
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ReJITCompilationStarted(FunctionID functionId, ReJITID rejitId, BOOL fIsSafeToBlock)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::GetReJITParameters(ModuleID moduleId, mdMethodDef methodId,
                                                       ICorProfilerFunctionControl* pFunctionControl)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ReJITCompilationFinished(FunctionID functionId, ReJITID rejitId, HRESULT hrStatus,
                                                             BOOL fIsSafeToBlock)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ReJITError(ModuleID moduleId, mdMethodDef methodId, FunctionID functionId,
                                               HRESULT hrStatus)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::MovedReferences2(ULONG cMovedObjectIDRanges, ObjectID oldObjectIDRangeStart[],
                                                     ObjectID newObjectIDRangeStart[], SIZE_T cObjectIDRangeLength[])
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::SurvivingReferences2(ULONG cSurvivingObjectIDRanges, ObjectID objectIDRangeStart[],
                                                         SIZE_T cObjectIDRangeLength[])
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ConditionalWeakTableElementReferences(
	ULONG cRootRefs, ObjectID keyRefIds[], ObjectID valueRefIds[], GCHandleID rootIds[])
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::GetAssemblyReferences(const WCHAR* wszAssemblyPath,
                                                          ICorProfilerAssemblyReferenceProvider* pAsmRefProvider)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ModuleInMemorySymbolsUpdated(ModuleID moduleId)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::DynamicMethodJITCompilationStarted(FunctionID functionId, BOOL fIsSafeToBlock,
                                                                       LPCBYTE ilHeader, ULONG cbILHeader)
{
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::DynamicMethodJITCompilationFinished(FunctionID functionId, HRESULT hrStatus,
                                                                        BOOL fIsSafeToBlock)
{
	return S_OK;
}




HRESULT Profiler::QueryInterface(const IID& riid, void** ppvObject)
{
    if (riid == __uuidof(ICorProfilerCallback8) ||
        riid == __uuidof(ICorProfilerCallback7) ||
        riid == __uuidof(ICorProfilerCallback6) ||
        riid == __uuidof(ICorProfilerCallback5) ||
        riid == __uuidof(ICorProfilerCallback4) ||
        riid == __uuidof(ICorProfilerCallback3) ||
        riid == __uuidof(ICorProfilerCallback2) ||
        riid == __uuidof(ICorProfilerCallback) ||
        riid == IID_IUnknown)
    {
        *ppvObject = this;
        this->AddRef();
        return S_OK;
    }

    *ppvObject = nullptr;
    return E_NOINTERFACE;
}

ULONG Profiler::AddRef()
{

    return ::InterlockedIncrement(&_referenceCounter);
}

ULONG Profiler::Release()
{
    auto newValue = ::InterlockedDecrement(&_referenceCounter);

    if (newValue == 0)
    {
        delete this;
    }

    return newValue;
}
