#pragma once

#include "unknwn.h"

class ClassFactory : public IClassFactory
{
private:
	LONG _referenceCounter = 0;
public:
	ClassFactory();
	virtual ~ClassFactory();
	HRESULT STDMETHODCALLTYPE QueryInterface(REFIID riid, void** ppvObject) override;
	ULONG STDMETHODCALLTYPE AddRef(void) override;
	ULONG STDMETHODCALLTYPE Release(void) override;
	HRESULT STDMETHODCALLTYPE CreateInstance(IUnknown* pUnkOuter, REFIID riid, void** ppvObject) override;
	HRESULT STDMETHODCALLTYPE LockServer(BOOL fLock) override;
};
