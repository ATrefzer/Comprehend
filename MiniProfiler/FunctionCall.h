#pragma once
#include "ProfilerApi.h"
/// Abstracts hidded calls
class FunctionCall
{
public:
    // 0 if the function is hidden
    FunctionInfo* _info;

    FunctionCall(FunctionInfo* info)
    {
        _info = info;
    }

    bool IsHidden()
    {
        return _info->IsHidden();
 }

};
