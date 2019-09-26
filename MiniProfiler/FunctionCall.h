#pragma once
#include "ProfilerApi.h"

/// Abstracts hidden calls
class FunctionCall
{
public:
	FunctionInfo* _info;

	explicit FunctionCall(FunctionInfo* info);

	bool IsHidden();
};
