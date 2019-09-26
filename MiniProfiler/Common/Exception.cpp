//
// This file is part of the C++ Essential Library
// This software is provided "AS IS", without a warranty of any kind.
// You are free to use / modify this code but leave this header intact
//

//#include <atlutil.h>
#include "Exception.h"
#include <sstream>


namespace CppEssentials
{
	/*
	    /// Internal helper class to build the call stack dump.
	    class StackDumpHandlerImpl : public IStackDumpHandler
	    {
	    public:

	        StackDumpHandlerImpl()
	        {
	        }

	        void __stdcall OnBegin()
	        {
	        }

	        void __stdcall OnEntry(void *pvAddress, LPCSTR szModule, LPCSTR szSymbol)
	        {
	            (void)pvAddress;

	            _stream << szModule << " - " << szSymbol << "\n";
	        }
	        void __stdcall OnError(LPCSTR szError)
	        {
	            _stream << "Error traversing call stack: " << szError;
	        }

	        void __stdcall OnEnd()
	        {
	        }

	        std::string GetStackDump()
	        {
	            return _stream.str();
	        }

	        std::stringstream _stream;
	    };

	    */

	Exception::Exception()
	{
		_lastError = 0;
		_stackDump = DumpStack();
	}

	Exception::Exception(const wstring& message, unsigned long lastError /*= 0*/)
	{
		_lastError = lastError;
		_message = message;

		_stackDump = DumpStack();
		MakeWhat();
	}

	Exception::Exception(const wstring& message, const wstring& details, unsigned long lastError /*= 0*/)
	{
		_lastError = lastError;
		_message = message;
		_details = details;

		_stackDump = DumpStack();
		MakeWhat();
	}

	Exception::~Exception() noexcept
	{
	}

	const char* Exception::what() const
	{
		return _what.c_str();
	}

	std::wstring Exception::GetMessage()
	{
		return _message;
	}

	std::wstring Exception::GetDetails()
	{
		return _details;
	}

	unsigned long Exception::GetLastError()
	{
		return _lastError;
	}

	std::wstring Exception::GetStackDump()
	{
		// Simply widen the characters
		return wstring(_stackDump.begin(), _stackDump.end());
	}

	void Exception::MakeWhat()
	{
		// Simply narrow the characters. It is up to the user if he prefers to use the what() method
		_what = string(_message.begin(), _message.end());
	}

#pragma region Utility methods

	wstring Exception::ResolveLastError(unsigned long lastError, unsigned long langId)
	{
		wstring msg;

		if (lastError != 0)
		{
			LPWSTR lpBuffer = nullptr;
			FormatMessageW(
				FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM,
				nullptr,
				lastError,
				//LANG_USER_DEFAULT,
				//MAKELANGID(LANG_ENGLISH , SUBLANG_ENGLISH_US), Not available on my system!
				langId,
				(LPTSTR)&lpBuffer,
				0,
				nullptr);

			if (lpBuffer)
			{
				msg = lpBuffer;
				LocalFree(lpBuffer);
			}
		}

		return msg;
	}

	std::string Exception::DumpStack()
	{
		return "Not implemented";

		/*
		ATL Version 8 has a memory leak here! So we do not use this code.
		string stackDump;
		try
		{
		    StackDumpHandlerImpl impl;
		    AtlDumpStack(&impl);
		    stackDump = impl.GetStackDump();
		}
		catch(...)
		{
		    stackDump = "Stack dump not available!";
		}

		return stackDump;
		*/
	}
#pragma endregion
}
