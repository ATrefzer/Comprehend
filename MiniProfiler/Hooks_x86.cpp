#include <corhlpr.h>
#include <corprof.h>

#include "Callbacks.h"

//void __declspec(naked) __stdcall EnterNakedFunc(FunctionIDOrClientID functionIDOrClientID, COR_PRF_ELT_INFO eltInfo)
//{
//    __asm
//    {
//        push ebp
//        mov ebp, esp
//
//        pushad // Push general purpose registers
//        mov edx, [ebp + 12] // ebp + 12 = second parameter: eltInfo
//        push edx
//        mov eax, [ebp + 8]  // epb+8 = first parameter: functionId
//        push eax
//        call OnEnter // __stdcall: parameters are pushed right to left. 
//        popad
//        pop ebp
//        ret 8   // __stdcall: Callee cleans up the parameters
//    }
//}

// TODO Try saving FP like it is done here.
// http://read.pudn.com/downloads64/sourcecode/windows/system/228104/leave_x86.cpp__.htm



void __declspec(naked) __stdcall EnterNakedFunc(FunctionIDOrClientID functionIDOrClientID, COR_PRF_ELT_INFO eltInfo)
{
    __asm
    {
        // Needed otherwide the function call does not work.
        push ebp
        mov ebp, esp

        // Make space for locals.   
        // TODO sub esp, __LOCAL_SIZE
  
        pushad

        // Check if there's anything on the FP stack.   
        //   
        fstsw   ax
        and ax, 3800h       // Check the top-of-fp-stack bits   
        cmp     ax, 0           // If non-zero, we have something to save   
        jnz     SaveFPReg
        push    0               // otherwise, mark that there is no float value   
        jmp     NoSaveFPReg
        SaveFPReg :
        sub     esp, 8          // Make room for the FP value   
            fstp    qword ptr[esp] // Copy the FP value to the buffer as a double   
            push    1               // mark that a float value is present   
            NoSaveFPReg :
    }

   
    OnEnter(functionIDOrClientID, eltInfo);

    __asm
    {
        // Now see if we have to restore any floating point registers   //     
        cmp[esp], 0            // Check the flag   
        jz      NoRestoreFPRegs     // If zero, no FP regs   
        RestoreFPRegs :
            fld     qword ptr[esp + 4] // Restore FP regs   
            add    esp, 12              // Move ESP past the storage space   
            jmp   RestoreFPRegsDone
        NoRestoreFPRegs :
            add     esp, 4              // Move ESP past the flag   
            RestoreFPRegsDone :

            // Restore other registers   
            popad

            // Pop off locals   
           // add esp, __LOCAL_SIZE

            pop ebp

            ret SIZE functionIDOrClientID + SIZE eltInfo
    }
}



void __declspec(naked)  __stdcall LeaveNakedFunc(FunctionIDOrClientID functionIDOrClientID, COR_PRF_ELT_INFO eltInfo)
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

void __declspec(naked) __stdcall  TailCallNakedFunc(FunctionIDOrClientID functionIDOrClientID, COR_PRF_ELT_INFO eltInfo)
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