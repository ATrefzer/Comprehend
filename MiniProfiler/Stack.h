#pragma once
#include "FunctionCall.h"
#include <stack>

class Stack
{
public:

    int _level = 1;
    int Level()
    {
        return _level;
    }

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
