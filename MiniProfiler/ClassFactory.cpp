#include "pch.h"

#include "ClassFactory.h"
#include "Profiler.h"


ClassFactory::ClassFactory() : _referenceCounter(0)
{
}

ClassFactory::~ClassFactory()
{
	OutputDebugString(L"ClassFactory::~ClassFactory");
}

HRESULT STDMETHODCALLTYPE ClassFactory::QueryInterface(REFIID riid, void** ppvObject)
{
	if (riid == IID_IUnknown || riid == IID_IClassFactory)
	{
		*ppvObject = this;
		this->AddRef();
		return S_OK;
	}

	*ppvObject = nullptr;
	return E_NOINTERFACE;
}

ULONG STDMETHODCALLTYPE ClassFactory::AddRef()
{
	return ::InterlockedIncrement(&_referenceCounter);
}

ULONG STDMETHODCALLTYPE ClassFactory::Release()
{
	const auto newValue = ::InterlockedDecrement(&_referenceCounter);
	if (newValue == 0)
	{
		delete this;
	}

	return newValue;
}

HRESULT STDMETHODCALLTYPE ClassFactory::CreateInstance(IUnknown* pUnkOuter, REFIID riid, void** ppvObject)
{
	if (pUnkOuter != nullptr)
	{
		*ppvObject = nullptr;
		return CLASS_E_NOAGGREGATION;
	}


    // Any of the profiler callback interfaces ends at this implementation.
	auto profiler = new Profiler(); // throws bad_alloc

	return profiler->QueryInterface(riid, ppvObject);
}

HRESULT STDMETHODCALLTYPE ClassFactory::LockServer(BOOL fLock)
{
	// Affects the DllCanUnload exported function. Ignore.
	return S_OK;
}
