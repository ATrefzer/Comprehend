#pragma once
#include <corhlpr.h>
#include <corprof.h>
#include "ProfileWriter.h"

extern ProfileWriter* _callTrace;

// Callbacks for x86 and x64 code
void __stdcall OnEnter(FunctionIDOrClientID functionIDOrClientID, COR_PRF_ELT_INFO eltInfo);
void __stdcall OnLeave(FunctionIDOrClientID functionIDOrClientID, COR_PRF_ELT_INFO eltInfo);
void __stdcall OnTailCall(FunctionIDOrClientID functionIDOrClientID, COR_PRF_ELT_INFO eltInfo);
