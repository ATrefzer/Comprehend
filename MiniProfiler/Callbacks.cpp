#include "Callbacks.h"
#include <string>
#include "ProfileWriter.h"

ProfileWriter* _callTrace;

// Note: Naked function calls below needs __stdcall!
void __stdcall  OnEnter(FunctionIDOrClientID functionIDOrClientID, COR_PRF_ELT_INFO eltInfo)
{
   _callTrace->OnEnter(functionIDOrClientID.functionID);
    // TODO function ids may change!
    /*
     *
     * ULONG pcbArgumentInfo = 0;
    COR_PRF_FRAME_INFO frameInfo;
    g_corProfilerInfo->GetFunctionEnter3Info(functionIDOrClientID.functionID, eltInfo, &frameInfo, &pcbArgumentInfo, NULL);

    char* pArgumentInfo = new char[pcbArgumentInfo];
    g_corProfilerInfo->GetFunctionEnter3Info(functionIDOrClientID.functionID, eltInfo, &frameInfo, &pcbArgumentInfo, (COR_PRF_FUNCTION_ARGUMENT_INFO*)pArgumentInfo);

    COR_PRF_FUNCTION_ARGUMENT_INFO* ptr = (COR_PRF_FUNCTION_ARGUMENT_INFO*)pArgumentInfo;*/
}


void __stdcall   OnLeave(FunctionIDOrClientID functionIDOrClientID, COR_PRF_ELT_INFO eltInfo)
{
    _callTrace->OnLeave(functionIDOrClientID.functionID);
}

void  __stdcall  OnTailCall(FunctionIDOrClientID functionIDOrClientID, COR_PRF_ELT_INFO eltInfo)
{
    _callTrace->OnTailCall(functionIDOrClientID.functionID);
}