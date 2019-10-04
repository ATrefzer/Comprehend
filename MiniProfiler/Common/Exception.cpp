//
// This file is part of the C++ Essential Library
// This software is provided "AS IS", without a warranty of any kind.
// You are free to use / modify this code but leave this header intact
//

#include "Exception.h"
#include <sstream>


namespace CppEssentials
{
	Exception::Exception()
	{
		_lastError = 0;
	}

	Exception::Exception(const wstring& message, unsigned long lastError /*= 0*/)
	{
		_lastError = lastError;
		_message = message;
		MakeWhat();
	}

	Exception::Exception(const wstring& message, const wstring& details, unsigned long lastError /*= 0*/)
	{
		_lastError = lastError;
		_message = message;
		_details = details;

		MakeWhat();
	}

	Exception::~Exception() noexcept
	= default;

	Exception::Exception(const Exception& e): exception(e)
	{
		_message = e._message;
		_details = e._details;
		_lastError = e._lastError;
		_what = e._what;
	}

	const char* Exception::what() const
	{
		return _what.c_str();
	}

	std::wstring Exception::GetMessage() const
	{
		return _message;
	}

	std::wstring Exception::GetDetails() const
	{
		return _details;
	}

	unsigned long Exception::GetLastError() const
	{
		return _lastError;
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


#pragma endregion
}
