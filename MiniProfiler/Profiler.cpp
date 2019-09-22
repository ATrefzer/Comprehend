#include "pch.h"
#include "Profiler.h"

#include "corhlpr.h"
#include <string>


// http://www.blong.com/conferences/dcon2003/internals/profiling.htm

// https://github.com/microsoft/clr-samples/blob/master/ProfilingAPI/ELTProfiler/CorProfiler.cpp

// https://github.com/maybattle/Profiler1/blob/master/Profiler1/Profiler.cpp

// https://github.com/OpenCover/opencover/blob/master/main/OpenCover.Profiler/CodeCoverage_ProfilerInfo.cpp


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

// TODO atr resolve

#ifndef WIN32
	#define UINT_PTR_FORMAT "lx"
#else
#define PROFILER_CALLTYPE EXTERN_C void STDMETHODCALLTYPE
#define UINT_PTR_FORMAT "llx"
#endif

// TODO HACK
ICorProfilerInfo8* g_corProfilerInfo;
int g_spaces = 1;

std::wstring GetThreadId()
{
	// TODO monitor when threads are created
	// TODO Id seems wrong! (Too large)
	ThreadID id;
	g_corProfilerInfo->GetCurrentThreadID(&id);
	std::wstring tid = std::to_wstring(id);
	return tid;
}

std::wstring GetModuleName(FunctionID functionId)
{
	ClassID classId;
	ModuleID moduleId;
	mdToken functionToken;
	g_corProfilerInfo->GetFunctionInfo(functionId, &classId, &moduleId, &functionToken);

	wchar_t name[1000];
	g_corProfilerInfo->GetModuleInfo(moduleId, NULL, sizeof(name), nullptr, name, NULL);
	return std::wstring(name);
}

std::wstring GetFunctionName(FunctionID functionId)
{
	wchar_t funcName[4000];
	wchar_t typeName[4000];


	mdToken functionToken = mdTypeDefNil;
	mdTypeDef classToken = mdTypeDefNil;
	IMetaDataImport* pMDImport = nullptr;
	g_corProfilerInfo->GetTokenAndMetaDataFromFunction(functionId,
	                                                   IID_IMetaDataImport, reinterpret_cast<IUnknown**>(&pMDImport),
	                                                   &functionToken);


	pMDImport->GetMethodProps(functionToken,
	                          &classToken,
	                          funcName, sizeof(funcName),
	                          nullptr, nullptr, nullptr, nullptr, nullptr, nullptr);


	pMDImport->GetTypeDefProps(classToken, typeName, sizeof(typeName), nullptr, nullptr, nullptr);


	return std::wstring(typeName) + std::wstring(L".") + std::wstring(funcName);
}

std::wstring Format(std::wstring prefix, std::wstring& tid, std::wstring& name, int numSpaces)
{
	const auto spaces = std::wstring(numSpaces, ' ');

	std::wstring msg;
	msg.append(L"\r\n[");
	msg.append(tid);
	msg.append(L"]");
	msg.append(spaces);
	msg.append(prefix);
	msg.append(L" ");
	msg.append(name);
	return msg;
}

bool Ignore(std::wstring & moduleName)
{
	
	return moduleName.find(L"mscorlib.dll") != std::wstring::npos;
}

void OnEnter(FunctionIDOrClientID functionIDOrClientID, COR_PRF_ELT_INFO eltInfo)
{
	// TODO function ids may change!
	/*
	 *
	 * ULONG pcbArgumentInfo = 0;
    COR_PRF_FRAME_INFO frameInfo;
    g_corProfilerInfo->GetFunctionEnter3Info(functionIDOrClientID.functionID, eltInfo, &frameInfo, &pcbArgumentInfo, NULL);

    char* pArgumentInfo = new char[pcbArgumentInfo];
    g_corProfilerInfo->GetFunctionEnter3Info(functionIDOrClientID.functionID, eltInfo, &frameInfo, &pcbArgumentInfo, (COR_PRF_FUNCTION_ARGUMENT_INFO*)pArgumentInfo);

    COR_PRF_FUNCTION_ARGUMENT_INFO* ptr = (COR_PRF_FUNCTION_ARGUMENT_INFO*)pArgumentInfo;*/

	
	const auto functionId = functionIDOrClientID.functionID;
	auto moduleName = GetModuleName(functionId);
	auto name = GetFunctionName(functionId);

	if (Ignore(moduleName))
	{
		return;
	}

	auto tid = GetThreadId();
	const auto msg = Format(L"Enter", tid, name, g_spaces);
	OutputDebugString(msg.c_str());

	g_spaces++;
}


