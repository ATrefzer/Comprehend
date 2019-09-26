#pragma once
#include "FunctionCall.h"
#include <stack>
class Stack
{
public:

    std::stack<FunctionCall*> _stack;


    FunctionCall* _entryFunc;
    FunctionCall* _currentFunc;

    Stack()
    {
        _entryFunc = nullptr;
        _currentFunc = nullptr;
    }

    virtual ~Stack()
    {
        /// TODO delete all list
    }

    FunctionCall* Peek()
    {
        return _currentFunc;
    }

    FunctionCall* Push(FunctionInfo* info)
    {
        //  ::MessageBox(0, L"Push", (info->_funcName).c_str(), 0);

        auto call = new FunctionCall(info);

        _stack.push(call);


        // For direct accessing the most often used properties.
        if (_entryFunc == nullptr)
        {
            // First call on this stack
            _entryFunc = call;

        }
        _currentFunc = call;

        /*   std::wstringstream stream;
           stream << L"\nAfter pushed: ";
           stream << "_entryFunc: ";
           stream << std::to_wstring((unsigned long)_entryFunc);
           stream << "_currentFunc: ";
           stream << std::to_wstring((unsigned long)_currentFunc);
           OutputDebugString(stream.str().c_str());*/


        return call;
    }

    void Pop()
    {
        /*  std::wstringstream stream;
          stream << L"\nBefore pop: ";
          stream << "_entryFunc: ";
          stream << std::to_wstring((unsigned long)_entryFunc);
          stream << "_currentFunc: ";
          stream << std::to_wstring((unsigned long)_currentFunc);
          OutputDebugString(stream.str().c_str());*/




          // Parent function is active again

        _stack.pop();

        if (_stack.empty())
        {
            _currentFunc = nullptr;
            _entryFunc = nullptr;
        }
        else
        {
            _currentFunc = _stack.top();
        }


        /*   std::wstringstream stream2;
           stream2 << L"\nAfter pop: ";
           stream2 << "_entryFunc: ";
           stream2 << std::to_wstring((unsigned long)_entryFunc);
           stream2 << "_currentFunc: ";
           stream2 << std::to_wstring((unsigned long)_currentFunc);
           OutputDebugString(stream2.str().c_str());*/


    }

};
