//
// This file is part of the C++ Essential Library
// This software is provided "AS IS", without a warranty of any kind.
// You are free to use / modify this code but leave this header intact
//

#pragma once

#include <exception>
#include <string>
#include <sstream>
#include <Windows.h>
//#include <Strsafe.h>
using namespace std;

namespace CppEssentials
{
	/// General exception class for this library. Unlike the std::exception object
	/// it allows to convey unicode messages.
	///
	class Exception : public exception
	{
	public:

		/// Default constructor. It only creates a stack dump.
		///
		Exception();

		/// Constructor
		/// @param sourceLocation   Source code location where the exception was thrown
		/// @param message          Message why the exception was thrown.
		/// @param lastError        Optional error code, received via ::GetLastError()
		///
		Exception(const wstring& message, unsigned long lastError = 0);

		/// Constructor
		/// @param sourceLocation   Source code location where the exception was thrown
		/// @param message          Message in which context or why the exception was thrown.
		/// @param details          More details about the problem. (i.e. the string that couldn't be converted)
		/// @param lastError        Optional error code, received via ::GetLastError()
		///
		Exception(const wstring& message, const wstring& details, unsigned long lastError = 0);

		/// Proper destructor definition required by std::exception
		///
		virtual ~Exception() noexcept;

		/// Returns the message passed to the constructor.
		/// Do not use this method. It needs to be present due to the std::exception base class.
		///
		const char* what() const override;

		wstring GetMessage();

		wstring GetDetails();

		unsigned long GetLastError();

		/// Returns the stack dump created when the the constructor of the Exception class
		/// was called.
		///
		wstring GetStackDump();

		/// Returns the Windows system error text for the error code set in constructor.
		/// If the error code was not set an empty string is returned.
		///
		//static wstring ResolveLastError(unsigned long lastError);
		static wstring ResolveLastError(unsigned long lastError, unsigned long langId = LANG_SYSTEM_DEFAULT);

		/// Creates a stack dump
		std::string DumpStack();

	private:

		/// Converts the message (ctor) to a string returned by the what() method of std::exception.
		void MakeWhat();

		wstring _message;
		wstring _details;

		unsigned long _lastError;

		/// Holder for std::exception::what()
		string _what;

		string _stackDump;
	};

	
}
