#include <corhlpr.h>
#include <corprof.h>

#include "Callbacks.h"


// Idea of saving the FP related parts are from here.
// http://read.pudn.com/downloads64/sourcecode/windows/system/228104/leave_x86.cpp__.htm

//#define USE_LOCAL_VARS

void __declspec(naked) __stdcall EnterNakedFunc(FunctionIDOrClientID functionIDOrClientID, COR_PRF_ELT_INFO eltInfo)
{
	__asm
		{

		// Needed otherwise the function call below does not work.
		// Debugging is still not possible.

		// Store old frame base pointer
		push ebp

		// Stack pointer is the new base pointer for this stack frame
		mov ebp, esp

#ifdef USE_LOCAL_VARS
		sub esp, __LOCAL_SIZE
#endif

		// Store all general purpose registers 
		pushad

		// Check if there's anything on the FP stack.   

		// Store x87 FPU Status Word
		fstsw ax

		// Check the top-of-stack pinter (3 bits, 0...7)
		and ax, 3800h
		// If non-zero, we have something to save 
		cmp ax, 0
		jnz SaveFPReg

		// Otherwise mark that there is no float value
		push 0
		jmp NoSaveFPReg
		SaveFPReg :

		// Create space on stack for the FP value.
		sub esp, 8

		// Stores top of floating point register stack to the buffer as a double. Pops value.
		fstp qword ptr[esp]

		// Mark that a float value is present
		push 1
		NoSaveFPReg:
		}


	OnEnter(functionIDOrClientID, eltInfo);

	__asm
		{
		// Test if we have something to restore. If zero no FP registers.
		cmp[esp], 0
		jz NoRestoreFPRegs
		RestoreFPRegs:

		// Load floating point value: Pushes the source operand onto the FPU register stack
		fld qword ptr[esp + 4]

		// Move esp past the floating value storage space
		add esp, 12
		jmp RestoreFPRegsDone
		NoRestoreFPRegs :

		// Move esp past the flag
		add esp, 4
		RestoreFPRegsDone:

		// Restore general purpose registers
		popad

#ifdef USE_LOCAL_VARS
		// add esp, __LOCAL_SIZE
#endif

		pop ebp

		ret SIZE functionIDOrClientID + SIZE eltInfo
		}
}


void __declspec(naked) __stdcall LeaveNakedFunc(FunctionIDOrClientID functionIDOrClientID, COR_PRF_ELT_INFO eltInfo)
{
	__asm
		{
		// Needed otherwise the function call does not work.
		push ebp
		mov ebp, esp

#ifdef USE_LOCAL_VARS
		sub esp, __LOCAL_SIZE
#endif

		pushad


		fstsw ax
		and ax, 3800h
		cmp ax, 0
		jnz SaveFPReg
		push 0
		jmp NoSaveFPReg
		SaveFPReg :
		sub esp, 8
		fstp qword ptr[esp]
		push 1
		NoSaveFPReg :
		}


	OnLeave(functionIDOrClientID, eltInfo);

	__asm
		{

		cmp[esp], 0
		jz NoRestoreFPRegs
		RestoreFPRegs:
		fld qword ptr[esp + 4]
		add esp, 12
		jmp RestoreFPRegsDone
		NoRestoreFPRegs :
		add esp, 4
		RestoreFPRegsDone:


		popad

#ifdef USE_LOCAL_VARS
		add esp, __LOCAL_SIZE
#endif

		pop ebp

		ret SIZE functionIDOrClientID + SIZE eltInfo
		}
}


void __declspec(naked) __stdcall TailCallNakedFunc(FunctionIDOrClientID functionIDOrClientID,
                                                   COR_PRF_ELT_INFO eltInfo)
{
	__asm
		{
		push ebp
		mov ebp, esp

#ifdef USE_LOCAL_VARS
		sub esp, __LOCAL_SIZE
#endif

		pushad


		fstsw ax
		and ax, 3800h
		cmp ax, 0
		jnz SaveFPReg
		push 0
		jmp NoSaveFPReg
		SaveFPReg :
		sub esp, 8
		fstp qword ptr[esp]
		push 1
		NoSaveFPReg :
		}


	OnTailCall(functionIDOrClientID, eltInfo);

	__asm
		{

		cmp[esp], 0
		jz NoRestoreFPRegs
		RestoreFPRegs:
		fld qword ptr[esp + 4]
		add esp, 12
		jmp RestoreFPRegsDone
		NoRestoreFPRegs :
		add esp, 4
		RestoreFPRegsDone:

		popad

#ifdef USE_LOCAL_VARS
		add esp, __LOCAL_SIZE
#endif

		pop ebp

		ret SIZE functionIDOrClientID + SIZE eltInfo
		}
}
