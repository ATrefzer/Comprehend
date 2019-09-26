#pragma once
#include "FunctionCall.h"
#include <stack>

class Stack
{
public:

	std::stack<FunctionCall*> _stack;

	FunctionCall* _entryFunc;
	FunctionCall* _activeFunc;

	Stack()
	{
		_entryFunc = nullptr;
		_activeFunc = nullptr;
	}

	virtual ~Stack()
	{
		/// TODO delete all list
	}

	FunctionCall* ActiveFunction() const;

	FunctionCall* Push(FunctionInfo* info);

	FunctionCall* Pop();
};
