#include <corhlpr.h>
#include <corprof.h>

#include "Callbacks.h"

// Note: When profiling CefSharp.BrowserSubProcess.exe crashes with these functions.
// It only works when we save the FPU register, too.
// However, it seems pretty rare that this problem occurs.

void __declspec(naked) __stdcall EnterNakedFuncOld(FunctionIDOrClientID functionIDOrClientID, COR_PRF_ELT_INFO eltInfo)
{
	__asm
		{
		push ebp
		mov ebp, esp

		pushad // Push general purpose registers
		mov edx, [ebp + 12] // ebp + 12 = second parameter: eltInfo
		push edx
		mov eax, [ebp + 8] // epb+8 = first parameter: functionId
		push eax
		call OnEnter // __stdcall: parameters are pushed right to left. 
		popad
		pop ebp
		ret 8 // __stdcall: Callee cleans up the parameters
		}
}

void __declspec(naked) __stdcall LeaveNakedFuncOld(FunctionIDOrClientID functionIDOrClientID, COR_PRF_ELT_INFO eltInfo)
{
	__asm
		{
		push ebp
		mov ebp, esp
		pushad
		mov edx, [ebp + 12]
		push edx
		mov eax, [ebp + 8]
		push eax
		call OnLeave
		popad
		pop ebp
		ret 8
		}
}

void __declspec(naked) __stdcall TailCallNakedFuncOld(FunctionIDOrClientID functionIDOrClientID,
                                                      COR_PRF_ELT_INFO eltInfo)
{
	__asm
		{
		push ebp
		mov ebp, esp
		pushad
		mov edx, [ebp + 12]
		push edx
		mov eax, [ebp + 8]
		push eax
		call OnTailCall
		popad
		pop ebp
		ret 8
		}
}
