//
// This file is part of the C++ Essential Library
// This software is provided "AS IS", without a warranty of any kind.
// You are free to use / modify this code but leave this header intact
//

#include "SynchronizationPrimitives.h"

namespace CppEssentials
{

    CriticalSection::CriticalSection()
    {
        InitializeCriticalSection(&m_criticalSection);
    }

    CriticalSection::~CriticalSection()
    {
        DeleteCriticalSection(&m_criticalSection);
    }

    LPCRITICAL_SECTION CriticalSection::Get()
    {
        return &m_criticalSection;
    }

    void CriticalSection::Enter()
    {
        EnterCriticalSection(&m_criticalSection);
    }

    void CriticalSection::Leave()
    {
        LeaveCriticalSection(&m_criticalSection);
    }

#if(_WIN32_WINNT >= 0x0400)
    bool CriticalSection::TryEnter()
    {
        return (TryEnterCriticalSection(&m_criticalSection) == TRUE);
    }
#endif

    Event::Event(BOOL bManualReset /*= FALSE*/, BOOL bInitialState /*= FALSE*/, LPCTSTR lpszName /*= NULL*/)
    {
        _hEvent = ::CreateEvent(NULL, bManualReset, bInitialState, lpszName);
    }

    Event::~Event()
    {
        CloseHandle(_hEvent);
    }

    bool Event::Wait(DWORD timeoutInMs)
    {
        return WaitForSingleObject(_hEvent, timeoutInMs) == WAIT_OBJECT_0;
    }

    BOOL Event::IsSignaled()
    {
        BOOL bSignaled = FALSE;
        DWORD dwResult = WaitForSingleObject(_hEvent, 0);
        if (dwResult == WAIT_OBJECT_0)
        {
            bSignaled = TRUE;
        }

        return bSignaled;
    }

    BOOL Event::Signal()
    {
        return SetEvent(_hEvent);
    }

    BOOL Event::Reset()
    {
        return ResetEvent(_hEvent);
    }

    HANDLE Event::Get()
    {
        return _hEvent;
    }

    Event::operator HANDLE()
    {
        return Get();
    }

    ScopeLock::ScopeLock(CRITICAL_SECTION * criticalSection)
    {
        _criticalSection = criticalSection;
        Lock();
    }

    ScopeLock::~ScopeLock()
    {
        Release();
    }

    void ScopeLock::Release()
    {
        if (_criticalSection)
        {
            LeaveCriticalSection(_criticalSection);
            _criticalSection = NULL;
        }
    }

    void ScopeLock::Lock()
    {
        EnterCriticalSection(_criticalSection);
    }

}