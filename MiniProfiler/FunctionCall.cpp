#include "pch.h"
#include "FunctionCall.h"

FunctionCall::FunctionCall(FunctionInfo* info)
{
	_info = info;
}

bool FunctionCall::IsHidden()
{
	return _info->IsHidden();
}
