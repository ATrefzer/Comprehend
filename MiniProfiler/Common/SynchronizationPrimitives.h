//
// This file is part of the C++ Essential Library
// This software is provided "AS IS", without a warranty of any kind.
// You are free to use / modify this code but leave this header intact
//

#pragma once
#include <windows.h>

namespace CppEssentials
{

//class Boolean
//{
//public:
//
//    Boolean()
//    {
//        _flag = FALSE;
//    }
//
//    bool IsSet()
//    {
//        // If outer values are equal write the middle one to the left target!
//        // Otherwise nothing happens.
//        return (InterlockedCompareExchange((LONG*)&_flag, TRUE, TRUE) == TRUE);
//
//        //
//        //InterlockedCompareExchange((LONG*)&fValueHasBeenComputed,
//        //  FALSE, FALSE)==FALSE
//    }
//
//    bool IsClear()
//    {
//        // If outer values are equal write the middle one to the left target!
//        // Otherwise nothing happens.
//        return (InterlockedCompareExchange((LONG*)&_flag, FALSE, FALSE) == FALSE);
//    }
//
//    /// Returns the old value
//    bool Set()
//    {
//        return (::InterlockedExchange((LONG*)&_flag, TRUE) == TRUE);
//    }
//
//    /// Returns the old value
//    bool Clear()
//    {
//        return (::InterlockedExchange((LONG*)&_flag, FALSE) == TRUE);
//    }
//
//private:
//    LONG _flag;
//
//};

///
/// ScopeLock is used to enter a critical section in its constructor and
/// to leave it in destructor. It is used to ensure that in error cases the
/// critical section is left automatically when the object runs out of scope.
///
class ScopeLock
{

public:

    ScopeLock(CRITICAL_SECTION * criticalSection);

    ///
    /// Destructor automatically releases the critical section, if it is not released yet
    ///
    virtual ~ScopeLock();

    ///
    /// Releases the critical section.
    ///
    void Release();

private:

    void Lock();

    /// Critical section passed in the constructor
    CRITICAL_SECTION * _criticalSection;
};

class CriticalSection
{
public:

    CriticalSection();

    ~CriticalSection();

    LPCRITICAL_SECTION Get();

    operator LPCRITICAL_SECTION()
    {
        return &m_criticalSection;
    }

    void Enter();

    void Leave();

#if(_WIN32_WINNT >= 0x0400)
    bool TryEnter();
#endif

private:

    // No copy no assignment because the destructor will destroy the critical section!
    CriticalSection(const CriticalSection & section);
    void operator=(const CriticalSection & section);

    CRITICAL_SECTION m_criticalSection;
};

class Event
{
public:

    Event(BOOL bManualReset = FALSE, BOOL bInitialState = FALSE, LPCTSTR lpszName = NULL);

    ~Event();

    BOOL IsSignaled();

    BOOL Signal();

    BOOL Reset();

    HANDLE Get();

    bool Wait(DWORD timeoutInMs = INFINITE);

    operator HANDLE();

private:

    // No copy, no assignment because the destructor will destroy the event
    Event(const Event & evt);
    void operator=(const Event & evt);

    HANDLE _hEvent;
};

class ManualResetEvent : public Event
{
public:
    ManualResetEvent(BOOL initialState = FALSE, LPCTSTR lpszName = NULL) : Event(TRUE, initialState, lpszName)
    {

    }

private:

    ManualResetEvent(const ManualResetEvent & evt);
    void operator=(const ManualResetEvent & evt);

};

/// Lock for multiple readers and single writer.
/// Once the writer is waiting new readers have to wait.
///
class ReaderWriterLock
{
public:
    ReaderWriterLock() : _readersCleared(TRUE, TRUE)
    {
        _readers = 0;
    }

    ~ReaderWriterLock()
    {
        WaitForSingleObject(_readersCleared.Get() , INFINITE);
    }

    void EnterReader(void)
    {
        _write.Enter();

        IncrementReader();

        _write.Leave();
    }

    void LeaveReader(void)
    {
        DecrementReader();
    }

    void EnterWriter(void)
    {
        _write.Enter();
        WaitForSingleObject(_readersCleared.Get(), INFINITE);
    }

    void LeaveWriter(void)
    {
        _write.Leave();
    }

private:

    void IncrementReader()
    {
        _readerCount.Enter();
        _readers++;
        if (_readers == 1)
        {
            _readersCleared.Reset();
        }
        _readerCount.Leave();
    }

    void DecrementReader()
    {
        _readerCount.Enter();

        _readers--;
        if (_readers == 0)
        {
            _readersCleared.Signal();
        }

        _readerCount.Leave();
    }

    CriticalSection _write;
    CriticalSection _readerCount;
    long _readers;
    Event _readersCleared;
};
}
