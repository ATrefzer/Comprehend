// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"
#include "ClassFactory.h"


const IID IID_IUnknown = {0x00000000, 0x0000, 0x0000, {0xC0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x46}};

const IID IID_IClassFactory = {0x00000001, 0x0000, 0x0000, {0xC0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x46}};

// {7E981B79-6303-483F-A372-8169B1073A0F}
static const GUID CLSID_PROFILER = {0x7e981b79, 0x6303, 0x483f, {0xa3, 0x72, 0x81, 0x69, 0xb1, 0x7, 0x3a, 0xf}};

BOOL APIENTRY DllMain(HMODULE hModule,
                      DWORD ul_reason_for_call,
                      LPVOID lpReserved
)
{
	OutputDebugStringA("DllMain");
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
	case DLL_THREAD_ATTACH:
	case DLL_THREAD_DETACH:
	case DLL_PROCESS_DETACH:
		break;
	}
	return TRUE;
}

extern "C" HRESULT STDMETHODCALLTYPE DllGetClassObject(REFCLSID rclsid, REFIID riid, LPVOID* ppv)
{
	OutputDebugStringA("DllGetClassObject");
	if (ppv == nullptr || rclsid != CLSID_PROFILER)
	{
		return E_FAIL;
	}

	auto factory = new ClassFactory(); // throws bad_alloc


	return factory->QueryInterface(riid, ppv);
}

extern "C" HRESULT STDMETHODCALLTYPE DllCanUnloadNow()
{
	return S_OK;
}