void OnLeave(FunctionIDOrClientID functionIDOrClientID, COR_PRF_ELT_INFO eltInfo)
{
	const auto functionId = functionIDOrClientID.functionID;
	auto moduleName = GetModuleName(functionId);
	auto name = GetFunctionName(functionId);
	if (Ignore(moduleName))
	{
		return;
	}

	g_spaces--;

	auto spaces = std::wstring(g_spaces, ' ');
	auto tid = GetThreadId();
	const auto msg = Format(L"Leave", tid, name, g_spaces);
	OutputDebugString(msg.c_str());
}

bool OnTailCall(FunctionIDOrClientID functionIDOrClientID, COR_PRF_ELT_INFO eltInfo)
{
	const auto functionId = functionIDOrClientID.functionID;
	auto moduleName = GetModuleName(functionId);
	auto name = GetFunctionName(functionId);
	if (Ignore(moduleName))
	{
		return true;
	}

	auto spaces = std::wstring(g_spaces, ' ');
	auto tid = GetThreadId();
	const auto msg = Format(L"TailCall", tid, name, g_spaces);
	OutputDebugString(msg.c_str());
}

void _stdcall EnterFunc(FunctionIDOrClientID functionIDOrClientID, COR_PRF_ELT_INFO eltInfo)
{
	// TODO Registers are not saved when these callbacks are invoked. So why is it working fine?
	OnEnter(functionIDOrClientID, eltInfo);
}

void _stdcall LeaveFunc(FunctionIDOrClientID functionIDOrClientID, COR_PRF_ELT_INFO eltInfo)
{
	// TODO Registers are not saved when these callbacks are invoked. So why is it working fine?
	OnLeave(functionIDOrClientID, eltInfo);
}

void _stdcall TailCallFunc(FunctionIDOrClientID functionIDOrClientID, COR_PRF_ELT_INFO eltInfo)
{
	// TODO Registers are not saved when these callbacks are invoked. So why is it working fine?
	OnTailCall(functionIDOrClientID, eltInfo);
}

Profiler::Profiler() : _referenceCounter(0), _corProfilerInfo(nullptr)
{
}

Profiler::~Profiler()
{
	if (this->_corProfilerInfo != nullptr)
	{
		this->_corProfilerInfo->Release();
		this->_corProfilerInfo = nullptr;
	}
}

HRESULT STDMETHODCALLTYPE Profiler::Initialize(IUnknown* pICorProfilerInfoUnk)
{
	HRESULT queryInterfaceResult = pICorProfilerInfoUnk->QueryInterface(__uuidof(ICorProfilerInfo8),
	                                                                    reinterpret_cast<void **>(&_corProfilerInfo));


	if (FAILED(queryInterfaceResult))
	{
		return E_FAIL;
	}

	DWORD eventMask = COR_PRF_MONITOR_ENTERLEAVE | COR_PRF_ENABLE_FUNCTION_ARGS | COR_PRF_ENABLE_FUNCTION_RETVAL |
		COR_PRF_ENABLE_FRAME_INFO;

	// TODO hack
	g_corProfilerInfo = _corProfilerInfo;


	auto hr = _corProfilerInfo->SetEventMask(eventMask);
	if (hr != S_OK)
	{
		printf("ERROR: Profiler SetEventMask failed (HRESULT: %d)", hr);
	}

	//hr = _corProfilerInfo->SetEnterLeaveFunctionHooks3WithInfo(EnterNaked, LeaveNaked, TailCallNaked);
	hr = _corProfilerInfo->SetEnterLeaveFunctionHooks3WithInfo(EnterFunc, LeaveFunc, TailCallFunc);

	if (hr != S_OK)
	{
		printf("ERROR: Profiler SetEnterLeaveFunctionHooks3WithInfo failed (HRESULT: %d)", hr);
	}

	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::Shutdown()
{
	if (_corProfilerInfo != nullptr)
	{
		_corProfilerInfo->Release();
		_corProfilerInfo = nullptr;
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
	return S_OK;
}

HRESULT STDMETHODCALLTYPE Profiler::ThreadDestroyed(ThreadID threadId)
{
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
